namespace Meep.Tech.XBam {

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// This is the non-generic for utility reasons.
  /// </summary>
  public partial interface IComponent : IModel {

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public new Universe Universe {
      get;
      internal protected set;
    } Universe IModel.Universe {
      get => Universe;
      set => Universe = value;
    }

    /// <summary>
    /// Access to the builder factory for this type of component
    /// </summary>
    new XBam.IComponent.IFactory Factory {
      get;
      protected internal set;
    }

    XBam.IFactory IModel.Factory
      => Factory;

    /// <summary>
    /// A key used to index this type of component. There can only be one componet per key on a model or archetype
    /// This is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
    /// </summary>
    string Key
      => Factory.Key;

    internal static void _setUniverse(ref XBam.IComponent component, Universe universe) {
      component.Universe = universe;
    }
  }

  /// <summary>
  /// The base interface for components without branching archet
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent 
    where TComponentBase : IComponent<TComponentBase> {

    /// <summary>
    /// For overriding base configure calls
    /// </summary>
    XBam.IComponent OnInitialized(XBam.IBuilder builder)
      => this;

    ///<summary><inheritdoc/></summary>
    IModel IModel.OnInitialized(Archetype archetype, Universe? universe, XBam.IBuilder? builder) {
      Universe = universe ?? builder?.Archetype.Universe ?? Universe.Default;
      ((XBam.IComponent)(this)).Factory = (IFactory)(archetype ?? builder?.Archetype);

      return OnInitialized(builder);
    }
  }

  /// <summary>
  /// Serialization Related Component Extensions
  /// </summary>
  public static class ComponentExtensions {

    /// <summary>
    /// Helper function to fetch the key for this component type
    /// A key used to index this type of component. There can only be one componet per key on a model or archetype
    /// The Key is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
    /// </summary>
    public static string GetKey(this IComponent component)
      => component.Key;
  }
}
