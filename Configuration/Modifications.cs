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

    /// <summary>
    /// Add the given component to the given archetypes After Archetype Loading and Initialization.
    /// These are added after inital components are added, any components are removed, and before any components are updated
    /// These are called before FinishInitialization on the archetype.
    /// </summary>
    protected void AddAfterInitialzation(Archetype.IComponent component, params Archetype[] archetypes)
      => AddAfterInitialzation(archetypes, component);

    /// <summary>
    /// Add the given components to the given archetypes After Archetype Loading and Initialization.
    /// These are added after inital components are added, any components are removed, and before any components are updated
    /// These are called before FinishInitialization on the archetype.
    /// </summary>
    protected void AddAfterInitialzation(IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components) {
      if(Universe.Loader.IsFinished)
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
      components.ForEach(component
        => archetypes.ForEach(archetype => {
          if(archetype.AllowExternalComponentConfiguration) {
            archetype.AddComponent(component);
          }
        }
      ));
    }

    /// <summary>
    /// Remove the given component from the given archetypes After Archetype Loading and Initialization.
    /// These are removed after inital components are added, and before any extra components added or updated
    /// These are called before FinishInitialization on the archetype.
    /// </summary>
    protected void RemoveAfterInitialzation<TComponent>(params Archetype[] archetypes)
      where TComponent : Archetype.IComponent<TComponent>
        => RemoveAfterInitialzation(archetypes, Components<TComponent>.Key);

    /// <summary>
    /// Remove the given component from the given archetypes After Archetype Loading and Initialization.
    /// These are removed after inital components are added, and before any extra components added or updated
    /// These are called before FinishInitialization on the archetype.
    /// </summary>
    protected void RemoveAfterInitialzation(string componentKey, params Archetype[] archetypes)
      => RemoveAfterInitialzation(archetypes, componentKey);

    /// <summary>
    /// Remove the given components from the given archetypes After Archetype Loading and Initialization.
    /// These are removed after inital components are added, and before any extra components added or updated
    /// These are called before FinishInitialization on the archetype.
    /// </summary>
    protected void RemoveAfterInitialzation(IEnumerable<Archetype> archetypes, params string[] componentKeys) {
      if(Universe.Loader.IsFinished)
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");

      componentKeys.ForEach(componentKey
        => archetypes.ForEach(archetype => {
          if(archetype.AllowExternalComponentConfiguration && archetype.HasComponent(componentKey)) {
            archetype.RemoveComponent(componentKey);
          }
        }
      ));
    }

    /// <summary>
    /// Update the given component in the given archetypes After Archetype Loading and Initialization, if they exist.
    /// These are updated after inital components are added, extra components added, any components are removed, but before UpdateOrAddAfterInitialzation
    /// These are called before FinishInitialization on the archetype.
    /// </summary>
    protected void UpdateAfterInitialzation<TComponent>(Func<TComponent, TComponent> updateComponent, params Archetype[] archetypes)
      where TComponent : Archetype.IComponent<TComponent> {
      if(Universe.Loader.IsFinished)
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");

      archetypes.ForEach(archetype => {
        if(archetype.AllowExternalComponentConfiguration && archetype.HasComponent<TComponent>()) {
          archetype.UpdateComponent(updateComponent);
        }
      });
    }

    /// <summary>
    /// Remove the given component from the given archetypes After Archetype Loading and Initialization.
    /// This is called after inital components are added, extra components added, any components are removed, and existing components are updated
    /// ... it should be last before FinishInitialization on the archetype.
    /// </summary>
    protected void AddOrUpdateAfterInitialzation(IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components) {
      if(Universe.Loader.IsFinished)
        throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");

      components.ForEach(component
        => archetypes.ForEach(archetype => {
          if(archetype.AllowExternalComponentConfiguration) {
            archetype.AddOrUpdateComponent(component);
          }
        }
      ));
    }

    #endregion
  }
}
