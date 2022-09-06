using Meep.Tech.XBam.Configuration;
using System;

namespace Meep.Tech.XBam.Cloning.Configuration {

  /// <summary>
  /// A model copier
  /// </summary>
  public interface IModelCopier: IExtraUniverseContextType {

    /// <summary>
    /// The options
    /// </summary>
    ISettings? Options { get; }

    /// <summary>
    /// The way models are copied by default
    /// </summary>
    Func<IModel, IModel> CopyMethod { get; }

    /// <summary>
    /// Settings for the Model Copy Context
    /// </summary>
    public interface ISettings {

      /// <summary>
      /// The default way models are copied
      /// </summary>
      Func<IModel, IModel> DefaultCopyMethod { get; }
    }
  }
}