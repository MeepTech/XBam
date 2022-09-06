using System;
using System.Collections.Generic;
using System.Linq;
using Meep.Tech.Collections.Generic;

namespace Meep.Tech.XBam {

  /// <summary>
  /// This represents an object with some kind of component storage.
  /// To override component get logic on objects, you must override them implicitly via IReadableComponentStorage
  /// Overriding in the object itself without using implicits may not change the logic everywhere that's needed, 
  /// ... only do that if you want to add logic and use the base functionality too.
  /// </summary>
  public partial interface IReadableComponentStorage : IResource {

    /// <summary>
    /// Internal holder for components data
    /// </summary>
    internal protected Dictionary<string, IComponent> ComponentsByBuilderKey {
      get;
    }

    /// <summary>
    /// Internal holder for components data
    /// </summary>
    internal protected Dictionary<System.Type, ICollection<IComponent>> ComponentsWithWaitingContracts {
      get;
    }

    /// <summary>
    /// Equality Logic
    /// TODO: impliment this in the WithComponents models
    /// </summary>
    public static bool Equals(IReadableComponentStorage model, IReadableComponentStorage other) {
      foreach((_, IComponent dataComponent) in model.ComponentsByBuilderKey) {
        // check each child component that we need to:
        if(Meep.Tech.XBam.Components.GetFactory(dataComponent.GetType()).IncludeInParentModelEqualityChecks) {
          // if the other item doesn't have any components, is missing this component, or the other component doesn't equal the one from this model, it's not ==
          if(!other.TryToGetComponent(dataComponent, out IComponent otherComponent)
            || !dataComponent.Equals(otherComponent)
          ) {
            return false;
          }
        }
      }

      return other is not null;
    }

    #region Implicit Implementations

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public IComponent GetComponent(string key)
      => ReadableComponentStorageExtensions.GetComponent(this, key);

    /// <summary>
    /// Check if this has a component matching the given object.
    /// </summary>
    public bool HasComponent(string componentKey)
      => ReadableComponentStorageExtensions.HasComponent(this, componentKey);

    /// <summary>
    /// Get a component if this has that given component
    /// Overriding this overrides Get component and all other Has component functionalities
    /// </summary>
    public bool TryToGetComponent(string componentKey, out IComponent component)
      => ReadableComponentStorageExtensions.TryToGetComponent(this, componentKey, out component);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public bool TryToGetComponent(System.Type componentType, out IComponent component)
      => ReadableComponentStorageExtensions.TryToGetComponent(this, componentType, out component);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public bool TryToGetComponent<TComponent>(out TComponent component)
      where TComponent : IComponent<TComponent>
        => ReadableComponentStorageExtensions.TryToGetComponent<TComponent>(this, out component);

    /// <summary>
    /// Check if this has a given component by base type
    /// </summary>
    public bool HasComponent(System.Type componentType)
      => ReadableComponentStorageExtensions.HasComponent(this, componentType);

    /// <summary>
    /// Check if this has a component matching the given object
    /// </summary>
    public bool HasComponent(IComponent componentModel)
      => ReadableComponentStorageExtensions.HasComponent(this, componentModel);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public bool TryToGetComponent(IComponent componentModel, out IComponent component)
      => ReadableComponentStorageExtensions.TryToGetComponent(this, componentModel, out component);

    #endregion
  }

  public partial interface IModel {

    /// <summary>
    /// Readble Component Storage more specific to a model.
    /// Includes serialization requirements...
    /// <inheritdoc/>
    /// </summary>
    public interface IReadableComponentStorage : XBam.IReadableComponentStorage {

      /// <summary>
      /// The components, which will be serialized with the model.
      /// </summary>
      ReadOnlyModelComponentCollection Components { 
        get;
      }
    }

    /// <summary>
    /// Readble Component Storage more specific to a model.
    /// Includes serialization requirements...
    /// <inheritdoc/>
    /// </summary>
    public interface IWriteableComponentStorage 
      : XBam.IWriteableComponentStorage, IModel.IReadableComponentStorage {}
  }

  /// <summary>
  /// This represens an object with component storage that you can write to without restrictions.
  /// Adding this to an object will give you unrestricted access to write to it's components
  /// These logics can't be replaced on objecs like models.
  /// If you want to add logic, you can instead override the virtual model function and use the base function logic.
  /// </summary>
  public interface IWriteableComponentStorage 
    : IReadableComponentStorage {}

  public static class ReadableComponentStorageExtensions {

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static IComponent GetComponent(this IReadableComponentStorage storage, string key)
      => storage.TryToGetComponent(key, out IComponent component)
        ? component
        : throw new KeyNotFoundException($"No component of type {key} found in the storage.");

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static TComponent GetComponent<TComponent>(this IReadableComponentStorage storage)
      where TComponent : IComponent<TComponent>
        => (TComponent)storage.GetComponent(Components<TComponent>.Key);

