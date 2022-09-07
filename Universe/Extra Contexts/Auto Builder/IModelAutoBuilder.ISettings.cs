using Meep.Tech.XBam.Configuration;
using Meep.Tech.XBam;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Configures the Auto Builder on Models.
  /// </summary>
  public partial interface IModelAutoBuilder : IExtraUniverseContextType {

    /// <summary>
    /// Settings for an IModelAutoBuilder
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
