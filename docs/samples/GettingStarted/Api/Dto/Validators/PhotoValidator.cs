using FluentValidation;
using Magicalizer.Validators.Abstractions;

namespace App.Api.Dto.Validators;

public class PhotoValidator : AbstractValidator<Photo>
{
  public PhotoValidator()
  {
    this.RuleFor(p => p.Product).NotNull().DependentRules(() => {
      this.RuleFor(p => p.Product!.Id).GreaterThan(0);
    });

    this.RuleFor(p => p.Filename).NotEmpty().MaximumLength(64);
  }
}
