using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Meep.Tech.XBam {

  public abstract partial class Archetype {

    /// <summary>
    /// An Id unique to each Archetype.
    /// Can be used as a static key.
    /// This is a base, non-absract class for utility.
    /// </summary>
    public abstract class Identity : Enumeration<Identity> {

      /// <summary>
      /// The Name of this Identity.
      /// By default, this is used to generate the key.
      /// </summary>
      public string Name {
        get;
      }

      /// <summary>
      /// A Univerally Unique Key for this Achetype Identity.
      /// </summary>
      public string Key {
        get => _castKey ??= ExternalId as string;
      } string _castKey;

      /// <summary>
      /// The 'Default' Universe's archetype this id is for
      /// </summary>
      /// <seealso cref="ArchetypeFor"/>
      [JsonIgnore]
      public Archetype Archetype
        => Universe.Default.Archetypes.All.ById[Key];

      /// <summary>
      /// The archetype for a given universe
      /// </summary>
      /// <seealso cref="Archetype"/>
      public abstract IReadOnlyDictionary<Universe, Archetype> ArchetypeFor {
        get;
      }

      /// <summary>
      /// Can be used as an internal value to index this identity.
      /// May change between runtimes/runs of a program.
      /// </summary>
      [JsonIgnore]
      public int InternalIndex
        => InternalId;

      /// <summary>
      /// Make a new ID.
      /// </summary>
      protected Identity(
        string name,
        string key = null
      ) : base(key ?? name) {
        Name = name;
      }

      ///<summary><inheritdoc/></summary>
      protected internal override object UniqueIdCreationLogic(object uniqueIdentifier) 
        => Regex.Replace($"{uniqueIdentifier}", @"\s+", "");

      internal abstract void _registerForUniverse(Universe universe, Archetype archetype);

      internal abstract void _deRegisterForUniverse(Universe universe);
    }
  }

  /// <summary>
  /// An Id unique to each Archetype.
  /// Can be used as a static key.
  /// </summary>
  public partial class Archetype<TModelBase, TArchetypeBase>
    : Archetype
    where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {

    /// <summary>
    /// An Id unique to each Archetype.
    /// Can be used as a static key.
    /// </summary>
    public new class Identity : Archetype.Identity {
      Dictionary<Universe, Archetype> _archetypesByUniverse
        = new();

      /// <summary>
      /// Used to get the archetype for a given universe.
      /// </summary>
      public override IReadOnlyDictionary<Universe, Archetype> ArchetypeFor 
        => _archetypesByUniverse;

      /// <summary>
      /// Make a new identiy for this Archetype Base Type
      /// </summary>
      /// <param name="name">Used to generate the final part of the key. Spaces are removed before then.</param>
      /// <param name="keyPrefixEndingAdditions">Added to the key right before the end here: Type..{keyPrefixEndingAdditions}.name</param>
      /// <param name="keyOverride">can be used to fully replace the key if you want a different key and name</param>
      public Identity(string name, string keyPrefixEndingAdditions = null, string keyOverride = null) 
        : base(name, keyOverride ?? $"{typeof(TModelBase).FullName}.{keyPrefixEndingAdditions ?? ""}{(string.IsNullOrEmpty(keyPrefixEndingAdditions) ? "" : ".")}{name}") {}

      /// <summary>
      /// Make a new identiy for this Archetype Base Type
      /// </summary>
      /// <param name="name">Used to generate the final part of the key. Spaces are removed before then.</param>
      /// <param name="keyPrefixEndingAdditions">Added to the key right before the end here: Type..{keyPrefixEndingAdditions}.name</param>
      /// <param name="baseKeyStringOverride">Overrides the type fullname.</param>
      /// <param name="keyOverride">can be used to fully replace the key if you want a different key and name</param>
      protected Identity(string name, string keyPrefixEndingAdditions, string baseKeyStringOverride, string keyOverride = null) 
        : base(name, keyOverride ?? $"{baseKeyStringOverride ?? typeof(TModelBase).FullName}{(baseKeyStringOverride != "" ? "." :"")}{keyPrefixEndingAdditions ?? ""}{(string.IsNullOrEmpty(keyPrefixEndingAdditions) ? "" : ".")}{name}") {}

      internal override void _registerForUniverse(Universe universe, Archetype archetype) {
        _archetypesByUniverse.Add(universe, archetype);
      }

      internal override void _deRegisterForUniverse(Universe universe) {
        _archetypesByUniverse.Remove(universe);
      }
    }
  }
}
