// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Magicalizer.Api.Controllers;
using Magicalizer.Api.ModelBinding;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer.Api;

/// <summary>
/// Sets <see cref="LimitBinder"/> as the binder of the limit of the GET action of the magicalized controllers (even if
/// a custom controller overrides the action), so <see cref="MagicalizerOptions.MaxLimit"/> is applied to it.
/// </summary>
internal class MagicalizedControllerLimitConvention : IControllerModelConvention
{
  // Name of the limit parameter of the GET action.
  private const string LimitParameterName = "limit";

  /// <inheritdoc/>
  /// <remarks>
  /// It sets the binder of the limit parameter.
  /// </remarks>
  public void Apply(ControllerModel controller)
  {
    if (MagicalizedControllerConventionHelper.GetDtoType(controller.ControllerType) == null) return;

    foreach (ActionModel action in controller.Actions)
    {
      MethodInfo baseDefinition = action.ActionMethod.GetBaseDefinition();

      if (!IsGetAction(baseDefinition)) continue;

      // The parameter is found by its position in the base action, as an overriding action may rename it.
      int position = baseDefinition.GetParameters().Single(p => p.Name == LimitParameterName).Position;
      ParameterModel? limit = action.Parameters.FirstOrDefault(p => p.ParameterInfo.Position == position);

      if (limit == null) continue;

      limit.BindingInfo ??= new BindingInfo();
      limit.BindingInfo.BinderType = typeof(LimitBinder);
    }
  }

  // Checks whether the method is the GET action that gets the models (not the one that gets a model by its primary key).
  private static bool IsGetAction(MethodInfo baseDefinition)
  {
    Type? declaringType = baseDefinition.DeclaringType;

    return declaringType != null && declaringType.IsGenericType && declaringType.GetGenericTypeDefinition() == typeof(MagicalizedControllerBase<,,>) && baseDefinition.Name == nameof(MagicalizedControllerBase<,,>.GetAsync) && baseDefinition.IsPublic;
  }
}
