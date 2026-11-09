using App.Domain.Models;
using App.Filters;
using FluentValidation;
using Magicalizer.Domain.Services;
using Magicalizer.Domain.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace App.Domain.Services;

public class NotebookService : Service<int, Data.Entities.Notebook, Notebook, NotebookFilter>
{
  private readonly ICurrentUser currentUser;

  public NotebookService(ICurrentUser currentUser, DbContext dbContext, IEnumerable<IQueryPrefilter<Data.Entities.Notebook, NotebookFilter>>? queryPrefilters = null, IValidator<Notebook>? validator = null) : base(dbContext, queryPrefilters, validator)
  {
    this.currentUser = currentUser;
  }

  public override Task<Notebook> CreateAsync(Notebook model, CancellationToken cancellationToken = default)
  {
    // A new notebook belongs to the current user, whatever the client sends.
    model.UserId = this.GetUserId();
    return base.CreateAsync(model, cancellationToken);
  }

  public override Task EditAsync(Notebook model, CancellationToken cancellationToken = default)
  {
    // The DTO has no user, so it's kept (the prefilter lets the user edit only the own notebooks).
    model.UserId = this.GetUserId();
    return base.EditAsync(model, cancellationToken);
  }

  private string GetUserId()
  {
    return this.currentUser.Id ?? throw new InvalidOperationException("There is no current user.");
  }
}
