using System;
using System.Collections.Generic;

namespace Meep.Tech.XBam {

  public partial interface IComponent {

    /// <summary>
    /// The default factory for Models without Archetypes.
    /// One of these is instantiated for each Model[] class and IComponent[] class by default.
    /// This is the base interface.
    /// </summary>
    public new interface IFactory
      : IModel.IFactory {

      /// <summary>
      /// The key for the component type.
      /// This is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
      /// </summary>
      string Key {
        get;
      }

      /// <summary>
      /// If the component made from this factory should be included in equality checks for the parent object
      /// </summary>
      public virtual bool IncludeInParentModelEqualityChecks
        => true;

      /// <summary>
      /// Make a default component for an archetype.
      /// </summary>
      /// <returns></returns>
      public new IComponent Make()
        => (this as Archetype).Make<IComponent>();

      /// <summary>
      /// Make a default component for an archetype.
      /// </summary>
      /// <returns></returns>
      public IComponent Make(IBuilder parentBuilder)
        => (this as Archetype).Make<IComponent>(parentBuilder);
    }
  }

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent
    where TComponentBase : XBam.IComponent<TComponentBase> {

    /// <summary>
    /// General Base Builder Factory for Components.
    /// </summary>
    [Configuration.Loader.Settings.DoNotBuildInInitialLoad]
    public new class Factory
      : Factory<Factory>,
      IComponent.IFactory {

      /// <summary>
      /// Default test params for this builder factory.
      /// </summary>
      public new Dictionary<string, object> DefaultTestParams {
        protected get => base.DefaultTestParams;
        init => base.DefaultTestParams = value; 
      }

      /// <summary>
      /// The key for the component type.
      /// This is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
      /// </summary>
      public string Key
        => ModelBaseType.FullName;

      ///<summary><inheritdoc/></summary>
      protected internal override Func<Archetype, IEnumerable<KeyValuePair<string, object>>, Universe, IBuilder<TComponentBase>> BuilderConstructor {
        get => _defaultBuilderCtor ??= new((archetype, @params, universe) => new Builder(archetype, @params, null, universe));
        set => _defaultBuilderCtor = value;
      }

      /// <summary>
      /// Overrideable builder constructor.
      /// </summary>
      public new Func<Archetype, IEnumerable<KeyValuePair<string, object>>, Universe, Builder> BuildrCtor {
        init => _defaultBuilderCtor = (a, p, u) => value(a, p, u);
      }

      /// <summary>
      /// Overrideable model constructor
      /// </summary>
      public Func<Builder, TComponentBase> ComponentCtor {
        init => ModelInitializer = builder => value((Builder)builder);
      }

      public Factory(
        Identity id,
        Universe universe = null
      )  : base(id, universe) {}

      public Factory(
        Identity id,
        Universe universe,
        HashSet<IComponent> archetypeComponents,
        IEnumerable<Func<XBam.IBuilder, IModel.IComponent>> modelComponentCtors
      )  : base(id, universe, archetypeComponents, modelComponentCtors) {}
    }
  }
}
