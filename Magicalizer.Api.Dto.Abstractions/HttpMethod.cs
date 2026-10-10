// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Enumeration of supported HTTP methods.
/// </summary>
public enum HttpMethod
{
  /// <summary>
  /// Any HTTP method.
  /// </summary>
  Any,

  /// <summary>
  /// The GET method (getting a model or a list of models).
  /// </summary>
  Get,

  /// <summary>
  /// The POST method (creating a model).
  /// </summary>
  Post,

  /// <summary>
  /// The PUT method (updating a model).
  /// </summary>
  Put,

  /// <summary>
  /// The PATCH method (partially updating a model).
  /// </summary>
  Patch,

  /// <summary>
  /// The DELETE method (deleting a model).
  /// </summary>
  Delete
}