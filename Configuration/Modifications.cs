using Meep.Tech.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Can be used to modify existing Archetypes who have components open to external edits 
  /// </summary>
  public abstract class Modifications {

    /// <summary>
    /// The universe this is modifiying
    /// </summary>
    public Universe Universe {
      get;
    }

    /// <summary>
    /// Base Ctor, override and keep hidden.
    /// </summary>
    internal protected Modifications(Universe universe) {
      Universe = universe;
    }

    /// <summary>
    /// (Optional) ECSBAM Assemblies this one depends on.
    /// TOOD: impliment for this, and for archetypes.
    /// </summary>
    protected virtual IEnumerable<Assembly> Dependencies {
      get;
    } = Enumerable.Empty<Assembly>();

    /// <summary>
    /// This is called after all Archetypes are loaded in their base form from their libaries initially; in mod load order.
    /// These modifications will then run afterwards. also in mod load order.
    /// This is called before finalize is called on all archetypes.
    /// You can access universe data though this.Universe
    /// </summary>
    protected internal abstract void Initialize();

    #region Configuration

    #region Modify Factory

    /// <summary>
    /// Used to set the factory in mods.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="factory"></param>
    protected void SetFactory<TModel>(IModel.IFactory factory)
      where TModel : IModel<TModel>
        => Universe.Models._setFactory<TModel>(factory);

    #endregion

    #region Modify Components

    #region Archetype Components

    #region Add

    /// <summary>
    /// Add the given component to the given archetypes
    /// </summary>
    protected void AddComponentToArchetypes(Archetype.IComponent component, params Archetype[] archetypes)
      => _addComponentsToArchetypes(Universe, archetypes, component);

    /// <summary>
    /// Add the given components to the given archetypes
    /// </summary>
    protected void AddComponentsToArchetypes(IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components)
      => _addComponentsToArchetypes(Universe, archetypes, components);

    internal static void _addComponentToArchetypes(Universe universe, Archetype.IComponent component, params Archetype[] archetypes)
      => _addComponentsToArchetypes(universe, archetypes, component);

    internal static void _addComponentsToArchetypes(Universe universe, IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components) {
      if(universe.Loader.IsFinished)
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      components.ForEach(component
        => archetypes.ForEach(archetype => {
          if(archetype.AllowExternalComponentConfiguration) {
            archetype.AddComponent(component);
            archetype._initialComponents.Add(component.Key, component);
            if (component is Archetype.IComponent.ILinkedComponent linkedComponent) {
              archetype._modelLinkedComponents.Add(linkedComponent);
            }
          }
        }
      ));
    }

    #endregion

    #region Remove

    /// <summary>
    /// Remove the given component from the given archetypes
    /// </summary>
    protected void RemoveComponentFromArchetypes<TComponent>(params Archetype[] archetypes)
      where TComponent : Archetype.IComponent<TComponent>
        => RemoveComponentsFromArchetypes(archetypes, Components<TComponent>.Key);

    /// <summary>
    /// Remove the given component from the given archetypes
    /// </summary>
    protected void RemoveComponentFromArchetypes(string componentKey, params Archetype[] archetypes)
      => RemoveComponentsFromArchetypes(archetypes, componentKey);

    /// <summary>
    /// Remove the given component from the given archetypes
    /// </summary>
    protected void RemoveComponentFromArchetypes(System.Type componentType, params Archetype[] archetypes)
      => RemoveComponentsFromArchetypes(archetypes, XBam.Components.GetKey(componentType));

    /// <summary>
    /// Remove the given components from the given archetypes
    /// </summary>
    protected void RemoveComponentsFromArchetypes(IEnumerable<Archetype> archetypes, params string[] componentKeys)
      => _removeComponentsFromArchetypes(Universe, archetypes, componentKeys);

    internal static void _removeComponentFromArchetypes<TComponent>(Universe universe, params Archetype[] archetypes)
      where TComponent : Archetype.IComponent<TComponent>
        => _removeComponentsFromArchetypes(universe, archetypes, Components<TComponent>.Key);

    internal static void _removeComponentFromArchetypes(Universe universe, string componentKey, params Archetype[] archetypes)
      => _removeComponentsFromArchetypes(universe, archetypes, componentKey);

    internal static void _removeComponentFromArchetypes(Universe universe, System.Type componentType, params Archetype[] archetypes)
      => _removeComponentsFromArchetypes(universe, archetypes, XBam.Components.GetKey(componentType));

    internal static void _removeComponentsFromArchetypes(Universe universe, IEnumerable<Archetype> archetypes, params string[] componentKeys) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      componentKeys.ForEach(componentKey
        => archetypes.ForEach(archetype => {
          if (archetype.AllowExternalComponentConfiguration && archetype.TryToGetComponent(componentKey, out var component)) {
            archetype.RemoveComponent(componentKey);
            archetype._initialComponents.Remove(componentKey);
            if (component is Archetype.IComponent.ILinkedComponent linkedComponent) {
              archetype._modelLinkedComponents.Remove(linkedComponent);
            }
          }
        }
      ));
    }

    #endregion

    #region Update

    /// <summary>
    /// Update the existing component from the given archetypes.
    /// </summary>
    protected void UpdateComponentForArchetypes<TComponent>(Func<TComponent, TComponent> updateComponent, params Archetype[] archetypes)
      where TComponent : Archetype.IComponent<TComponent> {
      if (Universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      archetypes.ForEach(archetype => {
        if (archetype.AllowExternalComponentConfiguration && archetype.HasComponent<TComponent>()) {
          archetype.UpdateComponent(updateComponent);
        }
      });
    }

    /// <summary>
    /// Update the existing component from the given archetypes.
    /// </summary>
    protected void UpdateComponentsForArchetypes(Dictionary<System.Type, Func<Archetype.IComponent, Archetype.IComponent>> updateComponentsByType, params Archetype[] archetypes)
      => _updateComponentsForArchetypes(Universe, updateComponentsByType, archetypes);

    /// <summary>
    /// Update or add the given component from the given archetypes.
    /// </summary>
    protected void UpdateOrAddComponentsForArchetypes(IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components)
      => _updateOrAddComponentsForArchetypes(Universe, archetypes, components);

    internal static void _updateOrAddComponentsForArchetypes(Universe universe, IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      components.ForEach(component
        => archetypes.ForEach(archetype => {
          if (archetype.AllowExternalComponentConfiguration) {
            archetype.AddOrUpdateComponent(component);
          }
        }
      ));
    }

    internal static void _updateComponentsForArchetypes(Universe universe, Dictionary<System.Type, Func<Archetype.IComponent, Archetype.IComponent>> updateComponentsByType, params Archetype[] archetypes) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      updateComponentsByType.ForEach(e => {
        var (componentType, componentUpdater) = e;
        archetypes.ForEach(archetype => {
          if (archetype.AllowExternalComponentConfiguration && archetype.TryToGetComponent(componentType, out var component)) {
            archetype.UpdateComponent(componentUpdater(component));
          }
        });
      });
    }

    #endregion

    #endregion

    #region Model Components

    #region Add

    /// <summary>
    /// Add the given initial model components to the given archetypes
    /// </summary>
    protected void AddInitialModelComponentsToArchetypes(IEnumerable<Archetype> archetypes, Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> componentBuilders)
      => _addInitialModelComponentsToArchetypes(Universe, archetypes, componentBuilders);

    /// <summary>
    /// Add the given initial model component to the given archetypes
    /// </summary>
    protected void AddInitialModelComponentToArchetypes(IEnumerable<Archetype> archetypes, string componentKey, Func<IComponent.IBuilder, IModel.IComponent> componentBuilder)
      => _addInitialModelComponentToArchetypes(Universe, archetypes, componentKey, componentBuilder);

    internal static void _addInitialModelComponentsToArchetypes(Universe universe, IEnumerable<Archetype> archetypes, Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> componentBuilders) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      componentBuilders.ForEach(componentBuilder
        => archetypes.ForEach(archetype => {
          if (archetype.AllowExternalComponentConfiguration) {
            archetype._initialUnlinkedModelComponents.Add(componentBuilder.Key, componentBuilder.Value);
          }
        }
      ));
    }

    internal static void _addInitialModelComponentToArchetypes(Universe universe, IEnumerable<Archetype> archetypes, string componentKey, Func<IComponent.IBuilder, IModel.IComponent> componentBuilder) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      archetypes.ForEach(archetype => {
        if (archetype.AllowExternalComponentConfiguration) {
          archetype._initialUnlinkedModelComponents.Add(componentKey, componentBuilder);
        }
      });
    }

    #endregion

    #region Remove

    /// <summary>
    /// Remove the given component from the given archetypes
    /// </summary>
    protected void RemoveInitialModelComponentFromArchetypes<TComponent>(params Archetype[] archetypes)
      where TComponent : IModel.IComponent<TComponent>
        => RemoveInitialModelComponentsFromArchetypes(archetypes, Components<TComponent>.Key);

    /// <summary>
    /// Remove the given component from the given archetypes
    /// </summary>
    protected void RemoveInitialModelComponentFromArchetypes(string componentKey, params Archetype[] archetypes)
      => RemoveInitialModelComponentsFromArchetypes(archetypes, componentKey);

    /// <summary>
    /// Remove the given component from the given archetypes
    /// </summary>
    protected void RemoveInitialModelComponentFromArchetypes(System.Type componentType, params Archetype[] archetypes)
      => RemoveInitialModelComponentsFromArchetypes(archetypes, Components.GetKey(componentType));

    /// <summary>
    /// Remove the given components from the given archetypes
    /// </summary>
    protected void RemoveInitialModelComponentsFromArchetypes(IEnumerable<Archetype> archetypes, params string[] componentKeys)
      => _removeInitialModelComponentsFromArchetypes(Universe, archetypes, componentKeys);

    internal static void _removeInitialModelComponentFromArchetypes<TComponent>(Universe universe, params Archetype[] archetypes)
      where TComponent : Archetype.IComponent<TComponent>
        => _removeInitialModelComponentsFromArchetypes(universe, archetypes, Components<TComponent>.Key);

    internal static void _removeInitialModelComponentFromArchetypes(Universe universe, string componentKey, params Archetype[] archetypes)
      => _removeInitialModelComponentsFromArchetypes(universe, archetypes, componentKey);

    internal static void _removeInitialModelComponentFromArchetypes(Universe universe, System.Type componentType, params Archetype[] archetypes)
      => _removeInitialModelComponentsFromArchetypes(universe, archetypes, Components.GetKey(componentType));

    internal static void _removeInitialModelComponentsFromArchetypes(Universe universe, IEnumerable<Archetype> archetypes, params string[] componentKeys) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      componentKeys.ForEach(componentKey
        => archetypes.ForEach(archetype => {
          if (archetype.AllowExternalComponentConfiguration && archetype.TryToGetComponent(componentKey, out var component)) {
            archetype.RemoveComponent(componentKey);
            archetype._initialUnlinkedModelComponents.Remove(componentKey);
          }
        }
      ));
    }

    #endregion

    #region Update

    /// <summary>
    /// Update the existing component from the given archetypes.
    /// </summary>
    protected void UpdateInitialModelComponentForArchetypes<TComponent>(Func<IComponent.IBuilder, TComponent> newComponentInitializer, params Archetype[] archetypes)
      where TComponent : IModel.IComponent<TComponent>
        => _updateInitialModelComponentsForArchetypes(
          Universe,
          new Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>>() {
            {Components<TComponent>.Key, b => newComponentInitializer(b) }
          },
          archetypes
        );

    /// <summary>
    /// Update the existing component from the given archetypes.
    /// </summary>
    protected void UpdateInitialModelComponentsForArchetypes(Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> updateComponentsByType, params Archetype[] archetypes)
      => _updateInitialModelComponentsForArchetypes(
        Universe,
        updateComponentsByType,
        archetypes
      );

    /// <summary>
    /// Update or add the given component from the given archetypes.
    /// </summary>
    protected void UpdateOrAddInitialModelComponentsForArchetypes(IEnumerable<Archetype> archetypes, Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> newComponentInitializers)
      => _updateOrAddInitialModelComponentsForArchetypes(Universe, newComponentInitializers, archetypes.ToArray());

    /// <summary>
    /// Update the existing component from the given archetypes.
    /// </summary>
    protected void UpdateInitialModelComponentForArchetypes<TComponent>(Func<Func<IComponent.IBuilder, TComponent>, IComponent.IBuilder, TComponent> newComponentInitializer, params Archetype[] archetypes)
      where TComponent : IModel.IComponent<TComponent>
        => _updateInitialModelComponentsForArchetypes(
          Universe,
          new Dictionary<string, Func<Func<IComponent.IBuilder, IModel.IComponent>, IComponent.IBuilder, IModel.IComponent>>() {
            {Components<TComponent>.Key, (o, b) => newComponentInitializer(bldr => (TComponent)o(bldr), b) }
          },
          archetypes
        );

    /// <summary>
    /// Update the existing component from the given archetypes.
    /// </summary>
    protected void UpdateInitialModelComponentsForArchetypes(Dictionary<string, Func<Func<IComponent.IBuilder, IModel.IComponent>, IComponent.IBuilder, IModel.IComponent>> updateComponentsByType, params Archetype[] archetypes)
      => _updateInitialModelComponentsForArchetypes(
        Universe,
        updateComponentsByType,
        archetypes
      );

    /// <summary>
    /// Update or add the given component from the given archetypes.
    /// </summary>
    protected void UpdateOrAddInitialModelComponentsForArchetypes(IEnumerable<Archetype> archetypes, Dictionary<string, Func<Func<IComponent.IBuilder, IModel.IComponent>, IComponent.IBuilder, IModel.IComponent>> newComponentInitializers)
      => _updateOrAddInitialModelComponentsForArchetypes(Universe, newComponentInitializers, archetypes.ToArray());

    internal static void _updateInitialModelComponentsForArchetypes(Universe universe, Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> newComponentInitializers, params Archetype[] archetypes) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      newComponentInitializers.ForEach(newComponentInitializer =>
        archetypes.ForEach(archetype => {
          (string key, Func<IComponent.IBuilder, IModel.IComponent> initializer) = newComponentInitializer;
          if (archetype.AllowExternalComponentConfiguration && archetype.InitialUnlinkedModelComponents.ContainsKey(key)) {
            archetype._initialUnlinkedModelComponents[key] = b => initializer(b);
          }
        })
      );
    }

    internal static void _updateOrAddInitialModelComponentsForArchetypes(Universe universe, Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> newComponentInitializers, params Archetype[] archetypes) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      newComponentInitializers.ForEach(newComponentInitializer =>
        archetypes.ForEach(archetype => {
          (string key, Func<IComponent.IBuilder, IModel.IComponent> initializer) = newComponentInitializer;
          if (archetype.AllowExternalComponentConfiguration) {
            archetype._initialUnlinkedModelComponents[key] = b => initializer(b);
          }
        })
      );
    }

    internal static void _updateInitialModelComponentsForArchetypes(Universe universe, Dictionary<string, Func<Func<IComponent.IBuilder, IModel.IComponent>, IComponent.IBuilder, IModel.IComponent>> newComponentInitializers, params Archetype[] archetypes) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      newComponentInitializers.ForEach(newComponentInitializer =>
        archetypes.ForEach(archetype => {
          (string key, Func<Func<IComponent.IBuilder, IModel.IComponent>, IComponent.IBuilder, IModel.IComponent> initializer) = newComponentInitializer;
          if (archetype.AllowExternalComponentConfiguration && archetype.InitialUnlinkedModelComponents.TryGetValue(key, out var original)) {
            archetype._initialUnlinkedModelComponents[key] = b => initializer(original, b);
          }
        })
      );
    }

    internal static void _updateOrAddInitialModelComponentsForArchetypes(Universe universe, Dictionary<string, Func<Func<IComponent.IBuilder, IModel.IComponent>, IComponent.IBuilder, IModel.IComponent>> newComponentInitializers, params Archetype[] archetypes) {
      if (universe.Loader.IsFinished) {
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      }

      newComponentInitializers.ForEach(newComponentInitializer =>
        archetypes.ForEach(archetype => {
          (string key, Func<Func<IComponent.IBuilder, IModel.IComponent>, IComponent.IBuilder, IModel.IComponent> initializer) = newComponentInitializer;
          if (archetype.AllowExternalComponentConfiguration) {
            var original = archetype._initialUnlinkedModelComponents.TryToGet(key);
            archetype._initialUnlinkedModelComponents[key] = b => initializer(original, b);
          }
        })
      );
    }

    #endregion

    #endregion

    #endregion

    #endregion
  }
}
