using App.Filters;
using FluentValidation;
using Magicalizer.Domain.Services.Abstractions;

namespace App.Domain.Models.Validators;

public class NoteValidator : AbstractValidator<Note>
{
  public NoteValidator(IService<int, Notebook, NotebookFilter> notebookService)
  {
    // The notebook service is prefiltered, so a notebook of another user isn't found either.
    this.RuleFor(n => n.Notebook!.Id)
      .MustAsync(async (id, cancellationToken) => await notebookService.GetByIdAsync(id, cancellationToken: cancellationToken) != null)
      .WithMessage("The notebook doesn't exist.")
      .When(n => n.Notebook != null);
  }
}
