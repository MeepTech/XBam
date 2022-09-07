using System.Reflection;
using System;

namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Configures the Auto Builder on Models.
  /// </summary>
  public interface IModelAutoBuilder : IExtraUniverseContextType {

    ///<summary><inheritdoc/></summary>
    new ISettings Options 
      { get; }

    /// <summary>
    /// Check if an archetype has auto builder steps included.
    /// </summary>
    bool HasAutoBuilderSteps<TArchetype>(TArchetype archetype) where TArchetype : Archetype;

    /// <summary>
    /// Settings for an IModelAutoBuilder
    /// TODO: move to it's own file.
    /// </summary>
    public interface ISettings {

      /// <summary>
      /// Executed once for each xbam auto build property created when scanning models.
      /// </summary>
      /// <remarks>
      /// Use += to add functionality
      /// </remarks>
      Action<System.Type, AutoBuildAttribute, PropertyInfo>? OnLoaderAutoBuildPropertyCreationStart { get; set; }

      /// <summary>
      /// Executed once for each xbam auto build property created when scanning models.
      /// </summary>
      /// <remarks>
      /// Use += to add functionality
      /// </remarks>
      Action<
        System.Type,
        AutoBuildAttribute,
        PropertyInfo,
        (int, string, Func<IModel, IBuilder, IModel>)
      >? OnLoaderAutoBuildPropertyCreationComplete { get; set; }
    }
  }
}
