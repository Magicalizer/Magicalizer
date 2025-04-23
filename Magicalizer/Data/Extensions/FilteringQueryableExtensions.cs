//// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Provides extension method for applying filtering to queryable entities based on filter objects.
/// </summary>
public static class FilteringQueryableExtensions
{
  /// <summary>
  /// Applies filtering to a queryable result based on the provided filter object.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <typeparam name="TEntityFilter">The type of the filter object.</typeparam>
  /// <param name="result">The queryable entity set to apply the filter to.</param>
  /// <param name="filter">The filter object used to apply conditions to the query.</param>
  /// <returns>The filtered queryable result.</returns>
  public static IQueryable<TEntity> ApplyFiltering<TEntity, TEntityFilter>(this IQueryable<TEntity> result, TEntityFilter filter) where TEntity : class, IEntity where TEntityFilter : class, IFilter
  {
    if (filter == null) return result;

    ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "e");
    Expression? filterExpression = BuildFilterExpression(filter, parameter, []);

    if (filterExpression != null)
      result = result.Where(Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter));

    return result;
  }

  // Builds the filter expression based on the filter properties.
  private static Expression? BuildFilterExpression(IFilter filter, ParameterExpression parameter, IList<string> propertyPath)
  {
    Expression? filterExpression = null;

    foreach (PropertyInfo property in filter.GetType().GetProperties())
    {
      if (property.IsDefined(typeof(IgnoreFilterAttribute), inherit: false)) continue;

      object? propertyValue = property.GetValue(filter);

      if (propertyValue == null) continue;

      if (IsValue(property))
        filterExpression = CombineExpressions(filterExpression, BuildValueExpression(parameter, propertyPath, property, propertyValue)!);

      else if (IsEnumerableFilter(property))
        filterExpression = CombineExpressions(filterExpression, BuildEnumerableFilterExpression(parameter, propertyPath, property, propertyValue)!);

      else if (IsFilter(property))
        filterExpression = CombineExpressions(filterExpression, BuildFilterExpression(parameter, propertyPath, property, propertyValue)!);
    }

    return filterExpression;
  }

  // Combines multiple filter expressions using "AND" logic.
  private static Expression CombineExpressions(Expression? left, Expression right)
  {
    return left == null ? right : Expression.AndAlso(left, right);
  }

  // Builds an expression to compare the property value with the filter criteria (e.g., equals, contains).
  private static Expression? BuildValueExpression(ParameterExpression parameter, IList<string> propertyPath, PropertyInfo property, object propertyValue)
  {
    Expression propertyExpression = BuildPropertyExpression(parameter, propertyPath);
    Expression? comparisonExpression = BuildComparisonExpression(propertyExpression, property.Name, propertyValue);

    return comparisonExpression == null ? null : comparisonExpression;
  }

  // Builds an expression for enumerable filters (e.g., "Any" or "None" in a collection).
  private static Expression? BuildEnumerableFilterExpression(ParameterExpression parameter, IList<string> propertyPath, PropertyInfo property, object propertyValue)
  {
    PropertyInfo? anyProperty = property.PropertyType.GetProperty("Any");
    PropertyInfo? noneProperty = property.PropertyType.GetProperty("None");
    object? anyPropertyValue = anyProperty?.GetValue(propertyValue);
    object? nonePropertyValue = noneProperty?.GetValue(propertyValue);

    if (anyPropertyValue == null && nonePropertyValue == null) return null;

    Expression enumerableExpression = BuildPropertyExpression(parameter, [.. propertyPath, property.Name]);
    PropertyInfo? entityNavigationProperty = parameter.Type.GetProperty(property.Name);

    if (entityNavigationProperty == null || !entityNavigationProperty.PropertyType.IsGenericType || entityNavigationProperty.PropertyType.GetGenericArguments().Length == 0) return null;

    Type entityType = entityNavigationProperty.PropertyType.GetGenericArguments()[0];

    parameter = Expression.Parameter(entityType, "e");

    IFilter? filter = (anyPropertyValue ?? nonePropertyValue) as IFilter;
    Expression? filterExpression = filter == null ? null : BuildFilterExpression(filter, parameter, []);

    if (filterExpression == null) return null;

    LambdaExpression filterLambda = Expression.Lambda(filterExpression, parameter);
    MethodInfo anyMethod = typeof(Enumerable).GetMethods()
      .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
      .MakeGenericMethod(entityType);

    Expression any = Expression.Call(anyMethod, enumerableExpression, filterLambda);

    if (nonePropertyValue != null)
      any = Expression.Not(any);

    return any;
  }

  // Recursively builds filter expressions for nested filters.
  private static Expression? BuildFilterExpression(ParameterExpression parameter, IList<string> propertyPath, PropertyInfo property, object propertyValue)
  {
    IFilter? filter = propertyValue as IFilter;

    if (filter == null) return null;

    return BuildFilterExpression(filter, parameter, [.. propertyPath, property.Name]);
  }

  // Builds an expression for the given property path.
  private static Expression BuildPropertyExpression(ParameterExpression parameter, IList<string> propertyPath)
  {
    Expression expression = parameter;

    foreach (string property in propertyPath)
      expression = Expression.PropertyOrField(expression, property);

    return expression;
  }

  // Builds a comparison expression based on the filter criterion (e.g., Equals, From, To).
  private static Expression? BuildComparisonExpression(Expression propertyExpression, string criterionName, object propertyValue)
  {
    Expression propertyValueExpression = Expression.Constant(propertyValue, propertyValue.GetType());

    return criterionName switch
    {
      "IsNull" => Expression.Equal(propertyExpression, Expression.Constant(null)),
      "IsNotNull" => Expression.NotEqual(propertyExpression, Expression.Constant(null)),
      "Equals" => Expression.Equal(propertyExpression, propertyValueExpression),
      "NotEquals" => Expression.NotEqual(propertyExpression, propertyValueExpression),
      "From" => Expression.GreaterThanOrEqual(propertyExpression, propertyValueExpression),
      "To" => Expression.LessThanOrEqual(propertyExpression, propertyValueExpression),
      "Contains" => BuildContainsExpression(propertyExpression, propertyValue),
      "In" => BuildInExpression(propertyExpression, propertyValue),
      _ => null
    };
  }

  // Builds a "Contains" expression for string filtering.
  private static MethodCallExpression? BuildContainsExpression(Expression propertyExpression, object propertyValue)
  {
    if (propertyValue is not string) return null;

    MethodInfo? containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);

    if (containsMethod == null) return null;

    return Expression.Call(propertyExpression, containsMethod, Expression.Constant(propertyValue));
  }

  // Builds an "In" expression for filtering based on a list of values.
  private static MethodCallExpression? BuildInExpression(Expression propertyExpression, object propertyValue)
  {
    if (propertyValue is not IEnumerable propertyValues) return null;

    MethodInfo? containsMethod = typeof(Enumerable).GetMethods()
      .FirstOrDefault(m => m.Name == "Contains" && m.GetParameters().Length == 2)
      ?.MakeGenericMethod(propertyExpression.Type);

    if (containsMethod == null) return null;

    return Expression.Call(containsMethod, Expression.Constant(propertyValues), propertyExpression);
  }

  // Determines if the property is a value type that can be used in comparisons.
  private static bool IsValue(PropertyInfo property)
  {
    Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

    return new Type[] {
      typeof(bool), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(string), typeof(Guid), typeof(DateTime),
      typeof(IEnumerable<byte>), typeof(IEnumerable<short>), typeof(IEnumerable<int>), typeof(IEnumerable<long>), typeof(IEnumerable<float>), typeof(IEnumerable<double>), typeof(IEnumerable<decimal>), typeof(IEnumerable<string>), typeof(IEnumerable<Guid>),
    }.Contains(type);
  }

  // Checks if the property is an enumerable filter.
  private static bool IsEnumerableFilter(PropertyInfo property)
  {
    return property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(EnumerableFilter<>);
  }

  // Checks if the property is a filter object.
  private static bool IsFilter(PropertyInfo property)
  {
    return typeof(IFilter).IsAssignableFrom(property.PropertyType);
  }
}