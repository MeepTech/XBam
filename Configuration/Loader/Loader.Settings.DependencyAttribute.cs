using System;

namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Used to prevent this type of Model, Archetype or Component from being loaded by the Loader until all of the dependent types are successfully loaded first.   
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
  public class DependencyAttribute
    : Attribute {

    /// <summary>
    /// The type this is dependent on
    /// </summary>
    public Type DependentOnType {
      get;
      internal set;
    }

    /// <summary>
    /// Add a new dependency to this type.
    /// </summary>
    public DependencyAttribute(Type requiredType) {
      DependentOnType = requiredType;
    }
  }
}
