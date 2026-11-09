// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;
using System.Reflection;

namespace Magicalizer.Mapping;

/// <summary>
/// Converts objects by the constructor of the target type that takes the source object (e.g. the DTO constructor that
/// takes the model). The call of the constructor is compiled once for each pair of the types, as it's faster than
/// reflection.
/// </summary>
/// <typeparam name="TSource">Type of the objects to convert.</typeparam>
/// <typeparam name="TTarget">Type of the converted objects.</typeparam>
internal static class ConstructorMapper<TSource, TTarget>
{
  // Call of the constructor, compiled when the pair of the types is used first.
  private static readonly Func<TSource, TTarget> Constructor = Compile();

  /// <summary>
  /// Converts the object.
  /// </summary>
  /// <param name="source">Object to convert.</param>
  /// <returns>Converted object.</returns>
  public static TTarget Map(TSource source)
  {
    return Constructor(source);
  }

  // Compiles the call of the constructor of the target type that takes the source object.
  private static Func<TSource, TTarget> Compile()
  {
    ConstructorInfo? constructor = typeof(TTarget).GetConstructor([typeof(TSource)]);

    if (constructor == null)
      throw new InvalidOperationException($"The type {typeof(TTarget).Name} must have a constructor that takes {typeof(TSource).Name}.");

    ParameterExpression parameter = Expression.Parameter(typeof(TSource), "source");

    return Expression.Lambda<Func<TSource, TTarget>>(Expression.New(constructor, parameter), parameter).Compile();
  }
}
