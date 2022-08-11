using System;

namespace Meep.Tech.XBam {

  namespace Configuration {

    /// <summary>
    /// For types that implement ISplayed, this allows new types to be built when a new enum is added during runtime.
    /// It takes the IBuildOneForEach type as an argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AllowLazyArchetypeInitializationsOnNewLazyEnumerationInitializationsAttribute : Attribute, ITrait<AllowLazyArchetypeInitializationsOnNewLazyEnumerationInitializationsAttribute> {

      public System.Type SplayType { get; }

      public string TraitName => "Allow Lazy Loaded Splayed Types";

      public string TraitDescription => "For types that implement ISplayed, this allows new types to be built when a new enum is added during runtime.";

      public AllowLazyArchetypeInitializationsOnNewLazyEnumerationInitializationsAttribute(Type SplayType) {
        this.SplayType = SplayType;
      }
    }
  }
}