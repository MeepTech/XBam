using Meep.Tech.Collections.Generic;
using Meep.Tech.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  public partial class Universe {

    /// <summary>
    /// Data pertaining to enumerations
    /// </summary>
    public class EnumerationData {
      internal readonly Dictionary<string, ITag> _withExtraContext
        = new();
      internal readonly Dictionary<string, Dictionary<string, ITag>> _tagsByTypeAndKey
        = new();
      readonly Dictionary<string, Dictionary<object, Enumeration>> _byType
          = new();

      /// <summary>
      /// The universe this data is for
      /// </summary>
      public Universe Universe {
        get;
      }

      internal EnumerationData(Universe universe) {
        Universe = universe;
      }

      /// <summary>
      /// All enumerations indexed by type.
      /// GetAllForType is faster.
      /// </summary>
      public IReadOnlyDictionary<System.Type, IEnumerable<Enumeration>> ByType
        => _byType.ToDictionary(
          e => TypeExtensions.GetTypeByFullName(e.Key),
          e => (IEnumerable<Enumeration>)e.Value.Values
        ); 

      /// <summary>
      /// Get all enumerations of a given type
      /// </summary>
      public IEnumerable<Enumeration> GetAllByType(System.Type type)
        => GetAllByType(type.FullName);

      /// <summary>
      /// Get all enumerations of a given type
      /// </summary>
      public IEnumerable<Enumeration> GetAllByType(string typeFullName)
        => _byType[typeFullName].Values;

      /// <summary>
      /// Get all enumerations of a given type
      /// </summary>
      public IEnumerable<Enumeration> GetAllByType<TEnumeration>()
        where TEnumeration : Enumeration
          => GetAllByType(typeof(TEnumeration));

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public Enumeration Get(string typeFullName, object externalId)
          => _byType[typeFullName][externalId];

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public TEnumeration Get<TEnumeration>(object externalId)
        where TEnumeration : Enumeration
          => (TEnumeration)Get(typeof(TEnumeration).FullName, externalId);

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public Enumeration Get(System.Type enumType, object externalId)
          => Get(enumType.FullName, externalId);

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public bool TryToGet(System.Type enumType, object externalId, out Enumeration found) {
        if (_byType.TryGetValue(enumType.FullName, out var _found)) {
          if (_found.TryGetValue(externalId, out found)) {
            return true;
          }
        }

        found = null;
        return false;
      }

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public bool TryToGet<TEnumeration>(object externalId, out TEnumeration found) where TEnumeration : Enumeration {
        if (_byType.TryGetValue(typeof(TEnumeration).FullName, out var _found)) {
          if (_found.TryGetValue(externalId, out var foundEnum)) {
            found = foundEnum as TEnumeration;
            return foundEnum is not null;
          }
        }

        found = null;
        return false;
      }

      internal void _register(Enumeration enumeration) {
        _addEnumToAllMatchingTypeCollections(enumeration);
        _checkForAndInitializeLazilySplayedArchetypes(enumeration);
      }

      void _addEnumToAllMatchingTypeCollections(Enumeration enumeration) {
        var enumType = enumeration.GetType();
        while (enumType.IsAssignableToGeneric(typeof(Enumeration<>)) && (!enumType.IsGenericType || (enumType.GetGenericTypeDefinition() != typeof(Enumeration<>)))) {
          if (_byType.TryGetValue(enumType.FullName, out var found)) {
            found.Add(
              enumeration.ExternalId,
             enumeration
            );
          }
          else
            _byType[enumType.FullName] = new Dictionary<object, Enumeration> {
              {enumeration.ExternalId, enumeration }
            };

          enumType = enumType.BaseType;
        }
      }

      void _checkForAndInitializeLazilySplayedArchetypes(Enumeration enumeration) {
        if (Universe.Loader.IsFinished && Universe.Loader.Options.AllowRuntimeTypeRegistrations) {
          if (Archetype.ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType.TryGetValue(enumeration.EnumBaseType, out var potentialLazySplayedTypes)) {
            if (potentialLazySplayedTypes.TryGetValue(enumeration.GetType(), out var lazySplayedArchetypeCtors)) {
              lazySplayedArchetypeCtors.ForEach(c => {
                Universe.ExtraContexts.OnLoaderArchetypeInitializationStart(enumeration.GetType(), true);
                var a = c.Value(enumeration);
                Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(true, a.GetType(), a, null, true);
              });
            }
          }
        }
      }

      internal void _deRegister(Enumeration enumeration) {
        var enumType = enumeration.GetType();
        while (enumType.IsAssignableToGeneric(typeof(Enumeration<>)) && (!enumType.IsGenericType || (enumType.GetGenericTypeDefinition() != typeof(Enumeration<>)))) {
          if(_byType.TryGetValue(enumType.FullName, out var found)) {
            found.Remove(enumeration.ExternalId);
          }

          enumType = enumType.BaseType;
        }
      }
    }
  }
}
