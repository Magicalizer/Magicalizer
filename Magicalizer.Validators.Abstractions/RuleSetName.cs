// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Validators.Abstractions;

/// <summary>
/// The standard validation rule set names.
/// </summary>
public static class RuleSetName
{
  /// <summary>
  /// General rules that should be validated each time an item is created or edited.
  /// </summary>
  public const string Default = "Default";

  /// <summary>
  /// Rules that should be validated each time an item is created.
  /// </summary>
  public const string Create = "Create";

  /// <summary>
  /// Rules that should be validated each time an item is edited.
  /// </summary>
  public const string Edit = "Edit";

  /// <summary>
  /// Combines the general rules with the rules specific to item creation.
  /// </summary>
  public const string DefaultCreate = "Default,Create";

  /// <summary>
  /// Combines the general rules with the rules specific to item editing.
  /// </summary>
  public const string DefaultEdit = "Default,Edit";
}