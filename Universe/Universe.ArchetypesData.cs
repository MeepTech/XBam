using Meep.Tech.Reflection;
using Meep.Tech.XBam.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  public partial class Universe {

    /// <summary>
    /// Used to hold the data for all archetypes
    /// </summary>
    public class ArchetypesData {

      /// <summary>
      /// link to the parent universe
      /// </summary>
      Universe _universe;

      /// <summary>
      /// All archetypes:
      /// </summary>
      public Archetype.Collection All {
        get;
      }

      /// <summary>
      /// All registered Archetype Identities
      /// </summary>
      public IEnumerable<Archetype.Identity> Ids
        => _ids.Values;
      Dictionary<string, Archetype.Identity> _ids
        = new();

      /// <summary>
      /// Ids, indexed by external id value
      /// </summary>
      public IReadOnlyDictionary<string, Archetype.Identity> Id
        => _ids;

      /// <summary>
      /// Dependencies for different types.
      /// </summary>
      public IReadOnlyDictionary<System.Type, IEnumerable<System.Type>> Dependencies
        => _dependencies; internal Dictionary<System.Type, IEnumerable<System.Type>> _dependencies
          = new();

      /// <summary>
      /// Root types for archetypes based on a model type fullname.
      /// </summary>
      internal Dictionary<string, System.Type> _rootArchetypeTypesByBaseModelType
        = new();

      /// <summary>
      /// All Root Archetype Collections.
      /// Doesn't include Branch collections.
      /// </summary>
      public IEnumerable<Archetype.Collection> RootCollections {
        get => _collectionsByRootArchetype.Values;
      }
      internal readonly Dictionary<string, Archetype.Collection> _collectionsByRootArchetype
        = new();

      /// <summary>
      /// All Archetype Collections:
      /// </summary>
      public IEnumerable<Archetype.Collection> Collections {
        get => RootCollections.Concat(_branchedCollectionsByBranchArchetype.Values);
      }
      readonly Dictionary<string, Archetype.Collection> _branchedCollectionsByBranchArchetype
        = new();

      internal ArchetypesData(Universe universe) {
        _universe = universe;
        All = new Archetype.Collection(universe);
        _collectionsByRootArchetype.Add(typeof(Archetype).FullName, All);
      }

      /// <summary>
      /// Get a collection registered to an archetype root:
      /// </summary>
      public Archetype.Collection GetCollection(Archetype root)
        => _collectionsByRootArchetype.TryGetValue(root.Id.Key, out Archetype.Collection collection)
          ? collection
          // recurse until it's found. This should throw a null exception eventually if one isn't found.
          : GetCollection(root.Type.BaseType);

      /// <summary>
      /// Get a collection registered to an archetype root:
      /// </summary>
      public bool TryToGetCollection(System.Type root, out Archetype.Collection found) {
        if (_collectionsByRootArchetype.TryGetValue(root?.FullName, out Archetype.Collection collection)) {
          found = collection;
          return true;
        } // stop if we reached the base
        else if (root.Equals(typeof(object)) || root?.BaseType is null) {
          found = null;
          return false;
        }
        // recurse until it's found.
        else {
          return TryToGetCollection(root.BaseType, out found);
        }
      }

      /// <summary>
      /// Get a collection registered to an archetype type:
      /// </summary>
      public Archetype.Collection GetCollection(System.Type root)
        => root is not null
          ? _collectionsByRootArchetype.TryGetValue(root?.FullName ?? "", out Archetype.Collection collection)
            ? collection
            // recurse until it's found. This should throw a null exception eventually if one isn't found.
            : GetCollection(root.BaseType)
          : throw new Exception($"Invalid Archetype Base Type Provided.");

      /// <summary>
      /// Get a collection registered to an archetype type.
      /// returns null if not found
      /// </summary>
      public Archetype.Collection TryToGetCollection(System.Type root)
        => root is not null
          ? _collectionsByRootArchetype.TryGetValue(root?.FullName ?? "", out Archetype.Collection collection)
            ? collection
            // recurse until it's found. This should throw a null exception eventually if one isn't found.
            : TryToGetCollection(root.BaseType)
          : null;

      /// <summary>
      /// Get the "default" archetype or factory for a given model type.
      /// </summary>
      public Archetype GetDefaultForModel<TModelBase>()
        where TModelBase : IModel
          => GetDefaultForModelOfType(typeof(TModelBase));

      /// <summary>
      /// Get the "default" archetype or factory for a given model type.
      /// </summary>
      public Archetype GetDefaultForModelOfType(System.Type modelBaseType) {
        Archetype archetype;
        if (modelBaseType.IsAssignableToGeneric(typeof(IModel<,>))) {
          if (_rootArchetypeTypesByBaseModelType.TryGetValue(modelBaseType.FullName, out var rootArchetypeType)) {
            if (rootArchetypeType.FullName == null) {
              throw new Loader.CannotInitializeModelException($"Default archetype system type: {rootArchetypeType.Name}, for the model Type: {modelBaseType} can not be recovered as an archetype. The fullname returned by the base archetype System.Type was null, this likely means the base archetype is a generic type.");
            }

            if ((archetype = rootArchetypeType.TryToGetAsArchetype()) != null) {
              return archetype;
            }

            // if we couldn;t make the type into an archetype (it may be a base type), we need to get any default archetype from it's collection:
            Archetype.Collection collection = GetCollection(rootArchetypeType);
            if (collection is null) {
              throw new KeyNotFoundException($"Could not find an archetype collection the model Type: {modelBaseType}");
            }
            if ((archetype = collection.FirstOrDefault()) != null) {
              return archetype;
            }
            else {
              throw new KeyNotFoundException($"Could not find a default archetype for the model Type: {modelBaseType}, in collection: {GetCollection(_rootArchetypeTypesByBaseModelType[modelBaseType.FullName]).GetType().ToFullHumanReadableNameString()}");
            }
          }
          else throw new KeyNotFoundException($"Could not find a Root Archetype for the Base Model Type: {modelBaseType}");
        }
        else {
          return _universe.Models.GetFactory(modelBaseType) as Archetype;
        }
      }

      /// <summary>
      /// Get the traits attached to the given archetype.
      /// </summary>
      public IEnumerable<(string name, string description)> GetTraits(Archetype archetype) {
        foreach (System.Type interfaceTrait in archetype.Type.GetAllInheritedGenericTypes(typeof(ITrait<>))) {
          Type splayedInterfaceType;
          if (typeof(Archetype.ISplayed).IsAssignableFrom(splayedInterfaceType = interfaceTrait.GetGenericArguments().First())) {
            Enumeration @enum = null;
            try {
              @enum = (Enumeration)splayedInterfaceType.GetProperty("AssociatedEnum").GetValue(archetype);
            } catch {
              continue;
            }

            if (splayedInterfaceType.GetGenericArguments().First() != @enum?.EnumBaseType) {
              continue;
            }
          }

          yield return (
            (string)interfaceTrait.GetProperty(nameof(ITrait<BranchAttribute>.TraitName)).GetValue(archetype),
            (string)interfaceTrait.GetProperty(nameof(ITrait<BranchAttribute>.TraitDescription)).GetValue(archetype)
          );
        }

        foreach (Attribute attributeTrait in archetype.GetType().GetCustomAttributes(true).Where(a => a.GetType().IsAssignableToGeneric(typeof(ITrait<>)))) {
          if (attributeTrait is Loader.Settings.DoNotBuildInInitialLoadAttribute) {
            if (Attribute.GetCustomAttribute(
              archetype.Type,
              typeof(Loader.Settings.DoNotBuildInInitialLoadAttribute),
              false
            ) == null) {
              continue;
            }
          }

          foreach (System.Type interfaceTrait in attributeTrait.GetType().GetAllInheritedGenericTypes(typeof(ITrait<>))) {
            yield return (
              (string)interfaceTrait.GetProperty(nameof(ITrait<BranchAttribute>.TraitName)).GetValue(attributeTrait),
              (string)interfaceTrait.GetProperty(nameof(ITrait<BranchAttribute>.TraitDescription)).GetValue(attributeTrait)
            );
          }
        }
      }

      internal void _registerArchetype(Archetype archetype, Archetype.Collection collection) {
        // Register to it's id
        archetype.Id.Archetype = archetype;

        // Register to collection:
        collection._add(archetype);
        archetype.TypeCollection = collection;

        // Add to All:
        All._add(archetype);
        _ids.Add(archetype.Id.Key, archetype.Id);
      }

      internal void _unRegisterArchetype(Archetype archetype) {
        // remove from all:
        All._remove(archetype);
        _ids.Remove(archetype.Id.Key);

        // remove metadata:
        foreach((string modelType, Type archetypeType) in _rootArchetypeTypesByBaseModelType.ToArray()) {
          if (archetypeType == archetype.Type) {
            _rootArchetypeTypesByBaseModelType.Remove(modelType);
          }
        }

        // Register to collection:
        archetype.TypeCollection._remove(archetype);
        archetype.TypeCollection = null;

        // de-register it from it's id
        archetype.Id.Archetype = null;
      }

      internal void _registerCollection(Archetype.Collection collection, Type rootArchetypeType = null) {
        if(!(_universe?.Archetypes is null || _universe?.Models is null || _universe?.Components is null)) {
          if(collection is Archetype.Collection.IBranch) {
            _branchedCollectionsByBranchArchetype.Add(rootArchetypeType.FullName, collection);
          } else
           _collectionsByRootArchetype.Add(rootArchetypeType?.FullName ?? "_all", collection);
        }
      }

      /// <summary>
      /// TODO: impliment?
      /// </summary>
      internal void _unRegisterCollection(Archetype.Collection collection, Type rootArchetypeType = null) {
        if (collection is Archetype.Collection.IBranch) {
          _branchedCollectionsByBranchArchetype.Remove(rootArchetypeType.FullName);
        }
        else
          _collectionsByRootArchetype.Remove(rootArchetypeType?.FullName ?? "_all");
      }
    }
  }
}
