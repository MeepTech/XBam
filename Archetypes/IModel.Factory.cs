using System;
using System.Collections.Generic;
using static Meep.Tech.XBam.Configuration.Loader.Settings;

namespace Meep.Tech.XBam {

  public partial interface IModel {

    /// <summary>
    /// The default factory for Models without Archetypes.
    /// One of these is instantiated for each Model[] class and IComponent[] class by default.
    /// This is the base non-generic utility class
    /// </summary>
    public interface IFactory : XBam.IFactory {}
  }

  public partial interface IModel<TModelBase> where TModelBase : IModel<TModelBase> {

    /// <summary>
    /// The default builder for Models without Archetypes.
    /// One of these is instantiated for each Model[] class and IComponent[] class by default.
    /// They can be overriden.
    /// </summary>
    [Configuration.Loader.Settings.DoNotBuildInInitialLoad]
    public new class Factory
      : Factory<Factory> {

      /// <summary>
      /// Used to make a new factory.
      /// </summary>
      public Factory(
        Identity id,
        Universe universe,
        HashSet<XBam.Archetype.IComponent>? archetypeComponents = null,
        IEnumerable<Func<IBuilder, IModel.IComponent>>? modelComponentCtors = null 
      )  : base(id, universe, archetypeComponents, modelComponentCtors) { }
    }

    /// <summary>
    /// The base of all BuilderFactories.
    /// Custom factories aren't built initially, you should maintain the singleton pattern yourself by setting it
    /// in the static constructor, or the Setup(Universe) override
    /// </summary>
    [DoNotBuildThisOrChildrenInInitialLoad]
    public abstract class Factory<TBuilderFactoryBase>
      : Archetype<TModelBase, TBuilderFactoryBase>,
        Archetype<TModelBase, TBuilderFactoryBase>.IExposeDefaultModelBuilderMakeMethods.Fully,
        IModel.IFactory 
      where TBuilderFactoryBase : Factory<TBuilderFactoryBase>
    {

      /// <summary>
      /// Used for Buidler Factories to easily change the base type
      /// </summary>
      public new Type ModelBaseType {
        get => base.ModelBaseType;
        init => base.ModelBaseType = value;
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      Func<XBam.IBuilder, IModel?> XBam.IFactory._modelConstructor {
        get => ModelConstructor is null
          ? null!
          : builder => ModelInitializer((IBuilder<TModelBase>)builder);
        set {
          if (value is null) {
            ModelInitializer = null!;
          }
          else {
            ModelInitializer = b => (TModelBase?)value.Invoke(b);
          }
        }
      }

      /// <summary>
      /// The static instance of this type of builder factory.
      /// </summary>
      public static Factory DefaultInstance
        => Archetypes.All.Get<Factory>();

      /// <summary>
      /// The static instance of this type of builder factory.
      /// </summary>
      public static Factory GetInstance(Universe universe)
        => universe.Archetypes.All.Get<Factory>();

      /// <summary>
      /// Overrideable builder constructor.
      /// </summary>
      public Func<Archetype, IEnumerable<KeyValuePair<string, object>>, Universe, Builder> BuildrCtor {
        init => _defaultBuilderCtor = (a, p, u) => value(a, p, u);
      }

      /// <summary>
      /// Overrideable model constructor
      /// </summary>
      public Func<Builder, TModelBase> ModelCtor {
        init => ModelInitializer = builder => value((Builder)builder);
      }

      /// <summary>
      /// Used to make a new factory.
      /// </summary>
      internal protected Factory(
        XBam.Archetype.Identity id,
        Universe universe,
        // TODO: implement:
        HashSet<XBam.Archetype.IComponent>? archetypeComponents = null,
        IEnumerable<Func<IBuilder, IModel.IComponent>>? modelComponentCtors = null
      ) : base(
            id,
            universe,
            u => (Collection)(u.Models._factoriesByModelBases
              .TryGetValue(typeof(TModelBase), out var collection)
                ? collection 
                : u.Models._factoriesByModelBases[typeof(TModelBase)] 
                  = new Collection(u))
        )
      {
        Universe.Models._factories._add(this);
      }
    }
  }
}