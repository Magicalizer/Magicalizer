// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Enumeration of supported HTTP methods.
/// </summary>
public enum HttpMethod
{
  Any,
  Get,
  Post,
  Put,
  Patch,
  Delete
}