using FluentValidation;
using Magicalizer.Validators.Abstractions;

namespace App.Api.Dto.Validators;

public class NoteValidator : AbstractValidator<Note>
{
  public NoteValidator()
  {
    this.RuleFor(n => n.Notebook).NotNull().DependentRules(() => {
      this.RuleFor(n => n.Notebook!.Id).GreaterThan(0);
    });

    this.RuleFor(n => n.Title).NotEmpty().MaximumLength(128);
    this.RuleFor(n => n.Text).MaximumLength(4000);
    this.RuleSet(RuleSetName.Edit, () => {
      this.RuleFor(n => n.Id).NotEmpty();
    });
  }
}
