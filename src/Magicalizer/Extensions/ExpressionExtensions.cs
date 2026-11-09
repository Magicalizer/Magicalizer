// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;

namespace Magicalizer.Extensions;

/// <summary>
/// Extensions of <see cref="Expression"/>.
/// </summary>
internal static class ExpressionExtensions
{
  /// <summary>
  /// Gets the property path of the expression.
  /// </summary>
  /// <param name="property">Expression of the property.</param>
  /// <returns>Property path (e.g. "Category.Name").</returns>
  public static string GetPropertyPath(this Expression? property)
  {
    IList<string> propertyNames = [];

    if (property is LambdaExpression lambdaExpression)
      property = lambdaExpression.Body;

    if (property is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
      property = unaryExpression.Operand;

    while (property is MemberExpression expression)
    {
      propertyNames.Insert(0, expression.Member.Name);
      property = expression.Expression;
    }

    return string.Join(".", propertyNames);
  }
}
