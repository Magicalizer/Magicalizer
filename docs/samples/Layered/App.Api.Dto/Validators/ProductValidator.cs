using FluentValidation;
using Magicalizer.Validators.Abstractions;

namespace App.Api.Dto.Validators;

public class ProductValidator : AbstractValidator<Product>
{
  public ProductValidator()
  {
    this.RuleFor(p => p.Category).NotNull().DependentRules(() => {
      this.RuleFor(p => p.Category!.Id).GreaterThan(0);
    });

    this.RuleFor(p => p.Name).NotEmpty().MaximumLength(64);
    this.RuleFor(p => p.Price).GreaterThan(0m);
    this.RuleSet(RuleSetName.Edit, () => {
      this.RuleFor(p => p.Id).NotEmpty();
    });
  }
}
