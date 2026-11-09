// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Api;

/// <summary>
/// FluentValidation rule sets that <see cref="FluentValidationFilterAttribute"/> validates an action parameter with.
/// Without the attribute, only the default rules are validated.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class ValidationRuleSetAttribute : Attribute
{
  /// <summary>
  /// Names of the rule sets to validate.
  /// </summary>
  public string[] RuleSets { get; }

  /// <summary>
  /// Creates an attribute.
  /// </summary>
  /// <param name="ruleSets">
  /// Comma-separated names of the rule sets to validate (e.g. "Default,Create"), see
  /// <see cref="Validators.Abstractions.RuleSetName"/> for the standard ones.
  /// </param>
  public ValidationRuleSetAttribute(string ruleSets)
  {
    this.RuleSets = ruleSets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
  }
}
