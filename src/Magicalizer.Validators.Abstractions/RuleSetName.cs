// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Validators.Abstractions;

/// <summary>
/// Names of standard validation rule sets.
/// </summary>
public static class RuleSetName
{
  /// <summary>
  /// Rules that are validated each time a model is created or edited.
  /// </summary>
  public const string Default = "Default";

  /// <summary>
  /// Rules that are validated each time a model is created.
  /// </summary>
  public const string Create = "Create";

  /// <summary>
  /// Rules that are validated each time a model is edited.
  /// </summary>
  public const string Edit = "Edit";

  /// <summary>
  /// Default rules with the creation ones.
  /// </summary>
  public const string DefaultCreate = "Default,Create";

  /// <summary>
  /// Default rules with the editing ones.
  /// </summary>
  public const string DefaultEdit = "Default,Edit";
}
