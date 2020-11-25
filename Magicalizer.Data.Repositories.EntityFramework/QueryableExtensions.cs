// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
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
        CombineWhereClauses(filter, "{0}", new ParameterIndex(), whereClauses, parameters);

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

        if (criterions.Length == 2)
          return result.OrderBy(ConvertSortingCriterion(criterions[0]))
            .ThenBy(ConvertSortingCriterion(criterions[1]));

        return result.OrderBy(ConvertSortingCriterion(criterions[0]))
            .ThenBy(ConvertSortingCriterion(criterions[1]))
            .ThenBy(ConvertSortingCriterion(criterions[2]));
      }

      return result;
    }

    public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> result, int? offset, int? limit) where TEntity : class, IEntity
    {
      if (offset != null && limit != null)
        return result.Skip((int)offset).Take((int)limit);

      return result;
    }

    private static void CombineWhereClauses<TEntityFilter>(TEntityFilter filter, string whereClauseTemplate, ParameterIndex parameterIndex, List<string> whereClauses, List<object> parameters) where TEntityFilter : class, IFilter
    {
      foreach (PropertyInfo property in filter.GetType().GetProperties())
      {
        if (IsValue(property))
        {
          object value = property.GetValue(filter);

          if (value != null)
            CombineWhereClauseForValue(whereClauseTemplate, parameterIndex, whereClauses, parameters, property, value);
        }

        else if (IsFilter(property))
        {
          IFilter subFilter = property.GetValue(filter) as IFilter;

          if (subFilter != null && property.GetCustomAttribute<IgnoreFilterAttribute>() == null)
            CombineWhereClauseForFilter(whereClauseTemplate, parameterIndex, whereClauses, parameters, property, subFilter);
        }
      }
    }

    private static void CombineWhereClauseForValue(string whereClauseTemplate, ParameterIndex parameterIndex, List<string> whereClauses, List<object> parameters, PropertyInfo property, object value)
    {
      FilterShortcutAttribute shortcutAttribute = property.GetCustomAttribute<FilterShortcutAttribute>();

      if (shortcutAttribute != null && !string.IsNullOrEmpty(shortcutAttribute.Path))
        whereClauseTemplate = string.Format(whereClauseTemplate, ComposeWhereClauseTemplateForShortcutAttributePath(shortcutAttribute));

      string whereClause;

      if (property.Name == "IsNull")
      {
        whereClause = string.Format(whereClauseTemplate, $" = null");
        parameterIndex.Value++;
      }

      else if (property.Name == "IsNotNull")
      {
        whereClause = string.Format(whereClauseTemplate, $" != null");
        parameterIndex.Value++;
      }

      else if (property.Name == "Equals")
      {
        if (value is DateTime)
          whereClause = string.Format(whereClauseTemplate, $".Date = @{parameterIndex.Value++}");

        else whereClause = string.Format(whereClauseTemplate, $" = @{parameterIndex.Value++}");
      }

      else if (property.Name == "From")
        whereClause = string.Format(whereClauseTemplate, $" >= @{parameterIndex.Value++}");

      else if (property.Name == "To")
        whereClause = string.Format(whereClauseTemplate, $" <= @{parameterIndex.Value++}");

      else if (property.Name == "Contains")
        whereClause = string.Format(whereClauseTemplate, $".Contains(@{parameterIndex.Value++})");

      else
      {
        if (whereClauseTemplate != "{0}")
          whereClauseTemplate = string.Format(whereClauseTemplate, ".{0}");

        whereClause = string.Format(whereClauseTemplate, $"{property.Name} = @{parameterIndex.Value++}");
      }

      whereClauses.Add(whereClause);

      if (value is DateTime && property.Name == "Equals")
        parameters.Add(((DateTime)value).Date);

      else parameters.Add(value);
    }

    private static void CombineWhereClauseForFilter(string whereClauseTemplate, ParameterIndex parameterIndex, List<string> whereClauses, List<object> parameters, PropertyInfo property, IFilter filter)
    {
      if (whereClauseTemplate != "{0}")
        whereClauseTemplate = string.Format(whereClauseTemplate, ".{0}");

      FilterShortcutAttribute shortcutAttribute = property.GetCustomAttribute<FilterShortcutAttribute>();

      if (shortcutAttribute == null || string.IsNullOrEmpty(shortcutAttribute.Path))
        whereClauseTemplate = string.Format(whereClauseTemplate, property.Name + "{0}");

      else whereClauseTemplate = string.Format(whereClauseTemplate, ComposeWhereClauseTemplateForShortcutAttributePath(shortcutAttribute));

      CombineWhereClauses(filter, whereClauseTemplate, parameterIndex, whereClauses, parameters);
    }

    private static string ComposeWhereClauseTemplateForShortcutAttributePath(FilterShortcutAttribute shortcutAttribute)
    {
      string whereClauseTemplate = string.Empty;
      string[] clauses = shortcutAttribute.Path.Split("[]");
      int argumentIndex = 0;

      for (int i = 0; i != clauses.Length; i++, argumentIndex++)
      {
        if (i != 0)
          whereClauseTemplate += $".Any(x{argumentIndex} => x{argumentIndex}";

        whereClauseTemplate += clauses[i];
      }

      whereClauseTemplate += "{0}";

      for (int i = 0; i != clauses.Length - 1; i++)
        whereClauseTemplate += ')';

      return whereClauseTemplate;
    }

    private static bool IsValue(PropertyInfo property)
    {
      return property.PropertyType == typeof(bool?) ||
          property.PropertyType == typeof(int?) ||
          property.PropertyType == typeof(decimal?) ||
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