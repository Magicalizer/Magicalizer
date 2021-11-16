// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Validators.Abstractions
{
  /// <summary>
  /// Defines the rule set names.
  /// </summary>
  public static class RuleSetName
  {
    /// <summary>
    /// General rules that should be validated each time an item is created or edited.
    /// </summary>
    public const string Default = "Default";

    /// <summary>
    /// Rules that should be validated when an item is created.
    /// </summary>
    public const string Create = "Create";

    /// <summary>
    /// Rules that should be validated when an item is edited.
    /// </summary>
    public const string Edit = "Edit";

    /// <summary>
    /// General rules that should be validated each time an item is created or edited together with the ones
    /// that should be validated only when an item is created.
    /// </summary>
    public const string DefaultCreate = "Default,Create";

    /// <summary>
    /// General rules that should be validated each time an item is created or edited together with the ones
    /// that should be validated only when an item is edited.
    /// </summary>
    public const string DefaultEdit = "Default,Edit";
  }
}