// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Magicalizer.TestHost;

/// <summary>
/// Authenticates the requests by the test headers: <see cref="UserHeaderName"/> is the user name, and
/// <see cref="PermissionsHeaderName"/> is the comma-separated permissions (e.g. "edit,delete").
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
  /// <summary>
  /// Name of the authentication scheme.
  /// </summary>
  public const string SchemeName = "Test";

  /// <summary>
  /// Name of the header with the user name.
  /// </summary>
  public const string UserHeaderName = "X-Test-User";

  /// <summary>
  /// Name of the header with the comma-separated permissions.
  /// </summary>
  public const string PermissionsHeaderName = "X-Test-Permissions";

  /// <summary>
  /// Type of the claims with the permissions.
  /// </summary>
  public const string PermissionClaimType = "permission";

  /// <summary>
  /// Creates the handler.
  /// </summary>
  public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) { }

  /// <inheritdoc/>
  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    string? userName = this.Request.Headers[UserHeaderName];

    if (string.IsNullOrEmpty(userName))
      return Task.FromResult(AuthenticateResult.NoResult());

    List<Claim> claims = [new Claim(ClaimTypes.Name, userName)];
    string permissions = this.Request.Headers[PermissionsHeaderName].ToString();

    claims.AddRange(permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(p => new Claim(PermissionClaimType, p)));
    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName)), SchemeName)));
  }
}
