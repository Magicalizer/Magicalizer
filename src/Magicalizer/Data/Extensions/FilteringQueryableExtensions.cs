// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Extensions of queries to apply filters.
/// </summary>
public static class FilteringQueryableExtensions
{
  private static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
  private static readonly MethodInfo StringStartsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
  private static readonly MethodInfo StringEndsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!;
  private static readonly MethodInfo EnumerableAnyNoPredicateMethod = typeof(Enumerable).GetMethods().First(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 1);
  private static readonly MethodInfo EnumerableAnyMethod = typeof(Enumerable).GetMethods().First(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2);
  private static readonly MethodInfo EnumerableContainsMethod = typeof(Enumerable).GetMethods().First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

  // Scalar types of the conditions of value filters (e.g. IntegerFilter.From), the enums are scalar too.
  private static readonly HashSet<Type> ScalarTypes = [
    typeof(bool), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(string), typeof(Guid), typeof(DateTime), typeof(DateTimeOffset)
  ];

  /// <summary>
  /// Applies the filter to the query. Only the conditions that are set are applied.
  /// </summary>
  /// <typeparam name="TEntity">Entity type.</typeparam>
  /// <typeparam name="TFilter">Filter type.</typeparam>
  /// <param name="query">Query to apply the filter to.</param>
  /// <param name="filter">Filter to apply.</param>
  /// <returns>Filtered query.</returns>
  public static IQueryable<TEntity> ApplyFiltering<TEntity, TFilter>(this IQueryable<TEntity> query, TFilter? filter)
    where TEntity : class, IEntity
    where TFilter : class, IFilter
  {
    if (filter == null) return query;

    ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "e");
    Expression? filterExpression = BuildFilterExpression(filter, parameter, []);

