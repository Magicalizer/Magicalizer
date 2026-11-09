// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// HTTP method of the API.
/// </summary>
public enum HttpMethod
{
  /// <summary>
  /// Any HTTP method.
  /// </summary>
  Any,

  /// <summary>
  /// GET method (getting a model or a list of models).
  /// </summary>
  Get,

  /// <summary>
  /// POST method (creating a model).
  /// </summary>
  Post,

  /// <summary>
  /// PUT method (updating a model).
  /// </summary>
  Put,

  /// <summary>
  /// PATCH method (partially updating a model).
  /// </summary>
  Patch,

  /// <summary>
  /// DELETE method (deleting a model).
  /// </summary>
  Delete
}
