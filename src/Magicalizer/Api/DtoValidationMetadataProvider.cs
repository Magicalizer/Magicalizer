// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.ComponentModel.DataAnnotations;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Magicalizer.Api;

/// <summary>
/// Disables the built-in DataAnnotations validation of DTOs, as they are validated by FluentValidation only (see
/// <see cref="FluentValidationFilterAttribute"/>). It covers validation attributes and the implicit [Required] that
/// ASP.NET Core adds for non-nullable reference types, both on DTO properties and on action parameters of DTO types.
/// Other models of the app aren't affected.
/// </summary>
internal class DtoValidationMetadataProvider : IValidationMetadataProvider
{
  /// <inheritdoc/>
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

  // Removes validation attributes (including the implicit [Required]) from the metadata.
  private static void RemoveDataAnnotations(ValidationMetadataProviderContext context)
  {
    context.ValidationMetadata.IsRequired = false;

    for (int i = context.ValidationMetadata.ValidatorMetadata.Count - 1; i >= 0; i--)
    {
      if (context.ValidationMetadata.ValidatorMetadata[i] is ValidationAttribute)
        context.ValidationMetadata.ValidatorMetadata.RemoveAt(i);
    }
  }
}