    if (filterExpression != null)
      query = query.Where(Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter));

    return query;
  }

  // Builds the expression of the filter from the conditions and the nested filters that are set (null if none is set).
  // The properties of the filter are applied to the property at the path (the entity itself for an empty path).
  private static Expression? BuildFilterExpression(IFilter filter, ParameterExpression parameter, IList<string> propertyPath)
  {
    Expression? filterExpression = null;

    foreach (PropertyInfo property in filter.GetType().GetFilterProperties())
    {
      object? propertyValue = property.GetValueGetter()(filter);

      if (propertyValue == null) continue;

      if (IsValue(property))
        filterExpression = CombineExpressions(filterExpression, BuildConditionExpression(BuildPropertyExpression(parameter, propertyPath), property.Name, propertyValue));

      else if (IsEnumerableFilter(property))
      {
        IEnumerableFilter enumerableFilter = (IEnumerableFilter)propertyValue;
        IList<string> enumerablePath = [.. propertyPath, property.Name];

        if (enumerableFilter.IsEmpty != null)
          filterExpression = CombineExpressions(filterExpression, BuildEnumerableEmptinessExpression(parameter, enumerablePath, enumerableFilter.IsEmpty.Value));

        foreach (IFilter anyFilter in enumerableFilter.Any ?? [])
          filterExpression = CombineExpressions(filterExpression, BuildEnumerableFilterExpression(parameter, enumerablePath, anyFilter, isNone: false));

        foreach (IFilter noneFilter in enumerableFilter.None ?? [])
          filterExpression = CombineExpressions(filterExpression, BuildEnumerableFilterExpression(parameter, enumerablePath, noneFilter, isNone: true));
      }

      else if (IsFilter(property))
        filterExpression = CombineExpressions(filterExpression, BuildFilterExpression((IFilter)propertyValue, parameter, [.. propertyPath, property.Name]));
    }

    return filterExpression;
  }

  // Checks whether the property is a value that can be compared (a list condition, e.g. "In", is one if its elements are).
  private static bool IsValue(PropertyInfo property)
  {
    Type type = property.PropertyType;

    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
      type = type.GetGenericArguments()[0];

    type = Nullable.GetUnderlyingType(type) ?? type;
    return type.IsEnum || ScalarTypes.Contains(type);
  }

  // Combines the expressions with "AND" (an expression that isn't built is skipped).
  private static Expression? CombineExpressions(Expression? left, Expression? right)
  {
    if (left == null) return right;

    return right == null ? left : Expression.AndAlso(left, right);
  }

  // Builds the expression of a condition of a value filter (e.g. Equals, From or Contains) on the property, or returns
  // null if the condition is unknown.
  private static Expression? BuildConditionExpression(Expression propertyExpression, string conditionName, object value)
  {
    return conditionName switch {
      "IsNull" => BuildNullExpression(propertyExpression, isNull: value is true),
      "Equals" => Expression.Equal(propertyExpression, BuildParameterExpression(propertyExpression, value)),
      "NotEquals" => Expression.NotEqual(propertyExpression, BuildParameterExpression(propertyExpression, value)),
      "From" => Expression.GreaterThanOrEqual(propertyExpression, BuildParameterExpression(propertyExpression, value)),
      "To" => Expression.LessThanOrEqual(propertyExpression, BuildParameterExpression(propertyExpression, value)),
      "Contains" => BuildStringExpression(propertyExpression, StringContainsMethod, value),
      "StartsWith" => BuildStringExpression(propertyExpression, StringStartsWithMethod, value),
      "EndsWith" => BuildStringExpression(propertyExpression, StringEndsWithMethod, value),
      "In" => BuildInExpression(propertyExpression, value, isNotIn: false),
      "NotIn" => BuildInExpression(propertyExpression, value, isNotIn: true),
      _ => null
    };
  }

  // Builds the expression that checks whether the value is null (or isn't). A value of a non-nullable type is never null.
  private static Expression BuildNullExpression(Expression propertyExpression, bool isNull)
  {
    if (propertyExpression.Type.IsValueType && Nullable.GetUnderlyingType(propertyExpression.Type) == null)
      return Expression.Constant(!isNull);

    Expression nullExpression = Expression.Constant(null, propertyExpression.Type);

    return isNull ? Expression.Equal(propertyExpression, nullExpression) : Expression.NotEqual(propertyExpression, nullExpression);
  }

  // Builds the query parameter of the value to compare the property with. A value of a nullable property is a nullable
  // one too.
  private static Expression BuildParameterExpression(Expression propertyExpression, object value)
  {
    Type type = Nullable.GetUnderlyingType(propertyExpression.Type) == value.GetType() ? propertyExpression.Type : value.GetType();

    return QueryParameter.Create(value, type);
  }

  // Builds the expression of a string condition (e.g. "Contains"). The strings aren't converted (e.g. lowered), so the
  // database compares them by its collation, as it does for the other conditions, and can use the indexes.
  private static MethodCallExpression? BuildStringExpression(Expression propertyExpression, MethodInfo method, object value)
  {
    if (value is not string text) return null;

    return Expression.Call(propertyExpression, method, QueryParameter.Create(text, typeof(string)));
  }

  // Builds the "In" (or "NotIn") expression of a list of values. The values are copied to a list of the property type
  // (e.g. of nullable ones for a nullable property).
  private static Expression? BuildInExpression(Expression propertyExpression, object value, bool isNotIn)
  {
    if (value is not IEnumerable propertyValues) return null;

    IList values = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(propertyExpression.Type))!;

    foreach (object? propertyValue in propertyValues)
      values.Add(propertyValue);

    Expression inExpression = Expression.Call(
      EnumerableContainsMethod.MakeGenericMethod(propertyExpression.Type),
      QueryParameter.Create(values, typeof(IEnumerable<>).MakeGenericType(propertyExpression.Type)),
      propertyExpression
    );

    return isNotIn ? Expression.Not(inExpression) : inExpression;
  }

  // Builds the expression of the property path.
  private static Expression BuildPropertyExpression(ParameterExpression parameter, IList<string> propertyPath)
  {
    Expression expression = parameter;

    foreach (string property in propertyPath)
      expression = Expression.PropertyOrField(expression, property);

    return expression;
  }

  // Checks whether the property is a collection filter.
  private static bool IsEnumerableFilter(PropertyInfo property)
  {
    return typeof(IEnumerableFilter).IsAssignableFrom(property.PropertyType);
  }

  // Builds the expression that checks whether the collection at the path is empty (or isn't).
  private static Expression? BuildEnumerableEmptinessExpression(ParameterExpression parameter, IList<string> enumerablePath, bool isEmpty)
  {
    Expression enumerableExpression = BuildPropertyExpression(parameter, enumerablePath);
    Type? elementType = enumerableExpression.Type.GetCollectionElementType();

    if (elementType == null) return null;

    Expression anyExpression = Expression.Call(EnumerableAnyNoPredicateMethod.MakeGenericMethod(elementType), enumerableExpression);

    return isEmpty ? Expression.Not(anyExpression) : anyExpression;
  }

  // Builds the expression of a condition of the collection at the path ("Any" or "None") with the element filter.
  private static Expression? BuildEnumerableFilterExpression(ParameterExpression parameter, IList<string> enumerablePath, IFilter elementFilter, bool isNone)
  {
    Expression enumerableExpression = BuildPropertyExpression(parameter, enumerablePath);
    Type? elementType = enumerableExpression.Type.GetCollectionElementType();

    if (elementType == null) return null;

    ParameterExpression elementParameter = Expression.Parameter(elementType, "e");
    Expression? elementFilterExpression = BuildFilterExpression(elementFilter, elementParameter, []);

    if (elementFilterExpression == null) return null;

    Expression anyExpression = Expression.Call(EnumerableAnyMethod.MakeGenericMethod(elementType), enumerableExpression, Expression.Lambda(elementFilterExpression, elementParameter));

    return isNone ? Expression.Not(anyExpression) : anyExpression;
  }

  // Checks whether the property is a filter.
  private static bool IsFilter(PropertyInfo property)
  {
    return typeof(IFilter).IsAssignableFrom(property.PropertyType);
  }
}
