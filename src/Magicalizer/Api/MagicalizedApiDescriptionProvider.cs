// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Magicalizer.Api.ModelBinding;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer.Api;

/// <summary>
/// Describes query parameters of the magicalized controllers (and of custom controllers derived from them) as they are
/// passed, so API description tools (e.g. OpenAPI) show them correctly:
/// <list type="bullet">
///   <item>The names are in lower case (e.g. "name.contains"), as query parameters are case-insensitive.</item>
///   <item>
///     Conditions of collection elements (<see cref="EnumerableFilter{TFilter}.Any"/> and
///     <see cref="EnumerableFilter{TFilter}.None"/>) are described as parameters of the element filter (e.g.
///     "photos.any.filename.contains"), as <see cref="EnumerableFilterBinder"/> binds them, instead of an array of
///     objects. Only collections of the filter itself are described (not the ones of the entities it refers to), and
///     the element filter is expanded with the filters of the entities it refers to (e.g.
///     "photos.any.product.name.contains"), but not deeper and not with the ones already in the path, so cyclic
///     references can't produce endless parameters. The list form (e.g. "photos.any[0].filename.contains") isn't
///     described.
///   </item>
///   <item>
///     The sortings and inclusions (<see cref="SortingBinder{TModel}"/> and <see cref="InclusionBinder{TModel}"/>) are
///     described as strings of comma-separated property paths, as they are passed.
///   </item>
///   <item>
///     The conditions of a value filter are in the order of their declaration, from the base type (e.g.
///     "price.isnull" before "price.from").
///   </item>
///   <item>
///     The action that gets a list has the optional <c>Prefer</c> header (e.g. <c>Prefer: count=none</c>), which the
///     action reads itself, so it isn't its parameter (see <see cref="MagicalizerOptions.CountTotalByDefault"/>).
///   </item>
/// </list>
/// </summary>
internal class MagicalizedApiDescriptionProvider : IApiDescriptionProvider
{
  private readonly IModelMetadataProvider modelMetadataProvider;

  /// <inheritdoc/>
  /// <remarks>
  /// The provider runs after the default one of ASP.NET Core (-1000), which describes the parameters.
  /// </remarks>
  public int Order => -900;

  /// <summary>
  /// Creates a provider.
  /// </summary>
  /// <param name="modelMetadataProvider">Provider of metadata of the described parameters.</param>
  public MagicalizedApiDescriptionProvider(IModelMetadataProvider modelMetadataProvider)
  {
    this.modelMetadataProvider = modelMetadataProvider;
  }

  /// <inheritdoc/>
  /// <remarks>
  /// It does nothing, the parameters are changed after the default provider describes them.
  /// </remarks>
  public void OnProvidersExecuting(ApiDescriptionProviderContext context)
  {
  }

  /// <inheritdoc/>
  /// <remarks>
  /// It changes descriptions of query parameters of the magicalized controllers and adds the <c>Prefer</c> header to
  /// the action that gets a list.
  /// </remarks>
  public void OnProvidersExecuted(ApiDescriptionProviderContext context)
  {
    foreach (ApiDescription apiDescription in context.Results)
    {
      if (apiDescription.ActionDescriptor is not ControllerActionDescriptor action || MagicalizedControllerConventionHelper.GetDtoType(action.ControllerTypeInfo) == null)
        continue;

      List<ApiParameterDescription> parameters = [];

      foreach (ApiParameterDescription parameter in apiDescription.ParameterDescriptions)
      {
        Type? elementFilterType = parameter.Source == BindingSource.Query ? GetFilterListElementType(parameter.Type) : null;

        if (elementFilterType == null)
          parameters.Add(parameter);

        // Only collections of the filter itself are expanded (e.g. "photos.any"), the ones of the entities it refers to
        // (e.g. "category.products.any") would multiply parameters, so they aren't described.
        else if (parameter.Name.Count(c => c == '.') == 1)
          parameters.AddRange(this.Expand(parameter, parameter.Name, elementFilterType, [parameter.ParameterDescriptor?.ParameterType, elementFilterType], canExpandEntities: true));
      }

      apiDescription.ParameterDescriptions.Clear();

      foreach (ApiParameterDescription parameter in OrderConditions(parameters))
      {
        if (parameter.Source == BindingSource.Query)
        {
          parameter.Name = parameter.Name.ToLowerInvariant();

          if (IsPropertyPaths(parameter.Type))
          {
            parameter.Type = typeof(string);
            parameter.ModelMetadata = this.modelMetadataProvider.GetMetadataForType(typeof(string));
          }
        }

        apiDescription.ParameterDescriptions.Add(parameter);
      }

      if (MagicalizedControllerConventionHelper.IsListAction(action.MethodInfo))
        apiDescription.ParameterDescriptions.Add(this.DescribePreferHeader());
    }
  }

