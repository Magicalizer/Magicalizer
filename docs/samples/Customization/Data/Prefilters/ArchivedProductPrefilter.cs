using App.Filters;
using Magicalizer.Domain.Services.Abstractions;

namespace App.Data.Prefilters;

public class ArchivedProductPrefilter : IQueryPrefilter<Entities.Product, ProductFilter>
{
  private readonly IHttpContextAccessor httpContextAccessor;

  public ArchivedProductPrefilter(IHttpContextAccessor httpContextAccessor)
  {
    this.httpContextAccessor = httpContextAccessor;
  }

  public IQueryable<Entities.Product> Apply(IQueryable<Entities.Product> query, ProductFilter? filter)
  {
    // The editors see the archived products, the others don't.
    if (this.httpContextAccessor.HttpContext?.User.IsInRole("editor") == true)
      return query;

    return query.Where(p => !p.IsArchived);
  }
}
