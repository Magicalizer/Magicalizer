// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Magicalizer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Expression"/> type.
/// </summary>
public static class ExpressionExtensions
{
  /// <summary>
  /// Converts an expression into a property path string.
  /// </summary>
  /// <param name="property">The expression representing the property.</param>
  /// <returns>A string representing the property path (e.g., "Category.Name").</returns>
  public static string GetPropertyPath(this Expression? property)
  {
    // if (body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
    //  body = unaryExpression.Operand;

    IList<string> propertyNames = [];

    if (property is LambdaExpression lambdaExpression)
      property = lambdaExpression.Body;

    while (property is MemberExpression expression)
    {
      propertyNames.Insert(0, expression.Member.Name);
      property = expression.Expression;
    }

    return string.Join(".", propertyNames);
  }
}