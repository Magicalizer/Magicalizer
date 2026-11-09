using App.Domain;
using App.Filters;
using Magicalizer.Domain.Services.Abstractions;

namespace App.Data.Prefilters;

public class OwnNotePrefilter : IQueryPrefilter<Entities.Note, NoteFilter>
{
  private readonly ICurrentUser currentUser;

  public OwnNotePrefilter(ICurrentUser currentUser)
  {
    this.currentUser = currentUser;
  }

  public IQueryable<Entities.Note> Apply(IQueryable<Entities.Note> query, NoteFilter? filter)
  {
    string? userId = this.currentUser.Id;

    if (userId == null)
      return query.Where(n => false);

    // A note belongs to the user of its notebook.
    return query.Where(n => n.Notebook!.UserId == userId);
  }
}
