// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Services.Abstractions;

namespace Magicalizer.TestHost.Products;

/// <summary>
/// Hides the archived products from the users without the "archive" permission, whatever the filter is.
/// </summary>
public class ArchivedProductPrefilter : IQueryPrefilter<ProductEntity, ProductFilter>
{
  private readonly IHttpContextAccessor httpContextAccessor;

  /// <summary>
  /// Creates the prefilter.
  /// </summary>
  /// <param name="httpContextAccessor">Accessor of the current request (to check the user's permissions).</param>
  public ArchivedProductPrefilter(IHttpContextAccessor httpContextAccessor)
  {
    this.httpContextAccessor = httpContextAccessor;
  }

  /// <inheritdoc/>
  public IQueryable<ProductEntity> Apply(IQueryable<ProductEntity> query, ProductFilter? filter)
  {
    if (this.httpContextAccessor.HttpContext?.User.HasClaim(TestAuthenticationHandler.PermissionClaimType, "archive") == true)
      return query;

    return query.Where(p => !p.IsArchived);
  }
}
