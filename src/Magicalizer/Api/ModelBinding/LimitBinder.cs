// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Magicalizer.Api.ModelBinding;

/// <summary>
/// Binder of the limit of a request (the maximum number of models to return) with
/// <see cref="MagicalizerOptions.MaxLimit"/>: a request without the limit gets the maximum number of models, and a
/// greater limit is a model state error of the parameter (as an invalid number is). Nothing is changed if the maximum
/// isn't set.
/// </summary>
internal class LimitBinder : IModelBinder
{
  private readonly int? maxLimit;

  /// <summary>
  /// Creates a binder.
  /// </summary>
  /// <param name="options">Options of Magicalizer.</param>
  public LimitBinder(IOptions<MagicalizerOptions> options)
  {
    this.maxLimit = options.Value.MaxLimit;
  }

  /// <inheritdoc/>
  public Task BindModelAsync(ModelBindingContext bindingContext)
  {
    ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

    if (valueProviderResult == ValueProviderResult.None || string.IsNullOrEmpty(valueProviderResult.FirstValue))
    {
      if (this.maxLimit != null)
        bindingContext.Result = ModelBindingResult.Success(this.maxLimit);

      return Task.CompletedTask;
    }

    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

    if (!int.TryParse(valueProviderResult.FirstValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int limit))
    {
      ModelMetadata metadata = bindingContext.ModelMetadata;

      bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, metadata.ModelBindingMessageProvider.AttemptedValueIsInvalidAccessor(valueProviderResult.FirstValue, metadata.DisplayName ?? bindingContext.ModelName));
      bindingContext.Result = ModelBindingResult.Failed();
    }

    else if (limit > this.maxLimit)
    {
      bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"The limit can't be greater than {this.maxLimit}.");
      bindingContext.Result = ModelBindingResult.Failed();
    }

    else bindingContext.Result = ModelBindingResult.Success(limit);

    return Task.CompletedTask;
  }
}