    /// <summary>
    /// Check if this has a component matching the given object.
    /// </summary>
    public static bool HasComponent(this IReadableComponentStorage storage, string componentKey)
      => storage.TryToGetComponent(componentKey, out _);

    /// <summary>
    /// Get a component if this has that given component
    /// Overriding this overrides Get component and all other Has component functionalities
    /// </summary>
    public static bool TryToGetComponent(this IReadableComponentStorage storage, string componentBaseKey, out IComponent component)
      => storage.ComponentsByBuilderKey.TryGetValue(componentBaseKey, out component);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static bool TryToGetComponent(this IReadableComponentStorage storage, System.Type componentType, out IComponent component)
      => storage.TryToGetComponent(Components.GetFactory(componentType).Key, out component);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static bool TryToGetComponent<TComponent>(this IReadableComponentStorage storage, out TComponent component)
      where TComponent : IComponent<TComponent> {
     if (storage.TryToGetComponent(Components<TComponent>.Key, out var found)) {
        component = (TComponent)found;
        return true;
      }

      component = default;
      return false;
    }

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static bool HasComponent<TComponent>(this IReadableComponentStorage storage)
      where TComponent : IComponent<TComponent>
        => storage.HasComponent(Components<TComponent>.Key);

    /// <summary>
    /// Check if this has a given component by base type
    /// </summary>
    public static bool HasComponent(this IReadableComponentStorage storage, System.Type componentType)
      => storage.TryToGetComponent(componentType, out _);

    /// <summary>
    /// Check if this has a component matching the given object
    /// </summary>
    public static bool HasComponent(this IReadableComponentStorage storage, IComponent componentModel)
      => storage.HasComponent(componentModel.Key);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public static bool TryToGetComponent(this IReadableComponentStorage storage, IComponent componentModel, out IComponent component)
      => storage.TryToGetComponent(componentModel.Key, out component);

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// This is for internal use only
    /// </summary>
    internal static void AddComponent(this IReadableComponentStorage storage, IComponent toAdd) {
      _updateComponentUniverse(storage, toAdd);

      /// add
      storage.ComponentsByBuilderKey.Add(toAdd.Key, toAdd);

      /// set parent if we need to
      if(toAdd is IModel.IComponent.IKnowMyParentModel child) {
        child.Container = (IModel.IReadableComponentStorage)storage;
      }

      /// do on added
      if (toAdd is IComponent.IDoOnAdd doer) {
        doer.ExecuteWhenAdded(storage);
      }

      /// execute waiting contracts where toAdd is b
      if (storage.ComponentsWithWaitingContracts.TryGetValue(toAdd.GetType(), out var componentsWithContractsWaitingForThisComponent)) {
        foreach(IComponent waitingComponent in componentsWithContractsWaitingForThisComponent) {
          IComponent a = storage.GetComponent(waitingComponent.Key);
          (a, toAdd) = IComponent.IHaveContract._contracts[a.GetType()][toAdd.GetType()](a, toAdd);
          storage.UpdateComponent(a);
          storage.UpdateComponent(toAdd);
        }
        storage.ComponentsWithWaitingContracts.Remove(toAdd.GetType());
      }

      /// execute contracts where toAdd is a
      /// TODO, we need an ignore internal value to pass into updateComponent to prevent recursion
      if(toAdd is IComponent.IHaveContract contractedComponent) {
        foreach((Type bType, Func<IComponent, IComponent, (IComponent a, IComponent b)> contract) in IComponent.IHaveContract._contracts[toAdd.GetType()]) {
          if(storage.TryToGetComponent(bType, out IComponent foundB)) {
            (IComponent a, IComponent b) = contract(toAdd, foundB);
            storage.UpdateComponent(a);
            storage.UpdateComponent(b);
          }
          else
            storage.ComponentsWithWaitingContracts.AddToValueCollection(bType, contractedComponent);
        }
      }
    }

    /// <summary>
    /// Update a component's universe to a new owner
    /// </summary>
    static void _updateComponentUniverse(IReadableComponentStorage storage, IComponent toAdd) {
      if(storage is IModel model) {
        IComponent._setUniverse(ref toAdd, model.Universe);
      }
      else if(storage is Archetype archetype) {
        IComponent._setUniverse(ref toAdd, archetype.Universe);
      }
    }

