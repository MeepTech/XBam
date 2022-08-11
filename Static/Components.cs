using System;

namespace Meep.Tech.XBam {

  /// <summary>
  /// Static data values for components
  /// </summary>
  public static class Components {

    /// <summary>
    /// The default universe to use for models
    /// </summary>
    public static Universe DefaultUniverse {
      get => _defaultUniverseOverride ??= Archetypes.DefaultUniverse;
      set => _defaultUniverseOverride = value;
    } private static Universe _defaultUniverseOverride;

    /// <summary>
    /// Get the builder for a given component by type.d
    /// </summary>
    public static string GetKey(Type type)
      => DefaultUniverse.Components.GetFactory(type).Key;

    /// <summary>
    /// Get the builder for a given component by type.d
    /// </summary>
    public static IComponent.IFactory GetFactory(Type type)
      => DefaultUniverse.Components.GetFactory(type);

    /// <summary>
    /// Get the builder for a given component by type.d
    /// </summary>
    public static IComponent.IFactory GetFactory<TComponent>()
      where TComponent : XBam.IComponent
        => DefaultUniverse.Components.GetFactory(typeof(TComponent));

    /// <summary>
    /// Get the base model type of this model type.
    /// </summary>
    public static System.Type GetBaseType(this System.Type type)
      => DefaultUniverse.Components.GetBaseType(type);
  }

  /// <summary>
  /// Static data values for components
  /// </summary>
  public static class Components<TComponent> 
    where TComponent : XBam.IComponent<TComponent> 
  {

    /// <summary>
    /// The key for this type of component.
    /// This is based on the base model type's name.
    /// There should only be one component per key on a model.
    /// </summary>
    public static string Key
      => Factory.Key;

    /// <summary>
    /// Builder instance for this type of component.
    /// You can use this to set a custom builder for this type of component and it's children.
    /// </summary>
    public static IComponent<TComponent>.Factory Factory {
      get => (IComponent<TComponent>.Factory)
        Components.DefaultUniverse.Components
          .GetFactory<TComponent>();
      set {
        Components.DefaultUniverse.Components
          .SetFactory<TComponent>(value);
      }
    }
  }
}