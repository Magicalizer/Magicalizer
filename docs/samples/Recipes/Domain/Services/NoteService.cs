using App.Domain.Models;
using App.Filters;
using FluentValidation;
using Magicalizer.Domain.Services;
using Magicalizer.Domain.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace App.Domain.Services;

public class NoteService : Service<int, Data.Entities.Note, Note, NoteFilter>
{
  public NoteService(DbContext dbContext, IEnumerable<IQueryPrefilter<Data.Entities.Note, NoteFilter>>? queryPrefilters = null, IValidator<Note>? validator = null) : base(dbContext, queryPrefilters, validator) { }

  #region DefaultSorting
  protected override IQueryable<Data.Entities.Note> GetPrefilteredQuery(NoteFilter? filter)
  {
    // The notes go by their identifiers unless a request sorts them otherwise (its sorting replaces this one).
    return base.GetPrefilteredQuery(filter).OrderBy(n => n.Id);
  }
  #endregion

  #region DeleteAsync
  public override async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
  {
    // The note must be available, as it is for getting it (the prefilters and the soft deletion filter apply).
    if (await this.GetByIdAsync(id, cancellationToken: cancellationToken) == null)
      throw new ModelNotFoundException(typeof(Note), [id]);

    // The note is marked as deleted instead of being deleted.
    await this.dbContext.Set<Data.Entities.Note>()
      .Where(n => n.Id == id)
      .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsDeleted, true), cancellationToken);
  }
  #endregion
}
