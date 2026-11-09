// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Magicalizer.Api.ModelBinding;

/// <summary>
/// Provider of <see cref="InclusionBinder{TModel}"/> for the parameters of the
/// <see cref="IEnumerable{T}"/> of <see cref="IInclusion{TModel}"/> type.
/// </summary>
internal class InclusionBinderProvider : IModelBinderProvider
{
  /// <inheritdoc/>
  public IModelBinder? GetBinder(ModelBinderProviderContext context)
  {
    Type? modelType = context.Metadata.ModelType.GetEnumerableElementTypeArgument(typeof(IInclusion<>));

    if (modelType == null) return null;

    IOptions<MagicalizerOptions> options = context.Services.GetRequiredService<IOptions<MagicalizerOptions>>();

    return (IModelBinder)Activator.CreateInstance(typeof(InclusionBinder<>).MakeGenericType(modelType), options)!;
  }
}
