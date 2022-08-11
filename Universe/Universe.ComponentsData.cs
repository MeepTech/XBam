using Meep.Tech.Collections.Generic;
using Meep.Tech.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  public partial class Universe {

    /// <summary>
    /// Data for all components in the universe
    /// </summary>
    public class ComponentsData {

      /// <summary>
      /// The number of different loaded model types.
      /// </summary>
      public int Count
        => _baseTypes.Count;

      /// <summary>
      /// Dependencies for different types.
      /// </summary>
      public IReadOnlyDictionary<System.Type, IEnumerable<System.Type>> Dependencies
        => _dependencies; internal Dictionary<System.Type, IEnumerable<System.Type>> _dependencies
          = new();

      /// <summary>
      /// All base types, easily accessable
      /// </summary>
      public IEnumerable<System.Type> All
        => _baseTypes.Values;

      /// <summary>
      /// Cached model base types
      /// </summary>
      internal Dictionary<string, System.Type> _baseTypes
        = new();

      /// <summary>
      /// Cached model base types
      /// </summary>
      internal Dictionary<string, System.Type> _byKey
        = new();

      /// <summary>
      /// Cached linked types
      /// </summary>
      internal Map<System.Type, System.Type> _archetypeComponentsLinkedToModelComponents
        = new();

      Universe _universe;

      internal ComponentsData(Universe universe) {
        _universe = universe;
      }

      /// <summary>
      /// Get a component type by it's key
      /// </summary>
      public Type Get(string key)
        => _byKey[key];

      /// <summary>
      /// Get the builder for a given component by type.d
      /// </summary>
      public IComponent.IFactory GetFactory(Type type)
        => (IComponent.IFactory)_universe.Models.GetFactory(type);

      /// <summary>
      /// Get the builder for a given component by type.d
      /// </summary>
      public IComponent.IFactory GetFactory<TComponent>()
        where TComponent : IComponent<TComponent>
          => (IComponent.IFactory)_universe.Models.GetFactory<TComponent>();

      /// <summary>
      /// Set the builder factory for a type of component.
      /// TODO: Must be doen during init or static ctor calls
      /// </summary>
      public void SetFactory<TComponent>(IComponent.IFactory factory)
        where TComponent : IComponent<TComponent>
          => _universe.Models._factoriesByModelType[typeof(TComponent)] 
            = factory;

      /// <summary>
      /// Get the base model type of this component type.
      /// </summary>
      public System.Type GetBaseType(System.Type type)
        => _baseTypes.TryGetValue(type.FullName, out System.Type foundType)
          ? foundType
          : _baseTypes[type.FullName] = type.GetFirstInheritedGenericTypeParameters(typeof(IComponent<>)).First();
    }
  }
}
