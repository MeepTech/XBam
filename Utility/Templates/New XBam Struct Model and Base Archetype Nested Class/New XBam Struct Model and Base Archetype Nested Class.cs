using Meep.Tech.Data;

namespace $rootnamespace$ {

  /// <summary>
  /// A $rootnamespace$ Model
  /// </summary>
  public struct $fileinputname$ :
    Model<$fileinputname$, $fileinputname$.Type>.IFromInterface,
    IModel.IUseDefaultUniverse {

    /// <summary>
    /// All $fileinputname$ Archetypes 
    /// </summary>
    public static Type.Collection Types {
      get;
    } = new();

    /// <summary>
    /// The archetype used to build this $fileinputname$.
    /// </summary>
    public Type Archetype {
      get;
      private set;
    } Type Model<$fileinputname$, Type>.IFromInterface.Archetype {
      get => Archetype;
      set => Archetype = value;
    }

    /// <summary>
    /// The base archetype for $fileinputname$s
    /// </summary>
    public abstract class Type : Archetype<$fileinputname$, $fileinputname$.Type> {

      /// <summary>
      /// Used to make new Child Archetypes for $fileinputname$.Type 
      /// </summary>
      /// <param name="id">The unique identity of the Child Archetype</param>
      protected Type(Identity id)
        : base(id, Types) { }
    }
  }
}