    /// <summary>
    /// Add a component, updating the existing value if a component of this type already exists.
    /// </summary>
    internal static void AddOrUpdateComponent(this IReadableComponentStorage storage, IComponent toSet) {
      _updateComponentUniverse(storage, toSet);
      if(!storage.ComponentsByBuilderKey.ContainsKey(toSet.Key)) {
        storage.AddComponent(toSet);
      }
      else {
        storage.ComponentsByBuilderKey[toSet.Key] = toSet;
      }
    }

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    internal static void UpdateComponent(this IReadableComponentStorage storage, IComponent toUpdate) {
      _updateComponentUniverse(storage, toUpdate);
      if(storage.HasComponent(toUpdate.Key)) {
        storage.ComponentsByBuilderKey[toUpdate.Key] = toUpdate;
      }
      else
        throw new KeyNotFoundException($"Could not find compoennt of type {toUpdate.Key} to update.");
    }

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    internal static void UpdateComponent<TComponentType>(this IReadableComponentStorage storage, Func<TComponentType, TComponentType> UpdateComponent)
      where TComponentType : IComponent {
      if(storage.TryToGetComponent(typeof(TComponentType), out IComponent current)) {
        storage.ComponentsByBuilderKey[current.Key] = UpdateComponent((TComponentType)current);
      }
      else
        throw new KeyNotFoundException($"Could not find compoennt with the key of type {typeof(TComponentType).FullName} to update.");
    }

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent<TComponent>(this IReadableComponentStorage storage)
      where TComponent : IComponent<TComponent>
        => storage.RemoveComponent(Components<TComponent>.Key);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent<TComponent>(this IReadableComponentStorage storage, out IComponent removedComponent)
      where TComponent : IComponent<TComponent>
        => storage.RemoveComponent(Components<TComponent>.Key, out removedComponent);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, System.Type toRemove)
      => storage.RemoveComponent(Components.GetFactory(toRemove).Key);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, System.Type toRemove, out IComponent removedComponent)
      => storage.RemoveComponent(Components.GetFactory(toRemove).Key, out removedComponent);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, IComponent toRemove)
      => storage.RemoveComponent(toRemove.Key);

    /// <summary>
    /// Basic remove component logic.
    /// This should be the only one you need to override for all removal logic
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, string componentKey, out IComponent removedComponent) {
      if(storage.ComponentsByBuilderKey.TryGetValue(componentKey, out removedComponent)) {
        storage.ComponentsByBuilderKey.Remove(componentKey);
        return true;
      }

      removedComponent = null;
      return false;
    }

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, string componentKey)
      => storage.RemoveComponent(componentKey, out _);

  }

  public static class WriteableComponentStorageExtensions {

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    public static void AddComponent(this IWriteableComponentStorage storage, IComponent toAdd) 
      => ReadableComponentStorageExtensions.AddComponent(storage, toAdd);

    /// <summary>
    /// Add a component, updating the existing value if a component of this type already exists.
    /// </summary>
    public static void AddOrUpdateComponent(this IWriteableComponentStorage storage, IComponent toSet)
      => ReadableComponentStorageExtensions.AddOrUpdateComponent(storage, toSet);

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    public static void UpdateComponent(this IWriteableComponentStorage storage, IComponent toUpdate) 
      => ReadableComponentStorageExtensions.UpdateComponent(storage, toUpdate);

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    public static void UpdateComponent<TComponentType>(this IWriteableComponentStorage storage, Func<TComponentType, TComponentType> UpdateComponent)
      where TComponentType : IComponent
        => ReadableComponentStorageExtensions.UpdateComponent(storage, UpdateComponent);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, IComponent component)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, component);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, System.Type componentType)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, componentType);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, System.Type componentType, out IComponent foundComponent)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, componentType, out foundComponent);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent<TComponent>(this IWriteableComponentStorage storage)
      where TComponent : IComponent<TComponent>
        => ReadableComponentStorageExtensions.RemoveComponent<TComponent>(storage);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent<TComponent>(this IWriteableComponentStorage storage, out IComponent foundComponent)
      where TComponent : IComponent<TComponent>
        => ReadableComponentStorageExtensions.RemoveComponent<TComponent>(storage, out foundComponent);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, string componentKey, out IComponent removedComponent)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, componentKey, out removedComponent);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, string componentKey)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, componentKey);

    /// <summary>
    /// Add a new component, throws if the component key is taken already
    /// </summary>
    public static TComponent AddNewComponent<TComponent>(this IWriteableComponentStorage storage, IEnumerable<(string, object)> @params)
      where TComponent : XBam.IComponent<TComponent> {
      TComponent toAdd = Components<TComponent>.Factory.Make(@params);
      if(toAdd is IModel.IComponent.IAmRestrictedToCertainTypes restrictedComponent && storage is IModel storageModel && !restrictedComponent.IsCompatableWith(storageModel)) {
        throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {storage.GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
      }

      ReadableComponentStorageExtensions.AddComponent(storage, toAdd);
      return toAdd;
    }

    /// <summary>
    /// Add a new component, throws if the component key is taken already
    /// </summary>
    public static TComponent AddNewComponent<TComponent>(this IWriteableComponentStorage storage, params (string, object)[] @params)
      where TComponent : XBam.IComponent<TComponent>
        => AddNewComponent<TComponent>(storage, @params.Cast<(string, object)>());
  }
}