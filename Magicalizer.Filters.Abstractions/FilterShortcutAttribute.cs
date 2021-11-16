// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Filters.Abstractions
{
  /// <summary>
  /// Filter shortcuts allow to omit long navigation property paths and to filter by the enumeration element's properties.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class FilterShortcutAttribute : Attribute
  {
    /// <summary>
    /// The filtered objects' type navigation property path.
    /// To filter by the enumeration element's properties "[]" must be added after the enumeration navigation property name.
    /// Example:
    /// <code>
    /// public class CategoryFilter : IFilter
    /// {
    ///   [FilterShortcut("Products[]")]
    ///   public ProductFilter Product { get; set; }
    /// }
    /// </code>
    /// This code assumes that the <c>Category</c> entity contains the <c>Products</c> navigation property and allows to filter categories by a product’s properties.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterShortcutAttribute"/> class.
    /// </summary>
    /// <param name="path">
    /// The filtered objects' type navigation property path.
    /// To filter by the enumeration element's properties "[]" must be added after the enumeration navigation property name.
    /// Example:
    /// <code>
    /// public class CategoryFilter : IFilter
    /// {
    ///   [FilterShortcut("Products[]")]
    ///   public ProductFilter Product { get; set; }
    /// }
    /// </code>
    /// This code assumes that the <c>Category</c> entity contains the <c>Products</c> navigation property and allows to filter categories by a product’s properties.
    /// </param>
    public FilterShortcutAttribute(string path)
    {
      this.Path = path;
    }
  }
}