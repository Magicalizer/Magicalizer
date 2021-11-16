// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  /// <summary>
  /// Describes a filter. Filters are used to filter objects of the corresponding types.
  /// Each filter can consist of other ones (according to the properties of the filtered objects' type).
  /// <see cref="FilterShortcutAttribute"/> can be used to omit long navigation property paths and to filter by the enumeration element's properties.
  /// </summary>
  public interface IFilter
  {
  }
}