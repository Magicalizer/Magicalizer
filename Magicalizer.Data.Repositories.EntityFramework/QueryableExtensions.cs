// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Data.Repositories.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Magicalizer.Data.Repositories.EntityFramework.Extensions
{
  public static class QueryableExtensions
  {
    public static IQueryable<TEntity> ApplyFiltering<TEntity, TEntityFilter>(this IQueryable<TEntity> result, TEntityFilter filter) where TEntity : class, IEntity where TEntityFilter : class, IFilter
    {
      List<string> whereClauses = new List<string>();
      List<object> parameters = new List<object>();

      if (filter != null)
        CombineWhereClauses(filter, "{0}", new Indexer(), new Indexer(), whereClauses, parameters);

      foreach (string whereClause in whereClauses)
        result = result.Where(whereClause, parameters.ToArray());

      return result;
    }

    public static IQueryable<TEntity> ApplyInclusions<TEntity>(this IQueryable<TEntity> result, params Inclusion<TEntity>[] inclusions) where TEntity : class, IEntity
    {
      if (inclusions != null)
        inclusions.ToList().ForEach(i =>
        {
          if (i != null)
          {
            if (i.Property != null)
              result = result.Include(i.Property);

            else if (!string.IsNullOrEmpty(i.PropertyPath))
              result = result.Include(i.PropertyPath);
          }
        });

      return result;
    }

    public static IQueryable<TEntity> ApplySorting<TEntity>(this IQueryable<TEntity> result, string sorting) where TEntity : class, IEntity
    {
      if (!string.IsNullOrEmpty(sorting))
      {
        string[] criterions = sorting.Split(',');

        if (criterions.Length == 1)
          return result.OrderBy(ConvertSortingCriterion(criterions[0]));

        IOrderedQueryable<TEntity> orderedResult = null;

        for (int i = 0; i != criterions.Length; i++)
        {
            orderedResult = orderedResult == null ?
              result.OrderBy(ConvertSortingCriterion(criterions[i])) :
              orderedResult.ThenBy(ConvertSortingCriterion(criterions[i]));
        }

        return orderedResult;
      }

      return result;
    }

    public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> result, int? offset, int? limit) where TEntity : class, IEntity
    {
      if (offset != null && limit != null)
        return result.Skip((int)offset).Take((int)limit);

      return result;
    }

    private static void CombineWhereClauses<TEntityFilter>(TEntityFilter filter, string whereClauseTemplate, Indexer shortcutIndexer, Indexer parameterIndexer, List<string> whereClauses, List<object> parameters) where TEntityFilter : class, IFilter
    {
      foreach (PropertyInfo property in filter.GetType().GetProperties())
      {
        if (IsValue(property))
        {
          object value = property.GetValue(filter);

          if (value != null)
            CombineWhereClauseForValue(whereClauseTemplate, shortcutIndexer, parameterIndexer, whereClauses, parameters, property, value);
        }

        else if (IsFilter(property))
        {
          IFilter subFilter = property.GetValue(filter) as IFilter;

          if (subFilter != null && property.GetCustomAttribute<IgnoreFilterAttribute>() == null)
            CombineWhereClauseForFilter(whereClauseTemplate, shortcutIndexer, parameterIndexer, whereClauses, parameters, property, subFilter);
        }
      }
    }

    private static void CombineWhereClauseForValue(string whereClauseTemplate, Indexer shortcutIndexer, Indexer parameterIndexer, List<string> whereClauses, List<object> parameters, PropertyInfo property, object value)
    {
      FilterShortcutAttribute shortcutAttribute = property.GetCustomAttribute<FilterShortcutAttribute>();

      if (shortcutAttribute != null && !string.IsNullOrEmpty(shortcutAttribute.Path))
        whereClauseTemplate = string.Format(whereClauseTemplate, ComposeWhereClauseTemplateForShortcutAttributePath(shortcutIndexer, shortcutAttribute));

      string whereClause;

      if (property.Name == "IsNull")
      {
        whereClause = string.Format(whereClauseTemplate, $" = null");
        parameterIndexer.Value++;
      }

      else if (property.Name == "IsNotNull")
      {
        whereClause = string.Format(whereClauseTemplate, $" != null");
        parameterIndexer.Value++;
      }

      else if (property.Name == "Equals")
      {
        if (value is DateTime)
          whereClause = string.Format(whereClauseTemplate, $".Date = @{parameterIndexer.Value++}");

        else whereClause = string.Format(whereClauseTemplate, $" = @{parameterIndexer.Value++}");
      }

      else if (property.Name == "From")
        whereClause = string.Format(whereClauseTemplate, $" >= @{parameterIndexer.Value++}");

      else if (property.Name == "To")
        whereClause = string.Format(whereClauseTemplate, $" <= @{parameterIndexer.Value++}");

      else if (property.Name == "Contains")
        whereClause = string.Format(whereClauseTemplate, $".Contains(@{parameterIndexer.Value++})");

      else
      {
        if (whereClauseTemplate != "{0}")
          whereClauseTemplate = string.Format(whereClauseTemplate, ".{0}");

        whereClause = string.Format(whereClauseTemplate, $"{property.Name} = @{parameterIndexer.Value++}");
      }

      whereClauses.Add(whereClause);

      if (value is DateTime && property.Name == "Equals")
        parameters.Add(((DateTime)value).Date);

      else parameters.Add(value);
    }

    private static void CombineWhereClauseForFilter(string whereClauseTemplate, Indexer shortcutIndexer, Indexer parameterIndexer, List<string> whereClauses, List<object> parameters, PropertyInfo property, IFilter filter)
    {
      if (whereClauseTemplate != "{0}")
        whereClauseTemplate = string.Format(whereClauseTemplate, ".{0}");

      FilterShortcutAttribute shortcutAttribute = property.GetCustomAttribute<FilterShortcutAttribute>();

      if (shortcutAttribute == null || string.IsNullOrEmpty(shortcutAttribute.Path))
        whereClauseTemplate = string.Format(whereClauseTemplate, property.Name + "{0}");

      else whereClauseTemplate = string.Format(whereClauseTemplate, ComposeWhereClauseTemplateForShortcutAttributePath(shortcutIndexer, shortcutAttribute));

      CombineWhereClauses(filter, whereClauseTemplate, shortcutIndexer, parameterIndexer, whereClauses, parameters);
    }

    private static string ComposeWhereClauseTemplateForShortcutAttributePath(Indexer shortcutIndexer, FilterShortcutAttribute shortcutAttribute)
    {
      StringBuilder whereClauseTemplate = new StringBuilder();
      string[] clauses = shortcutAttribute.Path.Split("[]");

      for (int i = 0; i != clauses.Length; i++)
      {
        if (i != 0)
        {
          shortcutIndexer.Value++;
          whereClauseTemplate.Append($".Any(x{shortcutIndexer.Value} => x{shortcutIndexer.Value}");
        }

        whereClauseTemplate.Append(clauses[i]);
      }

      whereClauseTemplate.Append("{0}");
      whereClauseTemplate.Append(new string(')', clauses.Length - 1));
      return whereClauseTemplate.ToString();
    }

    private static bool IsValue(PropertyInfo property)
    {
      return property.PropertyType == typeof(bool?) ||
          property.PropertyType == typeof(byte?) ||
          property.PropertyType == typeof(short?) ||
          property.PropertyType == typeof(int?) ||
          property.PropertyType == typeof(long?) ||
          property.PropertyType == typeof(decimal?) ||
          property.PropertyType == typeof(float?) ||
          property.PropertyType == typeof(double?) ||
          property.PropertyType == typeof(Guid?) ||
          property.PropertyType == typeof(DateTime?) ||
          property.PropertyType == typeof(string);
    }

    private static bool IsFilter(PropertyInfo property)
    {
      return (typeof(IFilter).IsAssignableFrom(property.PropertyType));
    }

    private static string ConvertSortingCriterion(string criterion)
    {
      return criterion.Substring(1) + " " + (criterion.Remove(1) == "+" ? "ASC" : "DESC");
    }
  }
}