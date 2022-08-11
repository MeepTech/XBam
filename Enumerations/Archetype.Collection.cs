using Meep.Tech.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Meep.Tech.XBam {

  public abstract partial class Archetype : IEquatable<Archetype> {

    /// <summary>
    /// A Collection of Archetypes.
    /// All archetypes need to be in a collection of some kind.
    /// This is the base non-generic utility class for collections.
    /// </summary>
    public partial class Collection :
        IEnumerable<Archetype> {
      TagedCollection<ITag, Archetype> _byTag
        = new();
      readonly Dictionary<Type, Archetype> _byType
          = new();
      readonly Dictionary<Identity, Archetype> _byIdentity
        = new();
      Dictionary<string, Archetype> _byIdCompiled
        = new();

      /// <summary>
      /// The universe this collection is a part of
      /// </summary>
      public Universe Universe {
        get;
      }

      /// <summary>
      /// Generic collections have no root archetype.
      /// </summary>
      public virtual Archetype RootArchetype
        => null;

      /// <summary>
      /// Generic collections have no root archetype.
      /// </summary>
      public virtual Type RootArchetypeType
        => null;

      /// <summary>
      /// All archetypes registered to this collection
      /// </summary>
      public IEnumerable<Archetype> All
        => _byIdentity.Values;

      /// <summary>
      /// All archetypes registered to this collection by their Id Key.
      /// </summary>
      public IReadOnlyDictionary<string, Archetype> ById
        => _byIdCompiled ??= _byIdentity.ToDictionary(
          entry => entry.Key.Key,
          entry => entry.Value
        );

      /// <summary>
      /// All archetypes registered to this collection by their Identity.
      /// </summary>
      public IReadOnlyDictionary<Identity, Archetype> ByIdentity 
        => _byIdentity;

      /// <summary>
      /// All archetypes:
      /// </summary>
      public IReadOnlyDictionary<Type, Archetype> ByType
        => _byType;

      /// <summary>
      /// All archetypes organized by tags.
      /// </summary>
      public IReadOnlyTagedCollection<ITag, Archetype> ByTags
        => _byTag;

      #region Initialization

      /// <summary>
      /// Make a new archetype collection within the desired universe
      /// </summary>
      internal Collection(Universe universe) {
        Universe = universe;
        universe.Archetypes?._registerCollection(this, RootArchetypeType);
      }

      /// <summary>
      /// Add an archetype to this collection.
      /// This does NOT register the archetype, this can only be used to add a
      /// previously registered archetype to another collection.
      /// TODO: add a protected helper function to archetype class to use this and to add tags.
      /// </summary>
      internal virtual void _add(Archetype archetype) {
        _byIdentity.Add(archetype.Id, archetype);
        if (!_byType.ContainsKey(archetype.GetType())) {
          _byType.Add(archetype.GetType(), archetype);
        }

        _addTags(archetype);
        _clearCompiledCollections();
      }

      internal virtual void _addTags(Archetype archetype, IEnumerable<ITag>? tags = null) {
        tags ??= archetype.Tags;
        if (tags?.Any() ?? false) {
          _byTag.Add(archetype, tags.ToArray());
        }
      }

      internal virtual void _clearCompiledCollections() {
        _byIdCompiled = null;
      }

      #endregion

      #region Deinitialization

      /// <summary>
      ///  used to de-register an archetype.
      /// </summary>
      internal virtual void _remove(Archetype archetype) {
        _byIdentity.Remove(archetype.Id);
        if (_byType.ContainsKey(archetype.GetType())) {
          _byType.Remove(archetype.GetType());
        }
        _byTag.Remove(archetype);

        _clearCompiledCollections();
      }

      #endregion

      #region Accessors

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public TArchetype Get<TArchetype>()
        where TArchetype : Archetype
          => Get(typeof(TArchetype)) as TArchetype;

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public Archetype Get(Type type)
        => _byType[type];

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public Archetype TryToGet(Type type)
        => _byType.TryGetValue(type, out var found)
          ? found
          : null;

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// </summary>
      public bool TryToGet(Type type, out Archetype found)
        => _byType.TryGetValue(type, out found);

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public Archetype Get(Identity id)
        => _byIdentity[id];

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public Archetype Get(string externalId)
        => ById[externalId];

      /// <summary>
      /// Try to get an archetype from this collection by it's externalId.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public Archetype TryToGet(string externalId)
        => ById.TryGetValue(externalId, out var found)
          ? found
          : null;

      /// <summary>
      /// Try to get an archetype from this collection by it's externalId.
      /// </summary>
      public bool TryToGet(string externalId, out Archetype found)
        => ById.TryGetValue(externalId, out found);

      #endregion

      #region Enumeration

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public IEnumerator<Archetype> GetEnumerator()
        => All.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

      #endregion
    }

    /*/// <summary>
    /// A Collection of Archetypes.
    /// - Can be used to make a more specific child collection than Archetype[,].ArchetypeCollection.
    /// </summary>
    public partial class Collection<TModelBase, TArchetypeBase>
      : Archetype<TModelBase, TArchetypeBase>.Collection
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> 
    {

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public Collection(Universe universe = null) 
        : base(universe) {}
    }*/
  }

  public abstract partial class Archetype<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// A Collection of Archetypes.
    /// This is just an Archetype.Collection[&#44;] that is pre-built for the containing Archetype[&#44;] type.
    /// </summary>
    public new partial class Collection
      : Archetype.Collection,
        IEnumerable<TArchetypeBase> 
    {
      IEnumerable<TArchetypeBase> _allCompiled;
      Dictionary<string, TArchetypeBase> _byIdCompiled;
      Dictionary<Identity, TArchetypeBase> _byIdentityCompiled;
      TagedCollection<ITag, TArchetypeBase> _byTagCompiled;
      Dictionary<Type, TArchetypeBase> _byTypeCompiled;

      /// <summary>
      /// The root archetype. This may be null if the root archetype type is abstract.
      /// </summary>
      public override Archetype RootArchetype
        => RootArchetypeType.TryToGetAsArchetype();

      /// <summary>
      ///  The archetype type of the root archetype of this collection (if it's not abstract).
      /// </summary>
      public override Type RootArchetypeType
        => typeof(TArchetypeBase);

      /// <summary>
      /// All archetypes registered to this collection
      /// </summary>
      public new IEnumerable<TArchetypeBase> All
        => _allCompiled ??= base.All.Cast<TArchetypeBase>();

      /// <summary>
      /// All archetypes registered to this collection by their Identity.
      /// </summary>
      public new IReadOnlyDictionary<Identity, TArchetypeBase> ByIdentity {
        get => _byIdentityCompiled ??= base.ByIdentity.ToDictionary(
          archetype => (Identity)archetype.Key,
          archetype => (TArchetypeBase)archetype.Value
        );
      }

      /// <summary>
      /// All archetypes registered to this collection by their Id Key.
      /// </summary>
      public new IReadOnlyDictionary<string, TArchetypeBase> ById
        => _byIdCompiled ??= base.ById.ToDictionary(
          e => e.Key,
          e => (TArchetypeBase)e.Value
        );

      /// <summary>
      /// All archetypes:
      /// </summary>
      public new IReadOnlyDictionary<Type, TArchetypeBase> ByType
        => _byTypeCompiled ??= base.ByType.ToDictionary(
            e => e.Key,
            e => (TArchetypeBase)e.Value
          );

      /// <summary>
      /// All archetypes organized by tags.
      /// </summary>
      public new IReadOnlyTagedCollection<ITag, TArchetypeBase> ByTags
        => _byTagCompiled ??= _compileTags();

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public Collection(Universe universe = null)
        : base(universe ?? Archetypes.DefaultUniverse) { }

      internal override void _clearCompiledCollections() {
        base._clearCompiledCollections();
        _byIdCompiled = null;
        _byIdentityCompiled = null;
        _byTagCompiled = null;
        _byTypeCompiled = null;
        _allCompiled = null;
      }

      #region Accessors

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public new TArchetype Get<TArchetype>()
        where TArchetype : TArchetypeBase
          => base.Get<TArchetype>();

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public new TArchetypeBase Get(Type type)
          => (TArchetypeBase)base.Get(type);

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public TArchetype Get<TArchetype>(Type type)
        where TArchetype : TArchetypeBase
          => (TArchetype)Get(type);

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public new TArchetypeBase TryToGet(Type type)
          => (TArchetypeBase)base.TryToGet(type);

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public TArchetype TryToGet<TArchetype>(Type type)
        where TArchetype : TArchetypeBase
          => (TArchetype)TryToGet(type);

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public bool TryToGet(Type type, out TArchetypeBase found) {
        if(base.TryToGet(type, out var foundType)) {
          found = foundType as TArchetypeBase;
          if(found != null)
            return true;
        }

        found = null;
        return false;
      }

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public bool TryToGet<TArchetype>(Type type, out TArchetype found)
        where TArchetype : TArchetypeBase {
        if(TryToGet(type, out var foundArchetype)) {
          found = foundArchetype as TArchetype;
          if(found != null)
            return true;
        }

        found = null;
        return false;
      }

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public TArchetypeBase Get(Identity id)
          => (TArchetypeBase)base.Get(id);

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public TArchetype Get<TArchetype>(Identity id)
        where TArchetype : TArchetypeBase
          => (TArchetype)Get(id);

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public TArchetype Get<TArchetype>(string externalId)
        where TArchetype : TArchetypeBase
          => (TArchetype)Get(externalId);

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public new TArchetypeBase Get(string externalId)
          => (TArchetypeBase)base.Get(externalId);

      /// <summary>
      /// Try to get an archetype from this collection by it's externalId.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public new TArchetypeBase TryToGet(string externalId)
          => (TArchetypeBase)base.TryToGet(externalId);

      /// <summary>
      /// Try to get an archetype from this collection by it's externalId.
      /// </summary>
      public bool TryToGet(string externalId, out TArchetypeBase found)
        => base.TryToGet(externalId, out var foundItem)
          ? (found = foundItem as TArchetypeBase) != null
          : (found = null) != null;

      /// <summary>
      /// Try to get an archetype from this collection by it's externalId.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public TDesiredArchetype TryToGet<TDesiredArchetype>(string externalId)
        where TDesiredArchetype : TArchetypeBase
          => base.TryToGet(externalId) as TDesiredArchetype;

      /// <summary>
      /// Try to get an archetype from this collection by it's externalId.
      /// </summary>
      public bool TryToGet<TDesiredArchetype>(string externalId, out TDesiredArchetype found)
        where TDesiredArchetype : TArchetypeBase
          => base.TryToGet(externalId, out var foundItem)
            ? (found = foundItem as TDesiredArchetype) != null
            : (found = null) != null;

      #endregion

      #region Enumeration

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public new IEnumerator<TArchetypeBase> GetEnumerator()
        => All.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

      #endregion

      TagedCollection<ITag, TArchetypeBase> _compileTags() {
        var tags = new TagedCollection<ITag, TArchetypeBase>();
        base.ByTags.ForEach(
          t => tags.Add(
            (TArchetypeBase)t.Value,
            t.Key.ToArray()
          ));

        return tags;
      }
    }

    /*/// <summary>
    /// Quick link to the collection for the default universe
    /// </summary>
    public static Collection DefaultCollection
      => (Collection)
        Archetypes.DefaultUniverse.Archetypes.GetCollection(typeof(TArchetypeBase));*/
  }
}
