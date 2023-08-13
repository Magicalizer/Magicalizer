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
          {
            CombineWhereClauseForValue(whereClauseTemplate, shortcutIndexer, parameterIndexer, whereClauses, property);
            parameters.Add(ProcessValue(filter, property, value));
          }
        }

        else if (IsFilter(property))
        {
          IFilter subFilter = property.GetValue(filter) as IFilter;

          if (subFilter != null && property.GetCustomAttribute<IgnoreFilterAttribute>() == null)
            CombineWhereClauseForFilter(whereClauseTemplate, shortcutIndexer, parameterIndexer, whereClauses, parameters, property, subFilter);
        }
      }
    }

    private static void CombineWhereClauseForValue(string whereClauseTemplate, Indexer shortcutIndexer, Indexer parameterIndexer, List<string> whereClauses, PropertyInfo property)
    {
      FilterShortcutAttribute shortcutAttribute = property.GetCustomAttribute<FilterShortcutAttribute>();

      if (shortcutAttribute != null && !string.IsNullOrEmpty(shortcutAttribute.Path))
        whereClauseTemplate = string.Format(whereClauseTemplate, ComposeWhereClauseTemplateForShortcutAttributePath(shortcutIndexer, shortcutAttribute));

      string whereClause;

      if (property.Name == "IsNull")
        whereClause = CombineIsNullWhereClauseForValue(whereClauseTemplate, parameterIndexer);

      else if (property.Name == "IsNotNull")
        whereClause = CombineIsNotNullWhereClauseForValue(whereClauseTemplate, parameterIndexer);

      else if (property.Name == "Equals")
        whereClause = CombineEqualsWhereClauseForValue(whereClauseTemplate, parameterIndexer);

      else if (property.Name == "NotEquals")
        whereClause = CombineNotEqualsWhereClauseForValue(whereClauseTemplate, parameterIndexer);

      else if (property.Name == "From")
        whereClause = CombineFromWhereClauseForValue(whereClauseTemplate, parameterIndexer);

      else if (property.Name == "To")
        whereClause = CombineToWhereClauseForValue(whereClauseTemplate, parameterIndexer);

      else if (property.Name == "Contains")
        whereClause = CombineContainsWhereClauseForValue(whereClauseTemplate, parameterIndexer);

      else if (property.Name == "In")
        whereClause = CombineInWhereClauseForValue(whereClauseTemplate, parameterIndexer);

      else whereClause = CombineDefaultWhereClauseForValue(whereClauseTemplate, parameterIndexer, property);

      whereClauses.Add(whereClause);
    }

    private static string CombineIsNullWhereClauseForValue(string whereClauseTemplate, Indexer parameterIndexer)
    {
      parameterIndexer.Value++;
      return string.Format(whereClauseTemplate, $" = null");
    }

    private static string CombineIsNotNullWhereClauseForValue(string whereClauseTemplate, Indexer parameterIndexer)
    {
      parameterIndexer.Value++;
      return string.Format(whereClauseTemplate, $" != null");
    }

    private static string CombineEqualsWhereClauseForValue(string whereClauseTemplate, Indexer parameterIndexer)
    {
      return string.Format(whereClauseTemplate, $" = @{parameterIndexer.Value++}");
    }

    private static string CombineNotEqualsWhereClauseForValue(string whereClauseTemplate, Indexer parameterIndexer)
    {
      return string.Format(whereClauseTemplate, $" != @{parameterIndexer.Value++}");
    }

    private static string CombineFromWhereClauseForValue(string whereClauseTemplate, Indexer parameterIndexer)
    {
      return string.Format(whereClauseTemplate, $" >= @{parameterIndexer.Value++}");
    }

    private static string CombineToWhereClauseForValue(string whereClauseTemplate, Indexer parameterIndexer)
    {
      return string.Format(whereClauseTemplate, $" <= @{parameterIndexer.Value++}");
    }

    private static string CombineContainsWhereClauseForValue(string whereClauseTemplate, Indexer parameterIndexer)
    {
      return string.Format(whereClauseTemplate, $".Contains(@{parameterIndexer.Value++})");
    }

    private static string CombineInWhereClauseForValue(string whereClauseTemplate, Indexer parameterIndexer)
    {
      string lambdaOperator = " => ";
      string temp;

      if (whereClauseTemplate.Contains(lambdaOperator))
      {
        temp = whereClauseTemplate.Substring(
          whereClauseTemplate.LastIndexOf(lambdaOperator) + lambdaOperator.Length
        );

        temp = temp.Remove(temp.IndexOf('}') + 1);
      }

      else temp = whereClauseTemplate;

      return whereClauseTemplate.Replace(temp, $"@{parameterIndexer.Value++}.Contains({temp.Replace("{0}", string.Empty)})");
    }

    private static string CombineDefaultWhereClauseForValue(string whereClauseTemplate, Indexer parameterIndexer, PropertyInfo property)
    {
      if (whereClauseTemplate != "{0}")
        whereClauseTemplate = string.Format(whereClauseTemplate, ".{0}");

      return string.Format(whereClauseTemplate, $"{property.Name} = @{parameterIndexer.Value++}");
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

    private static object ProcessValue<TEntityFilter>(TEntityFilter filter, PropertyInfo property, object value)
    {
      if (property.Name != "In")
        return value;

      if (filter is ByteFilter)
        return value is string ids ? ids.Split(',').Select(id => byte.Parse(id)).ToList() : Array.Empty<byte>();

      if (filter is ShortFilter)
        return value is string ids ? ids.Split(',').Select(id => short.Parse(id)).ToList() : Array.Empty<short>();

      if (filter is IntegerFilter)
        return value is string ids ? ids.Split(',').Select(id => int.Parse(id)).ToList() : Array.Empty<int>();

      if (filter is LongFilter)
        return value is string ids ? ids.Split(',').Select(id => long.Parse(id)).ToList() : Array.Empty<long>();

      if (filter is DecimalFilter)
        return value is string ids ? ids.Split(',').Select(id => decimal.Parse(id)).ToList() : Array.Empty<decimal>();

      if (filter is FloatFilter)
        return value is string ids ? ids.Split(',').Select(id => float.Parse(id)).ToList() : Array.Empty<float>();

      if (filter is DoubleFilter)
        return value is string ids ? ids.Split(',').Select(id => double.Parse(id)).ToList() : Array.Empty<double>();

      if (filter is GuidFilter)
        return value is string ids ? ids.Split(',').Select(id => Guid.Parse(id)).ToList() : Array.Empty<Guid>();

      {
        return value is string ids ? ids.Split(',').ToList() : new string[] { };
      }
    }

    private static string ConvertSortingCriterion(string criterion)
    {
      return criterion.Substring(1) + " " + (criterion.Remove(1) == "+" ? "ASC" : "DESC");
    }
  }
}