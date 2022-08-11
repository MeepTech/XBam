using PropertyAccess;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Meep.Tech.Reflection {
  public static class FieldExtensionMethods {
    static readonly Dictionary<int, IPropertyReadAccess> _getterCache
      = new();
    static readonly Dictionary<int, IClassPropertyWriteAccess> _setterCache
      = new();

    /// <summary>
    /// Try to get an attribute
    /// </summary>
    public static bool TryToGetAttribute<TAttribute>(this PropertyInfo property, out TAttribute attribute)
      where TAttribute : Attribute
        => (attribute = property.GetCustomAttribute<TAttribute>()) != null;

    /// <summary>
    /// Check if a type has this attribute
    /// </summary>
    public static bool HasAttribute<TAttribute>(this PropertyInfo property)
      where TAttribute : Attribute
        => Attribute.IsDefined(property, typeof(TAttribute));

    /// <summary>
    /// Faster get value for properties.
    /// </summary>
    public static object Get(this PropertyInfo property, object forObject) {
      // build the key efficiently:
      int methodKey = HashCode.Combine(forObject.GetType().FullName, property.Name);

      // check if it's cached:
      if (_getterCache.TryGetValue(methodKey, out IPropertyReadAccess propertyAccess) && propertyAccess != null) {
        return propertyAccess.GetValue(forObject);
      }

      // Build a property accessor if it's not:
      propertyAccess
        = _getterCache[methodKey]
        = property.DeclaringType.IsValueType
           ? PropertyAccessFactory.CreateForValue(property)
           : PropertyAccessFactory.CreateForClass(property);

      return propertyAccess == null
        ? throw new Exception($"Could not create getter for {property.Name} on {property.DeclaringType.FullName}")
        : propertyAccess.GetValue(forObject);
    }

    /// <summary>
    /// Faster set value for properties.
    /// </summary>
    public static void Set(this PropertyInfo property, object forObject, object value) {
      // build the key efficiently:
      int methodKey = HashCode.Combine(forObject.GetType().FullName, property.Name);

      // check if it's cached:
      if (_setterCache.TryGetValue(methodKey, out IClassPropertyWriteAccess propertyAccess) && propertyAccess != null) {
        propertyAccess.SetValue(forObject, value);
        return;
      }

      // Build a property accessor if it's not:
      var propertyToUse = property;

      // find a setter in the hirearchy if there isn't one
      if (!propertyToUse.CanWrite) {
        propertyToUse = propertyToUse?.DeclaringType?.GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          ?? propertyToUse;
      }

      while (propertyToUse is not null && !propertyToUse.CanWrite) {
        propertyToUse = propertyToUse.ReflectedType?.BaseType?.GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      }

      if (propertyToUse is null) {
        throw new NotImplementedException($"Property: {property.Name}, on type {property.ReflectedType.FullName}, does not have a setter in it's hirearchy.");
      }

      // create the cached setter
      propertyAccess
        = _setterCache[methodKey]
        = PropertyAccessFactory.CreateForClass(propertyToUse);

      propertyAccess.SetValue(forObject, value);
    }
  }
}