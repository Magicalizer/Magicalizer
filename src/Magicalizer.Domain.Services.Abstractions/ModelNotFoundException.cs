// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Exception that is thrown if a model to edit or delete doesn't exist (or isn't available through the query
/// prefilters). The API returns 404 Not Found for it.
/// </summary>
public class ModelNotFoundException : Exception
{
  /// <summary>
  /// Model type.
  /// </summary>
  public Type ModelType { get; }

  /// <summary>
  /// Primary key values of the model.
  /// </summary>
  public object?[] KeyValues { get; }

  /// <summary>
  /// Creates an exception.
  /// </summary>
  /// <param name="modelType">Model type.</param>
  /// <param name="keyValues">Primary key values of the model.</param>
  /// <param name="innerException">
  /// Exception that caused this one (e.g. the database one if the model is deleted concurrently).
  /// </param>
  public ModelNotFoundException(Type modelType, object?[] keyValues, Exception? innerException = null)
    : base($"The model with the primary key ({string.Join(", ", keyValues)}) doesn't exist.", innerException)
  {
    this.ModelType = modelType;
    this.KeyValues = keyValues;
  }
}
