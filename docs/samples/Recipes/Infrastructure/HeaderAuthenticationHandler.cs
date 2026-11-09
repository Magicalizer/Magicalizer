using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace App.Infrastructure;

// FOR THE SAMPLE ONLY: authenticates anyone who sends the X-User header, with the roles of the X-Roles header. A real
// application uses a real scheme (e.g. JWT bearer tokens or cookies).
public class HeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
  public const string SchemeName = "Header";

  public HeaderAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) { }

  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    string? user = this.Request.Headers["X-User"];

    if (string.IsNullOrEmpty(user))
      return Task.FromResult(AuthenticateResult.NoResult());

    List<Claim> claims = [new Claim(ClaimTypes.Name, user)];
    string roles = this.Request.Headers["X-Roles"].ToString();

    claims.AddRange(roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(r => new Claim(ClaimTypes.Role, r)));

    ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));

    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
  }
}
