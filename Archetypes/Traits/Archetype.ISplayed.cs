using Meep.Tech.XBam.Configuration;
using System.Collections.Generic;

namespace Meep.Tech.XBam {

  public partial class Archetype {

    /// <summary>
    /// The base non-generic interface for ISplayed
    /// Is used to automatically generate an archetype for every member of a given enumeration.
    /// </summary>
    public interface ISplayed {

      /// <summary>
      /// Used to construct splayed archetypes
      /// </summary>
      public delegate Archetype Constructor(Enumeration @enum, Universe universe);

      // TODO: these can't be static to work correctly with universes.
      internal static HashSet<Constructor>? _splayedInterfaceTypesThatAllowLazyInitializations
        = new();
      // TODO: these can't be static to work correctly with universes.
      internal static Dictionary<System.Type, Dictionary<System.Type, Dictionary<System.Type, Constructor>>> _splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType
        = new();
      // TODO: these can't be static to work correctly with universes.
      internal static Dictionary<System.Type, HashSet<Enumeration>>? _completedEnumsByInterfaceBase
        = new();

      /// <summary>
      /// Get a splayed archetype
      /// </summary>
      public static Archetype GetArchetypeFor<TArchetype, TEnumeration>(Enumeration enumeration)
        where TEnumeration : Enumeration
        where TArchetype : Archetype, ISplayed<TEnumeration, TArchetype>
          => ISplayed<TEnumeration, TArchetype>.GetForValue((TEnumeration)enumeration);
    }

    /// <summary>
    /// This is a trait that dictates that one of these archetypes should be produced for each item in a given enumeration.
    /// Types extending this cannot be abstract.
    /// This will extend to types that inherit from this archetype, inheriting further from this archetype is not suggested.
    /// If the base archetype is never constructed for a splayed type. The splayed archetypes can be fetched via the System.Type that extends this interface statically. You must call ".FoGetForValuer()" on the correct splayed interface to get a specific sub-archetype specific to one of the enumerations.
    /// </summary>
    public interface ISplayed<TEnumeration, TArchetypeBase>
      : ITrait<ISplayed<TEnumeration, TArchetypeBase>>,
        ISplayed
        where TEnumeration : Enumeration
        where TArchetypeBase : Archetype, ISplayed<TEnumeration, TArchetypeBase>
      {

      string ITrait<ISplayed<TEnumeration, TArchetypeBase>>.TraitName
        => "Splayed";

      string ITrait<ISplayed<TEnumeration, TArchetypeBase>>.TraitDescription
        => $"This Archetype was created by a Parent Archetype, along with one other archetype for each Enumeration in: ${typeof(TEnumeration).FullName}. This archetype's Associated Enum is: {AssociatedEnum}.";

      // TODO: these can't be static to work correctly with universes.
      internal static Dictionary<TEnumeration, TArchetypeBase> _values
        = new();

      /// <summary>
      /// the enum associated with this archetype.
      /// </summary>
      TEnumeration AssociatedEnum {
        get;
        internal protected set;
      }

      internal static void _registerSubArchetype(TArchetypeBase archetype, TEnumeration enumeration) {
        _values[enumeration] = archetype;
      }

      /// <summary>
      /// This will be called for each enumeration loaded at runtime for the enumeration type.
      /// </summary>
      internal protected TArchetypeBase ConstructArchetypeFor(TEnumeration enumeration, Universe universe);

      /// <summary>
      /// Get the specific Archetype for an enum value.
      /// </summary>
      public static TArchetypeBase GetForValue(TEnumeration enumeration)
        => _values[enumeration];
    }
  }

  /// <summary>
  /// Extensions for types that extend Archetype.IBuildOneForEach[TArchetypeBase, TEnumeration] 
  /// </summary>
  public static class SplayedArchetypedExtensions {

    /// <summary>
    /// Get the specific sub-Archetype for an enum value.
    /// </summary>
    public static TArchetype SplayOn<TArchetype, TEnumeration>(this TArchetype splayedArchetype, TEnumeration enumeration)
      where TArchetype : Archetype, Archetype.ISplayed<TEnumeration, TArchetype>
      where TEnumeration : Enumeration
        => Archetype.ISplayed<TEnumeration, TArchetype>._values[enumeration];
  }
}