using FluentValidation;
using Magicalizer.Validators.Abstractions;

namespace App.Api.Dto.Validators;

public class NotebookValidator : AbstractValidator<Notebook>
{
  public NotebookValidator()
  {
    this.RuleFor(n => n.Name).NotEmpty().MaximumLength(64);
    this.RuleSet(RuleSetName.Edit, () => {
      this.RuleFor(n => n.Id).NotEmpty();
    });
  }
}
