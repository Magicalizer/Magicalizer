// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Extensions;

/// <summary>
/// Extensions of <see cref="Type"/>.
/// </summary>
internal static class TypeExtensions
{
  // Cached public instance properties of the types, as they are read for each request and query.
  private static readonly ConcurrentDictionary<Type, TypeProperties> PropertiesByTypes = [];

  // Cached compiled getters of the properties.
  private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object?>> ValueGettersByProperties = [];

  // Cached element types of the collection types (null for the other types).
  private static readonly ConcurrentDictionary<Type, Type?> CollectionElementTypesByTypes = [];

  // Cached properties of the filter types that are applied (without the ignored ones).
  private static readonly ConcurrentDictionary<Type, IReadOnlyList<PropertyInfo>> FilterPropertiesByTypes = [];

  // Cached results of IsValueFilter.
  private static readonly ConcurrentDictionary<Type, bool> IsValueFilterByTypes = [];

  /// <summary>
  /// Gets non-abstract classes of the loaded assemblies that implement the interface.
  /// </summary>
  /// <param name="type">Interface type.</param>
  /// <returns>Non-abstract classes that implement the interface.</returns>
  public static IEnumerable<Type> GetImplementations(this Type type)
  {
    return AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(a => a.GetTypes())
      .Where(t => type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
      .ToList();
  }

  /// <summary>
  /// Gets the type arguments of the generic interface implemented by the type.
  /// </summary>
  /// <param name="type">Type that implements the generic interface.</param>
  /// <param name="genericInterfaceDefinition">Generic interface type definition (e.g. <c>IModel&lt;,&gt;</c>).</param>
  /// <returns>Type arguments, or <c>null</c> if the interface isn't implemented.</returns>
  public static IEnumerable<Type>? GetGenericInterfaceTypeArguments(this Type type, Type genericInterfaceDefinition)
  {
    return type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericInterfaceDefinition)?.GetGenericArguments();
  }

  /// <summary>
  /// Gets the first type argument of the generic interface implemented by the type that is assignable to the base type.
  /// </summary>
  /// <param name="type">Type that implements the generic interface.</param>
  /// <param name="genericInterfaceDefinition">Generic interface type definition (e.g. <c>IModel&lt;,&gt;</c>).</param>
  /// <param name="typeArgumentBaseType">Base type of the type argument.</param>
  /// <returns>Type argument, or <c>null</c> if it isn't found.</returns>
  public static Type? GetGenericInterfaceTypeArgument(this Type type, Type genericInterfaceDefinition, Type typeArgumentBaseType)
  {
    return type.GetGenericInterfaceTypeArguments(genericInterfaceDefinition)?.FirstOrDefault(typeArgumentBaseType.IsAssignableFrom);
  }

  /// <summary>
  /// Gets the public instance properties of the type (without indexers) from the cache.
  /// </summary>
  /// <param name="type">Type to get the properties of.</param>
  /// <returns>Properties in the order of the reflection (the ones of the derived type first).</returns>
  public static IReadOnlyList<PropertyInfo> GetPublicProperties(this Type type)
  {
    return GetTypeProperties(type).All;
  }

  /// <summary>
  /// Gets the public instance property of the type (without indexers) from the cache by its name, case-insensitively
  /// (e.g. "Category" by "category").
  /// </summary>
  /// <param name="type">Type to get the property of.</param>
  /// <param name="name">Name of the property.</param>
  /// <returns>Property, or <c>null</c> if the type doesn't have it.</returns>
  public static PropertyInfo? GetPublicProperty(this Type type, string name)
  {
    return GetTypeProperties(type).ByNames.GetValueOrDefault(name);
  }

  /// <summary>
  /// Gets the compiled getter of the property from the cache (e.g. <c>o =&gt; (object)((Product)o).Name</c>), which is
  /// much faster than <see cref="PropertyInfo.GetValue(object)"/>.
  /// </summary>
  /// <param name="property">Property to get the getter of.</param>
  /// <returns>Getter that takes an object with the property and returns the value of the property.</returns>
  public static Func<object, object?> GetValueGetter(this PropertyInfo property)
  {
    return ValueGettersByProperties.GetOrAdd(property, static p => {
      ParameterExpression parameter = Expression.Parameter(typeof(object), "o");
      Expression body = Expression.Convert(Expression.Property(Expression.Convert(parameter, p.DeclaringType!), p), typeof(object));

      return Expression.Lambda<Func<object, object?>>(body, parameter).Compile();
    });
  }

