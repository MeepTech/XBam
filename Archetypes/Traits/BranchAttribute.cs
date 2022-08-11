using Meep.Tech.XBam.Configuration;
using System;

namespace Meep.Tech.XBam {

  /// <summary>
  /// Used as shorthand for an archetype that produces a different model via the Model constructor
  /// This will just set the model constructor of the archetype to the basic activator for the parameterless ctor of TNewBaseModel, or the declaring type of the current type.     
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true)]
  public class BranchAttribute
    : Attribute, ITrait<BranchAttribute> {

    string ITrait<BranchAttribute>.TraitName
      => "Branching";

    string ITrait<BranchAttribute>.TraitDescription
      => $"Makes this or any Child Archetype with a Class Declaration that is inside of a Model Class Declaration (as a Nested Type) produce that type of Model";

    /// <summary>
    /// The new base model this archetype branches for
    /// </summary>
    public Type NewBaseModelType {
      get;
      internal set;
    }

    public BranchAttribute(Type newBaseModelType = null) {
      NewBaseModelType = newBaseModelType;
    }
  }
}
