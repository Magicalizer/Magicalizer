using System.Security.Claims;
using App.Domain;

namespace App.Infrastructure;

public class HttpCurrentUser : ICurrentUser
{
  private readonly IHttpContextAccessor httpContextAccessor;

  public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
  {
    this.httpContextAccessor = httpContextAccessor;
  }

  // A real scheme usually puts the identifier into the NameIdentifier claim, the sample one uses the name.
  public string? Id => this.httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
}