  /// <summary>
  /// Gets the element type of the collection type (e.g. <c>Photo</c> of <c>ICollection&lt;Photo&gt;</c>). Strings
  /// aren't collections.
  /// </summary>
  /// <param name="type">Type to get the element type of.</param>
  /// <returns>Element type from the cache, or <c>null</c> if the type isn't a collection.</returns>
  public static Type? GetCollectionElementType(this Type type)
  {
    // The interfaces are read for each filter and query, and each read allocates an array.
    return CollectionElementTypesByTypes.GetOrAdd(type, static t => t == typeof(string) ? null :
      t.GetInterfaces().Append(t).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))?.GetGenericArguments()[0]);
  }

  /// <summary>
  /// Gets the type argument of the elements of the <see cref="IEnumerable{T}"/> type if they are of the generic type
  /// (e.g. <c>ProductModel</c> of <c>IEnumerable&lt;ISorting&lt;ProductModel&gt;&gt;</c> for <c>ISorting&lt;&gt;</c>).
  /// </summary>
  /// <param name="type">Type to get the type argument of its elements.</param>
  /// <param name="elementGenericDefinition">
  /// Generic type definition of the elements (e.g. <c>ISorting&lt;&gt;</c>).
  /// </param>
  /// <returns>
  /// Type argument, or <c>null</c> if the type isn't <see cref="IEnumerable{T}"/> of the elements of the generic type.
  /// </returns>
  public static Type? GetEnumerableElementTypeArgument(this Type type, Type elementGenericDefinition)
  {
    if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(IEnumerable<>)) return null;

    Type elementType = type.GetGenericArguments()[0];

    return elementType.IsGenericType && elementType.GetGenericTypeDefinition() == elementGenericDefinition ? elementType.GetGenericArguments()[0] : null;
  }

  /// <summary>
  /// Gets primary key types of the entity: type arguments of its <see cref="IEntity"/> interface with the most of them,
  /// as an entity with a composite primary key implements all the "smaller" interfaces too.
  /// </summary>
  /// <param name="entityType">Entity type.</param>
  /// <returns>
  /// Primary key types, or <c>null</c> if the entity doesn't implement any <see cref="IEntity"/> interface with a
  /// primary key.
  /// </returns>
  public static Type[]? GetEntityKeyTypes(this Type entityType)
  {
    Type[] keyInterfaces = [typeof(IEntity<>), typeof(IEntity<,>), typeof(IEntity<,,>)];

    return entityType.GetInterfaces()
      .Where(i => i.IsGenericType && keyInterfaces.Contains(i.GetGenericTypeDefinition()))
      .MaxBy(i => i.GetGenericArguments().Length)?
      .GetGenericArguments();
  }

  /// <summary>
  /// Gets the properties of the filter type that are applied (the ones marked with
  /// <see cref="IgnoreFilterAttribute"/> aren't) from the cache.
  /// </summary>
  /// <param name="filterType">Filter type.</param>
  /// <returns>Properties in the order of the reflection (the ones of the derived type first).</returns>
  public static IReadOnlyList<PropertyInfo> GetFilterProperties(this Type filterType)
  {
    // The attributes are slow to read, and the filters are read for each request and query.
    return FilterPropertiesByTypes.GetOrAdd(filterType, static t => t.GetPublicProperties().Where(p => !p.IsDefined(typeof(IgnoreFilterAttribute))).ToArray());
  }

  /// <summary>
  /// Checks whether the filter is a value filter (e.g. <see cref="StringFilter"/>): its properties are conditions only,
  /// none of them is a filter or a list of filters. The ignored properties aren't applied, so they aren't checked.
  /// </summary>
  /// <param name="filterType">Filter type.</param>
  /// <returns><c>true</c> if the filter is a value filter, otherwise <c>false</c>.</returns>
  public static bool IsValueFilter(this Type filterType)
  {
    // It's checked for each filter path of each request, and the attributes are slow to read.
    return IsValueFilterByTypes.GetOrAdd(filterType, static t => typeof(IFilter).IsAssignableFrom(t) && t.GetFilterProperties()
      .All(p => !typeof(IFilter).IsAssignableFrom(p.PropertyType) && !typeof(IFilter).IsAssignableFrom(p.PropertyType.GetCollectionElementType())));
  }

  /// <summary>
  /// Gets the element filter type of the collection filter: the type argument of its
  /// <see cref="IEnumerableFilter.Any"/> property (e.g. <c>PhotoFilter</c> of
  /// <c>EnumerableFilter&lt;PhotoFilter&gt;</c>).
  /// </summary>
  /// <param name="enumerableFilterType">Collection filter type.</param>
  /// <returns>Element filter type.</returns>
  public static Type GetElementFilterType(this Type enumerableFilterType)
  {
    return enumerableFilterType.GetPublicProperty(nameof(IEnumerableFilter.Any))!.PropertyType.GetCollectionElementType()!;
  }

  // Gets the cached properties of the type. If the names of the properties differ only in case, the first one is found
  // by the name.
  private static TypeProperties GetTypeProperties(Type type)
  {
    return PropertiesByTypes.GetOrAdd(type, static t => {
      PropertyInfo[] properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetIndexParameters().Length == 0).ToArray();
      Dictionary<string, PropertyInfo> propertiesByNames = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

      foreach (PropertyInfo property in properties)
        propertiesByNames.TryAdd(property.Name, property);

      return new TypeProperties(properties, propertiesByNames);
    });
  }

  /// <summary>
  /// Cached properties of a type.
  /// </summary>
  /// <param name="All">Properties in the order of the reflection.</param>
  /// <param name="ByNames">Properties by their names (case-insensitive).</param>
  private sealed record TypeProperties(IReadOnlyList<PropertyInfo> All, IReadOnlyDictionary<string, PropertyInfo> ByNames);
}
