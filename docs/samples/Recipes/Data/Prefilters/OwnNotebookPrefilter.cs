using App.Domain;
using App.Filters;
using Magicalizer.Domain.Services.Abstractions;

namespace App.Data.Prefilters;

public class OwnNotebookPrefilter : IQueryPrefilter<Entities.Notebook, NotebookFilter>
{
  private readonly ICurrentUser currentUser;

  public OwnNotebookPrefilter(ICurrentUser currentUser)
  {
    this.currentUser = currentUser;
  }

  public IQueryable<Entities.Notebook> Apply(IQueryable<Entities.Notebook> query, NotebookFilter? filter)
  {
    string? userId = this.currentUser.Id;

    // Without a user, there is nothing to see.
    if (userId == null)
      return query.Where(n => false);

    return query.Where(n => n.UserId == userId);
  }
}
