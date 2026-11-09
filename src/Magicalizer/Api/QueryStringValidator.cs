// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer.Api;

/// <summary>
/// Validates the query parameters of an action: each one must be a parameter of the action or a path of its filter
/// (see <see cref="FilterPath"/>), and the collection conditions must have consistent forms (see
/// <see cref="CollectionConditionValidator"/>).
/// </summary>
internal sealed class QueryStringValidator
{
  private readonly HashSet<string> parameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  private readonly List<(string Name, Type Type)> filterParameters = [];

  /// <summary>
  /// Creates a validator of the query parameters of the action.
  /// </summary>
  /// <param name="parameters">Parameters of the action.</param>
  public QueryStringValidator(IEnumerable<ControllerParameterDescriptor> parameters)
  {
    foreach (ControllerParameterDescriptor parameter in parameters)
    {
      BindingSource? bindingSource = parameter.BindingInfo?.BindingSource;

      // Only the parameters that can be bound from the query are allowed in it (e.g. not the route values or the body).
      if (bindingSource != null && !bindingSource.CanAcceptDataFrom(BindingSource.Query) || parameter.ParameterType == typeof(CancellationToken))
        continue;

      string name = parameter.BindingInfo?.BinderModelName ?? parameter.Name;

      if (typeof(IFilter).IsAssignableFrom(parameter.ParameterType))
        this.filterParameters.Add((name, parameter.ParameterType));

      else this.parameterNames.Add(name);
    }
  }

  /// <summary>
  /// Validates the names of the query parameters.
  /// </summary>
  /// <param name="keys">Names of the query parameters.</param>
  /// <returns>Errors by the names of the invalid parameters (or the collection paths).</returns>
  public IEnumerable<KeyValuePair<string, string>> Validate(IEnumerable<string> keys)
  {
    CollectionConditionValidator collectionConditionValidator = new CollectionConditionValidator();

    foreach (string key in keys)
    {
      if (this.parameterNames.Contains(key) || this.IsFilterPath(key, collectionConditionValidator))
        continue;

      yield return new KeyValuePair<string, string>(key, "The query parameter isn't supported.");
    }

    foreach (KeyValuePair<string, string> error in collectionConditionValidator.Validate())
      yield return error;
  }

  // Checks whether the query parameter is a path of a filter parameter (its name may be the prefix, e.g.
  // "filter.name"). The collection conditions of a valid path are added to the validator.
  private bool IsFilterPath(string key, CollectionConditionValidator collectionConditionValidator)
  {
    foreach ((string name, Type type) in this.filterParameters)
    {
      string path = key.StartsWith(name + ".", StringComparison.OrdinalIgnoreCase) ? key.Substring(name.Length + 1) : key;

      if (FilterPath.TryParse(type, path, out List<CollectionCondition> collectionConditions))
      {
        collectionConditionValidator.Add(collectionConditions);
        return true;
      }
    }

    return false;
  }
}
