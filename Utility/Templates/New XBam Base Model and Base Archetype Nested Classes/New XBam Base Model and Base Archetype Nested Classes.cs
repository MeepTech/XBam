using Meep.Tech.Data;

namespace $rootnamespace$ {
  
  /// <summary>
  /// The Base Model for all $fileinputname$s
  /// </summary>
  public class $fileinputname$ : Model<$fileinputname$, $fileinputname$.Type>, IModel.IUseDefaultUniverse {

    /// <summary>
    /// The Base Archetype for $fileinputname$s
    /// </summary>
    public abstract class Type : Archetype<$fileinputname$, $fileinputname$.Type> {

      /// <summary>
      /// Used to make new Child Archetypes for $fileinputname$.Type 
      /// </summary>
      /// <param name="id">The unique identity of the Child Archetype</param>
      protected Type(Identity id)
        : base(id) { }
    }
  }
}
