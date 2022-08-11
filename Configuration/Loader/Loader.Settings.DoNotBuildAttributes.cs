using System;

namespace Meep.Tech.XBam.Configuration {
  public partial class Loader {

    public partial class Settings {

      /// <summary>
      /// Prevents a type that inherits from Archetype or IModel from being built as an archetype during initial loading. this is NOT inherited.
      /// </summary>
      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
      public class DoNotBuildInInitialLoadAttribute
        : Attribute, ITrait<DoNotBuildInInitialLoadAttribute> {

        ///<summary><inheritdoc/></summary>
        public string TraitName 
          => "Not Built By Loader";

        ///<summary><inheritdoc/></summary>
        public string TraitDescription 
          => "Archetypes with this trait are not built by the Loader Initially.";
      }

      /// <summary>
      /// Prevents a type that inherits from Archetype or IModel and it's inherited types from being built into an archetype during initial loading.
      /// </summary>
      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
      public class DoNotBuildThisOrChildrenInInitialLoadAttribute
        : DoNotBuildInInitialLoadAttribute, ITrait<DoNotBuildThisOrChildrenInInitialLoadAttribute> {

        ///<summary><inheritdoc/></summary>
        public new string TraitName
          => "This and Child Archetypes Not Built by Loader";

        ///<summary><inheritdoc/></summary>
        public new string TraitDescription
          => "Archetypes, and Children of Archetypes with this trait are not built by the Loader Initially.";
      }
    }
  }
}
