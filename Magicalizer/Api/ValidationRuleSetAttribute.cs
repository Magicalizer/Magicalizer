// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Api;

/// <summary>
/// Specifies the FluentValidation rule sets to execute when an action parameter is validated
/// by the <see cref="FluentValidationFilterAttribute"/>. Without this attribute only the default rules are executed.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class ValidationRuleSetAttribute : Attribute
{
  /// <summary>
  /// The names of the rule sets to execute.
  /// </summary>
  public string[] RuleSets { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ValidationRuleSetAttribute"/> class.
  /// </summary>
  /// <param name="ruleSet">
  /// The comma-separated names of the rule sets to execute (e.g. "Default,Create"),
  /// see <see cref="Validators.Abstractions.RuleSetName"/> for the standard ones.
  /// </param>
  public ValidationRuleSetAttribute(string ruleSet)
  {
    this.RuleSets = ruleSet.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
  }
}