using System.Reflection;
using System;

namespace Meep.Tech.XBam.Configuration {

  public partial class AutoBuilderContext {
    /// <summary>
    /// Settings for an AutoBuilderContext
    /// </summary>
    public new class Settings : Universe.ExtraContext.Settings, IModelAutoBuilder.ISettings {

      ///<summary><inheritdoc/></summary>
      public virtual Action<System.Type, AutoBuildAttribute, PropertyInfo>? OnLoaderAutoBuildPropertyCreationStart 
        { get; set; } = (_,_,_) => { };

      ///<summary><inheritdoc/></summary>
      public virtual Action<
        System.Type,
        AutoBuildAttribute,
        PropertyInfo,
        (int, string, Func<IModel, IBuilder, IModel>)
      >? OnLoaderAutoBuildPropertyCreationComplete 
        { get; set; } = (_, _, _, _) => { };
    }
  }
}
