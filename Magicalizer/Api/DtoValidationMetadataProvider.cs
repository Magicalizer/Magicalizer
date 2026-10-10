// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Magicalizer.Api;

/// <summary>
/// Disables the built-in DataAnnotations validation for DTOs, as they are validated by FluentValidation only
/// (see <see cref="FluentValidationFilterAttribute"/>). This covers the validation attributes and the implicit
/// [Required] that ASP.NET Core adds for non-nullable reference types, both on DTO properties and on action
/// parameters of DTO types. Other models of the application are not affected.
/// </summary>
public class DtoValidationMetadataProvider : IValidationMetadataProvider
{
  /// <inheritdoc />
  public void CreateValidationMetadata(ValidationMetadataProviderContext context)
  {
    // The DTO itself (e.g. an action parameter or a nested DTO property): no attributes, no implicit [Required], no children.
    if (typeof(IDto).IsAssignableFrom(context.Key.ModelType))
    {
      RemoveDataAnnotations(context);
      context.ValidationMetadata.ValidateChildren = false;
    }

    // A property declared on a DTO type.
    else if (context.Key.ContainerType != null && typeof(IDto).IsAssignableFrom(context.Key.ContainerType))
      RemoveDataAnnotations(context);
  }

  // Removes the validation attributes (including the implicit [Required]) from the metadata.
  private static void RemoveDataAnnotations(ValidationMetadataProviderContext context)
  {
    context.ValidationMetadata.IsRequired = false;

    for (int i = context.ValidationMetadata.ValidatorMetadata.Count - 1; i >= 0; i--)
      if (context.ValidationMetadata.ValidatorMetadata[i] is ValidationAttribute)
        context.ValidationMetadata.ValidatorMetadata.RemoveAt(i);
  }
}