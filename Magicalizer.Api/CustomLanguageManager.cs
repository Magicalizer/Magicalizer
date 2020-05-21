// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentValidation.Resources;

namespace Magicalizer.Api
{
  public class CustomLanguageManager : LanguageManager
  {
    public CustomLanguageManager()
    {
      this.AddTranslation("en", "NotNullValidator", "Value must not be null.");
      this.AddTranslation("en", "NotEmptyValidator", "Value must not be null or empty.");
      this.AddTranslation("en", "MinimumLengthValidator", "Value must be at least {MinLength} characters long.");
      this.AddTranslation("en", "MaximumLengthValidator", "Value must be no more than {MaxLength} characters long.");
      this.AddTranslation("en", "MaximumLengthValidator", "Value must be no more than {MaxLength} characters long.");
      this.AddTranslation("en", "LessThanValidator", "Value must be less than {ComparisonValue}.");
      this.AddTranslation("en", "LessThanOrEqualValidator", "Value must be less than or equal to {ComparisonValue}.");
      this.AddTranslation("en", "GreaterThanValidator", "Value must be greater than {ComparisonValue}.");
      this.AddTranslation("en", "GreaterThanOrEqualValidator", "Value must be greater than or equal to {ComparisonValue}.");
      this.AddTranslation("en", "PredicateValidator", "Value is invalid due to specific conditions.");
    }
  }
}
