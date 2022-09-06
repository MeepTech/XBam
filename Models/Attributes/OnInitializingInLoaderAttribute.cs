using System;

namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Marks a delegate that is executed when the Model or Component's System.Type is initialized by the XBam Loader.
  /// Similar to a static ctor but is per-universe.
  /// The method or property must be static.
  /// </summary>

  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public class OnInitializingInLoaderAttribute : Attribute {

    /// <summary>
    /// A valid initializer pattern.
    /// </summary>
    public delegate void Initializer(Universe universe);
  }
}
