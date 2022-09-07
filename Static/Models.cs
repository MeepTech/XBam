using KellermanSoftware.CompareNetObjects;
using System;
using Meep.Tech.Reflection;
using System.Linq;

namespace Meep.Tech.XBam {

  /// <summary>
  /// Constants and static access for different types of Models
  /// </summary>
  public static class Models {
    
    /// <summary>
    /// The default universe to use for models
    /// TODO: this should clear some things in the future if it's changed during runtime, such as the default builder factory in the generic Models[] class.
    /// </summary>
    public static Universe DefaultUniverse {
      get => _defaultUniverseOverride ??= Archetypes.DefaultUniverse;
      set => _defaultUniverseOverride = value;
    } static Universe? _defaultUniverseOverride;

    /// <summary>
    /// Get the builder for a given component by type.d
    /// </summary>
    public static IModel.IFactory GetFactory(Type type)
      => DefaultUniverse.Models._factoriesByModelType[type];

    /// <summary>
    /// Get the base model type of this model type.
    /// </summary>
    public static System.Type GetModelBaseType(this System.Type type)
      => DefaultUniverse.Models.GetBaseType(type);

    /// <summary>
    /// Get logic used to compare models of the given type
    /// </summary>
    public static CompareLogic GetCompareLogic(System.Type type)
      => Models.DefaultUniverse.Models.GetCompareLogic(type);

    /// <summary>
    /// For models form interfaces
    /// </summary>
    public static class FromInterface<TModel> where TModel : Model.IFromInterface {

      /// <summary>
      /// Helper to get the collection for models from interfaces from the default universe:
      /// </summary>
      public static Archetype.Collection Types 
        => Universe.Default.Archetypes.GetDefaultForModel<TModel>()
          .Types;
    }
  }

  /// <summary>
  /// Static data values for components
  /// </summary>
  public static class Models<TModel> 
    where TModel : IModel<TModel> 
  {
    /// <summary>
    /// Overrideable Compare logic for each type of model.
    /// This is inherited once set
    /// </summary>
    public static CompareLogic CompareLogic {
      get => Models.GetCompareLogic(typeof(TModel));
      set => Models.DefaultUniverse.Models._compareLogicByModelType[typeof(TModel)] = value;
    }

    /// <summary>
    /// Builder instance for this type of component.
    /// You can use this to set a custom builder for this type of model and it's children.
    /// </summary>
    public static IModel<TModel>.Factory Factory {
      get => (IModel<TModel>.Factory)
        Models.DefaultUniverse.Models
          .GetFactory<TModel>();
      set {
        Models.DefaultUniverse.Models
          ._setFactory<TModel>(value);
      }
    }

    /// <summary>
    /// Set a new constructor for this model's builder class.
    /// </summary>
    public static void SetBuilderConstructor(Func<IModel<TModel>.Builder, TModel> newConstructor)
      => Models.DefaultUniverse.Models.SetBuilderConstructor(newConstructor);
  }
}