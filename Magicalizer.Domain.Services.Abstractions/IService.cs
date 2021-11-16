// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions
{
  /// <summary>
  /// Describes a service to manipulate the models.
  /// </summary>
  /// <typeparam name="TKey">A model's unique identifier type.</typeparam>
  /// <typeparam name="TModel">A model type.</typeparam>
  /// <typeparam name="TFilter">A model filter type.</typeparam>
  public interface IService<TKey, TModel, TFilter>
    where TModel : class, IModel
    where TFilter : class, IFilter
  {
    /// <summary>
    /// Gets a model by identifier.
    /// </summary>
    /// <param name="id">A model's identifier.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the model.</param>
    Task<TModel> GetByIdAsync(TKey id, params string[] inclusions);

    /// <summary>
    /// Gets the models.
    /// </summary>
    /// <param name="filter">A filter object to filter the models.</param>
    /// <param name="sorting">
    /// Comma-separated navigation property paths to sort the models by
    /// (user "+" and "-" to specify sorting direction; example: "+category.name,-position").
    /// </param>
    /// <param name="offset">Number of the models that should be skipped.</param>
    /// <param name="limit">Number of the models that should be returned.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the models.</param>
    Task<IEnumerable<TModel>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params string[] inclusions);

    /// <summary>
    /// Gets number of the models.
    /// </summary>
    /// <param name="filter">A filter object to filter the models.</param>
    Task<int> CountAsync(TFilter filter = null);

    /// <summary>
    /// Creates and returns a model.
    /// </summary>
    /// <param name="model">A model to create.</param>
    Task<TModel> CreateAsync(TModel model);

    /// <summary>
    /// Edits a model.
    /// </summary>
    /// <param name="model">A model to edit.</param>
    Task EditAsync(TModel model);

    /// <summary>
    /// Deletes a model.
    /// </summary>
    /// <param name="id">A model's identifier.</param>
    Task<bool> DeleteAsync(TKey id);
  }

  /// <summary>
  /// Describes a service to manipulate the models.
  /// </summary>
  /// <typeparam name="TKey1">The first model's unique identifier type.</typeparam>
  /// <typeparam name="TKey2">The second model's unique identifier type.</typeparam>
  /// <typeparam name="TModel">A model type.</typeparam>
  /// <typeparam name="TFilter">A model filter type.</typeparam>
  public interface IService<TKey1, TKey2, TModel, TFilter>
    where TModel : class, IModel
    where TFilter : class, IFilter
  {
    /// <summary>
    /// Gets a model by identifiers.
    /// </summary>
    /// <param name="id1">The first model's identifier.</param>
    /// <param name="id2">The second model's identifier.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the model.</param>
    Task<TModel> GetByIdAsync(TKey1 id1, TKey2 id2, params string[] inclusions);

    /// <summary>
    /// Gets the models.
    /// </summary>
    /// <param name="filter">A filter object to filter the models.</param>
    /// <param name="sorting">
    /// Comma-separated navigation property paths to sort the models by
    /// (user "+" and "-" to specify sorting direction; example: "+category.name,-position").
    /// </param>
    /// <param name="offset">Number of the models that should be skipped.</param>
    /// <param name="limit">Number of the models that should be returned.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the models.</param>
    Task<IEnumerable<TModel>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params string[] inclusions);

    /// <summary>
    /// Gets number of the models.
    /// </summary>
    /// <param name="filter">A filter object to filter the models.</param>
    Task<int> CountAsync(TFilter filter = null);

    /// <summary>
    /// Creates a model.
    /// </summary>
    /// <param name="model">A model to create.</param>
    Task<TModel> CreateAsync(TModel model);

    /// <summary>
    /// Edits a model.
    /// </summary>
    /// <param name="model">A model to edit.</param>
    Task EditAsync(TModel model);

    /// <summary>
    /// Deletes a model.
    /// </summary>
    /// <param name="id1">The first model's identifier.</param>
    /// <param name="id2">The second model's identifier.</param>
    Task<bool> DeleteAsync(TKey1 id1, TKey2 id2);
  }
}