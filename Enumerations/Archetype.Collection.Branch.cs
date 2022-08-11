namespace Meep.Tech.XBam {

  /*public abstract partial class Archetype<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {

    public partial class Collection {

      /// <summary>
      /// A Branch of an Collection of Archetypes, used to catalog a branch of the archetype tree.
      /// </summary>
      public class Branch<TNewArchetypeBase>
        : Collection<TModelBase, TArchetypeBase>,
          IEnumerable<TNewArchetypeBase>,
          IBranch
        where TNewArchetypeBase : TArchetypeBase
      {

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Archetype RootArchetype
          => typeof(TNewArchetypeBase).TryToGetAsArchetype();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override System.Type RootArchetypeType
          => typeof(TNewArchetypeBase);

        /// <summary>
        /// All archetypes registered to this collection
        /// </summary>
        public new IEnumerable<TNewArchetypeBase> All
          => _byIdentity.Values.Cast<TNewArchetypeBase>();

        /// <summary>
        /// All archetypes registered to this collection by their Identity.
        /// </summary>
        public new IReadOnlyDictionary<Identity, TNewArchetypeBase> ById {
          get => !RootArchetype.AllowInitializationsAfterLoaderFinalization ? (_compiledById ??= _byIdentity.ToDictionary(
            archetype => (Identity)Universe.Archetypes.Id[archetype.Key],
            archetype => (TNewArchetypeBase)archetype.Value
          )) : _byIdentity.ToDictionary(
            archetype => (Identity)Universe.Archetypes.Id[archetype.Key],
            archetype => (TNewArchetypeBase)archetype.Value
          );
        } Dictionary<Identity, TNewArchetypeBase> _compiledById;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Branch(Universe universe = null) 
          : base(universe) {
        }

        #region Accessors

        /// <summary>
        /// Get an archetype from this collection by it's type.
        /// </summary>
        public new TArchetype Get<TArchetype>()
          where TArchetype : TNewArchetypeBase
            => base.Get<TArchetype>();

        /// <summary>
        /// Get an archetype from this collection by it's type.
        /// </summary>
        public new TNewArchetypeBase Get(System.Type type)
            => (TNewArchetypeBase)base.Get(type);

        /// <summary>
        /// Get an archetype from this collection by it's type.
        /// </summary>
        public new TArchetype Get<TArchetype>(System.Type type)
          where TArchetype : TNewArchetypeBase
            => (TArchetype)Get(type);

        /// <summary>
        /// Try to get an archetype from this collection by it's type.
        /// Returns null on failure instead of throwing.
        /// </summary>
        public new TNewArchetypeBase TryToGet(System.Type type)
            => (TNewArchetypeBase)base.TryToGet(type);

        /// <summary>
        /// Try to get an archetype from this collection by it's type.
        /// Returns null on failure instead of throwing.
        /// </summary>
        public new TArchetype TryToGet<TArchetype>(System.Type type)
          where TArchetype : TNewArchetypeBase
            => (TArchetype)TryToGet(type);

        /// <summary>
        /// Try to get an archetype from this collection by it's type.
        /// Returns null on failure instead of throwing.
        /// </summary>
        public bool TryToGet(System.Type type, out TNewArchetypeBase found) {
          if(base.TryToGet(type, out var foundType)) {
            found = foundType as TNewArchetypeBase;
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
        public new bool TryToGet<TArchetype>(System.Type type, out TArchetype found)
          where TArchetype : TNewArchetypeBase {
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
        public new TNewArchetypeBase Get(Identity id)
          => (TNewArchetypeBase)base.Get(id);

        /// <summary>
        /// Get an archetype from it's Id.
        /// </summary>
        public TArchetype Get<TArchetype>(Identity id)
          where TArchetype : TNewArchetypeBase
            => (TArchetype)Get(id);

        /// <summary>
        /// Get an archetype from it's Id.
        /// </summary>
        public new TArchetype Get<TArchetype>(string externalId)
          where TArchetype : TNewArchetypeBase
            => (TArchetype)Get(externalId);

        /// <summary>
        /// Get an archetype from it's Id.
        /// </summary>
        public new TNewArchetypeBase Get(string externalId)
            => (TNewArchetypeBase)base.Get(externalId);

        #endregion

        #region Enumeration

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public new IEnumerator<TNewArchetypeBase> GetEnumerator()
          => All.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
          => GetEnumerator();

        #endregion
      }
    }
  }*/
}
