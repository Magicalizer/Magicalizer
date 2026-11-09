// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Magicalizer.Data;

/// <summary>
/// Parameter of an entity query. A value is wrapped, so EF Core passes it as a parameter and translates the query once
/// for all values (a constant would be put into SQL, and each value would get its own translation in the cache).
/// </summary>
internal static class QueryParameter
{
  // Compiled factories of the boxes of the types, as the parameters are created for each query.
  private static readonly ConcurrentDictionary<Type, Func<object?, object>> BoxFactoriesByTypes = [];

  /// <summary>
  /// Creates the expression of the parameter with the value.
  /// </summary>
  /// <param name="value">Value of the parameter.</param>
  /// <param name="type">Type of the parameter (e.g. a nullable one to compare with a nullable property).</param>
  /// <returns>Expression of the parameter.</returns>
  public static Expression Create(object? value, Type type)
  {
    // A value of another type (e.g. an int for a long) is converted by the reflection, as it was before the factories.
    object box = value == null || type.IsInstanceOfType(value) ?
      BoxFactoriesByTypes.GetOrAdd(type, CompileBoxFactory)(value) :
      Activator.CreateInstance(typeof(StrongBox<>).MakeGenericType(type), value)!;

    return Expression.Field(Expression.Constant(box), nameof(StrongBox<object>.Value));
  }

  // Compiles the factory of the boxes of the type (e.g. "v => new StrongBox<int>(v == null ? default : (int)v)"), which
  // is much faster than Activator.CreateInstance.
  private static Func<object?, object> CompileBoxFactory(Type type)
  {
    ParameterExpression parameter = Expression.Parameter(typeof(object), "v");
    Expression typedValue = Expression.Condition(Expression.Equal(parameter, Expression.Constant(null)), Expression.Default(type), Expression.Convert(parameter, type));
    Expression body = Expression.New(typeof(StrongBox<>).MakeGenericType(type).GetConstructor([type])!, typedValue);

    return Expression.Lambda<Func<object?, object>>(body, parameter).Compile();
  }
}
