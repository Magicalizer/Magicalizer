// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Extensions;
using Magicalizer.Domain;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Magicalizer.Api.ModelBinding;

/// <summary>
/// Binder of the inclusions of models from comma-separated property paths (e.g. "category,photos"), see
/// <see cref="PropertyPathsBinderBase{TModel, TItem}"/>. The property paths are also checked against the
/// navigations in the database context: a path can't make a cycle or be deeper than
/// <see cref="MagicalizerOptions.MaxInclusionDepth"/>.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
internal class InclusionBinder<TModel> : PropertyPathsBinderBase<TModel, IInclusion<TModel>> where TModel : class, IModel
{
  private readonly int? maxDepth;

  /// <summary>
  /// Creates a binder.
  /// </summary>
  /// <param name="options">Options of Magicalizer.</param>
  public InclusionBinder(IOptions<MagicalizerOptions> options)
  {
    this.maxDepth = options.Value.MaxInclusionDepth;
  }

  /// <inheritdoc/>
  protected override IInclusion<TModel> Parse(string value)
  {
    return new Inclusion<TModel>(value);
  }

  /// <inheritdoc/>
  protected override string? GetError(ModelBindingContext bindingContext, Type entityType, IInclusion<TModel> item)
  {
    DbContext? dbContext = bindingContext.HttpContext.RequestServices.GetService<DbContext>();

    return InclusionQueryableExtensions.GetError(entityType, item.PropertyPath, dbContext?.Model, this.maxDepth);
  }
}
