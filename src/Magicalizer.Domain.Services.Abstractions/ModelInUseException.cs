// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Exception that is thrown if a model can't be deleted, as the database restricts it (e.g. other models reference it).
/// The API returns 409 Conflict for it.
/// </summary>
public class ModelInUseException : Exception
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
  /// <param name="innerException">Database exception that caused this one.</param>
  public ModelInUseException(Type modelType, object?[] keyValues, Exception? innerException = null)
    : base($"The model with the primary key ({string.Join(", ", keyValues)}) can't be deleted, as the database restricts it (e.g. other models refer to it).", innerException)
  {
    this.ModelType = modelType;
    this.KeyValues = keyValues;
  }
}
