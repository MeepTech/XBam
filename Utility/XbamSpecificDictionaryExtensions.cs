using Meep.Tech.Collections.Generic;
using System;
using System.Collections.Generic;

namespace Meep.Tech.XBam {

  /// <summary>
  /// Dictionaty extensions specific to xbam components
  /// </summary>
  public static class XbamSpecificDictionaryExtensions {

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static Dictionary<string, object> Append(this Dictionary<string, object> current, string key, object value)
        => new Dictionary<string, object>(current).WithAddedPair(key, value);

    #region Model Component Helpers

    /// <summary>
    /// Append a new component
    /// </summary>
    public static IReadOnlyDictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> Append<TComponent>(this IReadOnlyDictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> current, Func<IComponent.IBuilder, TComponent> newComponentConstructor) where TComponent : IModel.IComponent
      => DictionaryExtensions.Append(current, Components.GetKey(typeof(TComponent)), new Func<IComponent.IBuilder, IModel.IComponent>(b => newComponentConstructor(b)));

    /// <summary>
    /// Append a new component with a default builder
    /// </summary>
    public static IReadOnlyDictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> Append<TComponent>(this IReadOnlyDictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> current) where TComponent : IModel.IComponent
      => current.Append(builder => (TComponent)Components.GetFactory(typeof(TComponent)).Make(builder));

    /// <summary>
    /// update an existing component
    /// </summary>
    public static IReadOnlyDictionary<string, Func<IBuilder, IModel.IComponent>> Update<TComponent>(this IReadOnlyDictionary<string, Func<IBuilder, IModel.IComponent>> current, Func<IBuilder, TComponent> newComponentConstructor) where TComponent : IModel.IComponent {
      string key = Components.GetKey(typeof(TComponent));
      return !current.ContainsKey(key)
        ? throw new KeyNotFoundException($"Key for component type: {key} not found to update.")
        : (IReadOnlyDictionary<string, Func<IBuilder, IModel.IComponent>>)new Dictionary<string, Func<IBuilder, IModel.IComponent>>(current).WithSetPair(key, new Func<IBuilder, IModel.IComponent>(b => newComponentConstructor(b)));
    }

    /// <summary>
    /// update an existing component
    /// </summary>
    public static IReadOnlyDictionary<string, Func<IBuilder, IModel.IComponent>> Update<TComponent>(this IReadOnlyDictionary<string, Func<IBuilder, IModel.IComponent>> current, Func<TComponent, TComponent> updateComponent) where TComponent : IModel.IComponent {
      string key = Components.GetKey(typeof(TComponent));
      if (!current.ContainsKey(key)) { 
        throw new KeyNotFoundException($"Key for component type: {key} not found to update.");
      }

      var @new = new Dictionary<string, Func<IBuilder, IModel.IComponent>>(current);
      @new[key] = builder => updateComponent((TComponent)current[key](builder));
      return @new;
    }

    /// <summary>
    /// update an existing component
    /// </summary>
    public static IReadOnlyDictionary<string, Func<IBuilder, IModel.IComponent>> Update<TComponent>(this IReadOnlyDictionary<string, Func<IBuilder, IModel.IComponent>> current, Func<IBuilder, TComponent, TComponent> updateComponent) where TComponent : IModel.IComponent {
      string key = Components.GetKey(typeof(TComponent));
      if (!current.ContainsKey(key)) {
        throw new KeyNotFoundException($"Key for component type: {key} not found to update.");
      }

      var @new = new Dictionary<string, Func<IBuilder, IModel.IComponent>>(current);
      @new[key] = builder => updateComponent(builder, (TComponent)current[key](builder));
      return @new;
    }

    #endregion

    #region Archetype Component Helpers

    /// <summary>
    /// Append a new component
    /// </summary>
    public static IReadOnlyDictionary<string, Archetype.IComponent> Append<TComponent>(this IReadOnlyDictionary<string, Archetype.IComponent> current)
      where TComponent : Archetype.IComponent
        => current.Append((Archetype.IComponent)Components.GetFactory(typeof(TComponent)).Make());

    /// <summary>
    /// Append a new component
    /// </summary>
    public static IReadOnlyDictionary<string, Archetype.IComponent> Append(this IReadOnlyDictionary<string, Archetype.IComponent> current, Archetype.IComponent newComponent)
      => current.Append(newComponent.Key, newComponent);

    /// <summary>
    /// update an existing component
    /// </summary>
    public static IReadOnlyDictionary<string, Archetype.IComponent> Update(this IReadOnlyDictionary<string, Archetype.IComponent> current, Archetype.IComponent newComponent) {
      string key = newComponent.Key;
      return !current.ContainsKey(key)
        ? throw new KeyNotFoundException($"Key for component: {key} not found to update.")
        : (IReadOnlyDictionary<string, Archetype.IComponent>)new Dictionary<string, Archetype.IComponent>(current).WithSetPair(key, newComponent);
    }

    /// <summary>
    /// update an existing component
    /// </summary>
    public static IReadOnlyDictionary<string, Archetype.IComponent> Update<TComponent>(this IReadOnlyDictionary<string, Archetype.IComponent> current, Func<TComponent, TComponent> updateComponent) where TComponent : Archetype.IComponent
      => current.Update(updateComponent((TComponent)current[Components.GetKey(typeof(TComponent))]));

    #endregion
  }
}
