using System.Collections.Generic;

namespace Meep.Tech.XBam {

  /// <summary>
  /// Static data for archetyps in the DefaultUniverse
  /// </summary>
  public static class Archetypes {

    /// <summary>
    /// The default universe to use for static access shortcuts
    /// </summary>
    public static Universe DefaultUniverse {
      get;
      set;
    }

    #region Data Access

    /// <summary>
    /// All archetypes:
    /// </summary>
    public static Archetype.Collection All
      => DefaultUniverse.Archetypes.All;

    /// <summary>
    /// All registered Archetype Identities
    /// </summary>
    public static IEnumerable<Archetype.Identity> Ids
      => DefaultUniverse.Archetypes.Ids;

    /// <summary>
    /// Ids, indexed by external id value
    /// </summary>
    public static IReadOnlyDictionary<string , Archetype.Identity> Id
      => DefaultUniverse.Archetypes.Id;

    /// <summary>
    /// All archetypes:
    /// </summary>
    public static IEnumerable<Archetype.Collection> Collections 
      => DefaultUniverse.Archetypes.Collections;

    /// <summary>
    /// Get a collection registered to an archetype root:
    /// </summary>
    public static Archetype.Collection GetCollectionFor(Archetype root)
      => DefaultUniverse.Archetypes.GetCollection(root);

    #endregion

    #region Conversions

    /// <summary>
    /// Get a system type as an archetype.
    /// </summary>
    public static Archetype AsArchetype(this System.Type type)
      => All.Get(type);

    /// <summary>
    /// Get a system type as an archetype.
    /// </summary>
    public static Archetype TryToGetAsArchetype(this System.Type type)
      => All.TryToGet(type);

    /// <summary>
    /// Get a system type as an archetype.
    /// </summary>
    public static TArchetype AsArchetype<TArchetype>(this System.Type type)
      where TArchetype : Archetype
        => All.Get<TArchetype>();

    #endregion
  }

  /// <summary>
  /// Static data for archetypes in the DefaultUniverse, by archetype class.
  /// </summary>
  public static class Archetypes<TArchetype> 
    where TArchetype : Archetype {

    /// <summary>
    /// The instance of this archetype type
    /// </summary>
    public static TArchetype Instance
      => (TArchetype)typeof(TArchetype).AsArchetype();

    /// <summary>
    /// The instance of this archetype type
    /// </summary>
    public static TArchetype Archetype
      => Instance;

    /// <summary>
    /// The instance of this archetype type
    /// </summary>
    public static TArchetype _
      => Instance;

    /// <summary>
    /// The instance of this archetype type
    /// This is because ._. looks sad sometimes, so you can use .u. to cheer them up.
    /// </summary>
    public static TArchetype u
      => Instance;

    /// <summary>
    /// The instance of this archetype type
    /// uwu
    /// </summary>
    public static TArchetype w
      => Instance;

    /// <summary>
    /// Helper to get the collection for this archetype:
    /// </summary>
    public static Archetype.Collection Collection
      => Archetypes.GetCollectionFor(typeof(TArchetype).AsArchetype());
  }
}