  // Gets the element filter type if the parameter is a list of filters: the conditions of collection elements (e.g.
  // Any).
  private static Type? GetFilterListElementType(Type? type)
  {
    Type? elementType = type?.GetCollectionElementType();

    return typeof(IFilter).IsAssignableFrom(elementType) ? elementType : null;
  }

  // Describes parameters of the filter: conditions of its value filters (e.g. "name.contains") and, if allowed, the
  // ones of the entity filters it refers to (e.g. "product.name.contains").
  private IEnumerable<ApiParameterDescription> Expand(ApiParameterDescription parameter, string prefix, Type filterType, HashSet<Type?> path, bool canExpandEntities)
  {
    foreach (PropertyInfo property in GetBindableFilterProperties(filterType))
    {
      string name = $"{prefix}.{property.Name}";

      if (!typeof(IFilter).IsAssignableFrom(property.PropertyType))
        yield return this.Describe(parameter, name, filterType, property);

      else if (property.PropertyType.IsValueFilter())
      {
        foreach (PropertyInfo condition in GetBindableFilterProperties(property.PropertyType))
          yield return this.Describe(parameter, $"{name}.{condition.Name}", property.PropertyType, condition);
      }

      // Collections of the element (e.g. photos of a product) aren't expanded, as they would multiply parameters.
      else if (canExpandEntities && !path.Contains(property.PropertyType) && !typeof(IEnumerableFilter).IsAssignableFrom(property.PropertyType))
      {
        foreach (ApiParameterDescription entityParameter in this.Expand(parameter, name, property.PropertyType, [.. path, property.PropertyType], canExpandEntities: false))
          yield return entityParameter;
      }
    }
  }

  // Gets filter properties that can be bound (ignored ones aren't applied, so they aren't described).
  private static IEnumerable<PropertyInfo> GetBindableFilterProperties(Type filterType)
  {
    return filterType.GetFilterProperties().Where(p => p.CanWrite);
  }

  // Describes a query parameter of a filter property, with the metadata of the property.
  private ApiParameterDescription Describe(ApiParameterDescription parameter, string name, Type containerType, PropertyInfo property)
  {
    return new ApiParameterDescription {
      Name = name,
      Type = property.PropertyType,
      Source = BindingSource.Query,
      ModelMetadata = this.modelMetadataProvider.GetMetadataForProperty(containerType, property.Name),
      ParameterDescriptor = parameter.ParameterDescriptor
    };
  }

  // Orders the conditions of each filter property (e.g. "price.isnull" and "price.from") as they are declared in the
  // hierarchy of the value filter, from the base type, while the reflection lists the properties of the derived type
  // first. The properties keep their order, as their conditions are grouped by the property path.
  private static IEnumerable<ApiParameterDescription> OrderConditions(IEnumerable<ApiParameterDescription> parameters)
  {
    return parameters
      .GroupBy(p => p.Name.Contains('.') ? p.Name[..p.Name.LastIndexOf('.')] : p.Name, StringComparer.OrdinalIgnoreCase)
      .SelectMany(g => g.OrderBy(p => GetDepth(p.ModelMetadata?.ContainerType?.GetPublicProperty(p.ModelMetadata.PropertyName!)?.DeclaringType)));
  }

  // Gets the number of the base types of the type (0 if there is no type).
  private static int GetDepth(Type? type)
  {
    int depth = 0;

    for (type = type?.BaseType; type != null; type = type.BaseType)
      depth++;

    return depth;
  }

  // Checks whether the type is bound from comma-separated property paths (see SortingBinder and InclusionBinder).
  private static bool IsPropertyPaths(Type? type)
  {
    return type?.GetEnumerableElementTypeArgument(typeof(ISorting<>)) != null || type?.GetEnumerableElementTypeArgument(typeof(IInclusion<>)) != null;
  }

  // Describes the optional Prefer header of the action that gets a list.
  private ApiParameterDescription DescribePreferHeader()
  {
    return new ApiParameterDescription {
      Name = PreferHeader.Name,
      Source = BindingSource.Header,
      Type = typeof(string),
      ModelMetadata = this.modelMetadataProvider.GetMetadataForType(typeof(string)),
      IsRequired = false
    };
  }
}
