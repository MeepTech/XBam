using System;

namespace Meep.Tech.XBam {

  /// <summary>
  /// Used to manually mark the model constructor for XBam
  /// </summary>

  [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
  public class ModelConstructorAttribute : Attribute {}

  /// <summary>
  /// Used to manually mark the archetype constructor for XBam
  /// TODO: move to it's own file.
  /// </summary>
  [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
  public class ArchetypeConstructorAttribute : Attribute { }

  /// <summary>
  /// Used to manually specify covariant model types produced by this archetype.
  /// By default archetypes only register the defaut model produced by tests, this allows other types to be added to ModelTypesProduced.
  /// TODO: change modeltypeproduced to modeltypeSproduced.
  /// TODO: implement
  /// TODO: move to it's own file.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class MakesModelAttribute : Attribute {

    /// <summary>
    /// The Types of Models this Member can Produce.
    /// </summary>
    public System.Type[]? Types {
      get;
      init;
    } = null;

    /// <summary>
    /// Mark a Method, Property, or Archetype as capable of producing a type of model.
    /// </summary>
    /// <param name="type"></param>
    public MakesModelAttribute(System.Type? type = null) {
      if (type is not null) {
        Types = new[] { type };
      }
    }
  }

  /// <summary>
  /// Used to manually specify covariant model types that may not be produced by this archetype.
  /// By default archetypes register the defaut model produced by tests, this allows base types to be excluded if included by mistake.
  /// TODO: implement
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class DoesNotMakeModelAttribute : Attribute {

    /// <summary>
    /// The Types of Models this Member can Produce.
    /// </summary>
    public System.Type? Type {
      get;
      init;
    } = null;

    /// <summary>
    /// Mark a Method, Property, or Archetype as capable of producing a type of model.
    /// </summary>
    /// <param name="type"></param>
    public DoesNotMakeModelAttribute(System.Type? type = null) {
      Type = type;
    }
  }
}
