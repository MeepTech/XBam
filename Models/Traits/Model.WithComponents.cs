using KellermanSoftware.CompareNetObjects;
using Meep.Tech.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  public abstract partial class Model<TModelBase>
    where TModelBase : Model<TModelBase> {

    /// <summary>
    /// A Model with Components built in
    /// </summary>
    public abstract class WithComponents
      : Model<TModelBase>,
      IModel.IReadableComponentStorage {

      /// <summary>
      /// Publicly readable components
      /// </summary>
      [ModelComponentsProperty]
      public IReadableComponentStorage.ReadOnlyModelComponentCollection Components {
        get => new(this, _components.ToDictionary(x => x.Key, y => y.Value as IModel.IComponent));
        // For deserialization:
        private set => value.Values.ForEach(AddComponent);
      }

      /// <summary>
      /// Internally stored components
      /// </summary>
      Dictionary<string, XBam.IComponent> _components
        = new();

      /// <summary>
      /// The accessor for the default Icomponents implimentation
      /// </summary>
      Dictionary<string, XBam.IComponent> IReadableComponentStorage.ComponentsByBuilderKey
        => _components;

      /// <summary>
      /// The accessor for the default Icomponents implimentation
      /// </summary>
      Dictionary<System.Type, ICollection<IComponent>> IReadableComponentStorage.ComponentsWithWaitingContracts { get; }
        = new();

      ///<summary><inheritdoc/></summary>
      public override bool Equals(object obj) {
        return base.Equals(obj)
          && IReadableComponentStorage.Equals(this, obj as IReadableComponentStorage);
      }

      ///<summary><inheritdoc/></summary>
      public override bool Equals(object obj, out ComparisonResult result) {
        return base.Equals(obj, out result)
          && IReadableComponentStorage.Equals(this, obj as IReadableComponentStorage);
      }

      #region Default Component Implimentations

      #region Read

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public TComponent GetComponent<TComponent>()
        where TComponent : IModel.IComponent<TComponent>
          => (this as IReadableComponentStorage).GetComponent<TComponent>();

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IModel.IComponent GetComponent(string componentKey)
        => ReadableComponentStorageExtensions.GetComponent(this, componentKey) as IModel.IComponent;

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IModel.IComponent GetComponent<TComponent>(string componentKey)
        where TComponent : IModel.IComponent
          => ReadableComponentStorageExtensions.GetComponent(this, componentKey) as IModel.IComponent;

      /// <summary>
      /// Get a component if this has a component of that given type
      /// </summary>
      public virtual bool TryToGetComponent(System.Type componentType, out IModel.IComponent component) {
        if (ReadableComponentStorageExtensions.TryToGetComponent(this, componentType, out XBam.IComponent found)) {
          component = found as IModel.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a given component by base type
      /// </summary>
      public virtual bool HasComponent(System.Type componentType)
        => ReadableComponentStorageExtensions.HasComponent(this, componentType);

      /// <summary>
      /// Check if this has a component matching the given object
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey)
        => ReadableComponentStorageExtensions.HasComponent(this, componentBaseKey);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool TryToGetComponent(string componentBaseKey, out IModel.IComponent component) {
        if (ReadableComponentStorageExtensions.TryToGetComponent(this, componentBaseKey, out XBam.IComponent found)) {
          component = found as IModel.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a component matching the given object
      /// </summary>
      public virtual bool HasComponent(IModel.IComponent componentModel)
        => ReadableComponentStorageExtensions.HasComponent(this, componentModel);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool TryToGetComponent(IModel.IComponent componentModel, out IModel.IComponent component) {
        if (ReadableComponentStorageExtensions.TryToGetComponent(this, componentModel, out XBam.IComponent found)) {
          component = found as IModel.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      #endregion

      #region Write

      /// <summary>
      /// Add a new component, throws if the component key is taken already
      /// </summary>
      protected virtual void AddComponent(IModel.IComponent toAdd) {
        if (toAdd is IModel.IComponent.IAmRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }

        ReadableComponentStorageExtensions.AddComponent(this, toAdd);
      }

      /// <summary>
      /// Add a new component, throws if the component key is taken already
      /// </summary>
      protected virtual void AddNewComponent<TComponent>(IEnumerable<(string, object)> @params)
        where TComponent : IModel.IComponent<TComponent> {
        IComponent toAdd = Components<TComponent>.Factory.Make(@params);
        if (toAdd is IModel.IComponent.IAmRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }

        ReadableComponentStorageExtensions.AddComponent(this, toAdd);
      }

      /// <summary>
      /// Add a new component, throws if the component key is taken already
      /// </summary>
      protected virtual void AddNewComponent<TComponent>(params (string, object)[] @params)
        where TComponent : IModel.IComponent<TComponent>
          => AddNewComponent<TComponent>(@params.Cast<(string, object)>());

      /// <summary>
      /// replace an existing component
      /// </summary>
      protected virtual void UpdateComponent(IModel.IComponent toUpdate) {
        ReadableComponentStorageExtensions.UpdateComponent(this, toUpdate);
      }

      /// <summary>
      /// update an existing component, given it's current data
      /// </summary>
      protected virtual void UpdateComponent<TComponent>(System.Func<TComponent, TComponent> UpdateComponent)
        where TComponent : IModel.IComponent {
        ReadableComponentStorageExtensions.UpdateComponent(this, UpdateComponent);
      }

      /// <summary>
      /// Add or replace a component
      /// </summary>
      protected virtual void AddOrUpdateComponent(IModel.IComponent toSet) {
        if (toSet is IModel.IComponent.IAmRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toSet.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }
        ReadableComponentStorageExtensions.AddOrUpdateComponent(this, toSet);
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(IModel.IComponent toRemove)
        => ReadableComponentStorageExtensions.RemoveComponent(this, toRemove.Key);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>()
        where TComponent : IModel.IComponent<TComponent>
          => ReadableComponentStorageExtensions.RemoveComponent<TComponent>(this);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>(out IComponent removed)
        where TComponent : IModel.IComponent<TComponent> {
        if (ReadableComponentStorageExtensions.RemoveComponent<TComponent>(this, out XBam.IComponent found)) {
          removed = found as IModel.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove)
        => ReadableComponentStorageExtensions.RemoveComponent(this, toRemove);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove, out IComponent removed) {
        if (ReadableComponentStorageExtensions.RemoveComponent(this, toRemove, out XBam.IComponent found)) {
          removed = found as IModel.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove and get an existing component
      /// </summary>
      protected virtual bool RemoveComponent(string componentKeyToRemove, out IModel.IComponent removedComponent) {
        if (ReadableComponentStorageExtensions.RemoveComponent(this, componentKeyToRemove, out XBam.IComponent component)) {
          removedComponent = component as IModel.IComponent;
          return true;
        }

        removedComponent = null;
        return false;
      }

      #endregion

      #endregion
    }
  }

  public abstract partial class Model<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase, TArchetypeBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// A Model with Components built in
    /// </summary>
    public abstract class WithComponents
      : Model<TModelBase, TArchetypeBase>,
      IModel.IReadableComponentStorage {

      /// <summary>
      /// Publicly readable components
      /// </summary>
      [ModelComponentsProperty]
      public IReadableComponentStorage.ReadOnlyModelComponentCollection Components {
        get => new(this, _components.ToDictionary(x => x.Key, y => y.Value as IModel.IComponent));
        // For deserialization:
        private set => value.Values.ForEach(AddComponent);
      }

      /// <summary>
      /// Internally stored components
      /// </summary>
      Dictionary<string, XBam.IComponent> _components
        = new();

      /// <summary>
      /// The accessor for the default Icomponents implimentation
      /// </summary>
      Dictionary<string, XBam.IComponent> IReadableComponentStorage.ComponentsByBuilderKey
        => _components;

      /// <summary>
      /// The accessor for the default Icomponents implimentation
      /// </summary>
      Dictionary<System.Type, ICollection<IComponent>> IReadableComponentStorage.ComponentsWithWaitingContracts { get; }
        = new();

      ///<summary><inheritdoc/></summary>
      public override bool Equals(object obj)
        => base.Equals(obj)
          && IReadableComponentStorage.Equals(this, obj as IReadableComponentStorage);

      ///<summary><inheritdoc/></summary>
      public override bool Equals(object obj, out ComparisonResult result)
        => base.Equals(obj, out result)
          && IReadableComponentStorage.Equals(this, obj as IReadableComponentStorage);

      #region Default Component Implimentations

      #region Read

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public TComponent GetComponent<TComponent>()
      where TComponent : IModel.IComponent<TComponent>
        => (this as IReadableComponentStorage).GetComponent<TComponent>();

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IModel.IComponent GetComponent(string componentKey)
        => ReadableComponentStorageExtensions.GetComponent(this, componentKey) as IModel.IComponent;

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IComponent GetComponent<TComponent>(string componentKey)
        where TComponent : IModel.IComponent
          => ReadableComponentStorageExtensions.GetComponent(this, componentKey) as IModel.IComponent;

      /// <summary>
      /// Get a component if this has a component of that given type
      /// </summary>
      public virtual bool TryToGetComponent(System.Type componentType, out IModel.IComponent component) {
        if (ReadableComponentStorageExtensions.TryToGetComponent(this, componentType, out XBam.IComponent found)) {
          component = found as IModel.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Get a component if this has a component of that given type
      /// </summary>
      public virtual bool TryToGetComponent<TComponent>(out TComponent component)
        where TComponent : IComponent<TComponent> {
        if (ReadableComponentStorageExtensions.TryToGetComponent<TComponent>(this, out var found)) {
          component = found;
          return true;
        }

        component = default;
        return false;
      }

      /// <summary>
      /// Check if this has a given component by base type
      /// </summary>
      public virtual bool HasComponent<TComponent>()
        where TComponent : IComponent
          => ReadableComponentStorageExtensions.HasComponent(this, typeof(TComponent));

      /// <summary>
      /// Check if this has a given component by base type
      /// </summary>
      public virtual bool HasComponent(System.Type componentType)
        => ReadableComponentStorageExtensions.HasComponent(this, componentType);

      /// <summary>
      /// Check if this has a component matching the given object
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey)
        => ReadableComponentStorageExtensions.HasComponent(this, componentBaseKey);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool TryToGetComponent(string componentBaseKey, out IModel.IComponent component) {
        if (ReadableComponentStorageExtensions.TryToGetComponent(this, componentBaseKey, out XBam.IComponent found)) {
          component = found as IModel.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a component matching the given object
      /// </summary>
      public virtual bool HasComponent(IModel.IComponent componentModel)
        => ReadableComponentStorageExtensions.HasComponent(this, componentModel);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool TryToGetComponent(IModel.IComponent componentModel, out IModel.IComponent component) {
        if (ReadableComponentStorageExtensions.TryToGetComponent(this, componentModel, out XBam.IComponent found)) {
          component = found as IModel.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      #endregion

      #region Write

      /// <summary>
      /// Add a new component, throws if the component key is taken already
      /// </summary>
      protected virtual void AddComponent(IModel.IComponent toAdd) {
        if (toAdd is IModel.IComponent.IAmRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }

        ReadableComponentStorageExtensions.AddComponent(this, toAdd);
      }

      /// <summary>
      /// Add a new component, throws if the component key is taken already
      /// </summary>
      protected virtual void AddNewComponent<TComponent>(IEnumerable<(string, object)> @params)
        where TComponent : IModel.IComponent<TComponent> {
        IComponent toAdd = Components<TComponent>.Factory.Make(@params);
        if (toAdd is IModel.IComponent.IAmRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }

        ReadableComponentStorageExtensions.AddComponent(this, toAdd);
      }

      /// <summary>
      /// Add a new component, throws if the component key is taken already
      /// </summary>
      protected virtual void AddNewComponent<TComponent>(params (string, object)[] @params)
        where TComponent : IModel.IComponent<TComponent>
          => AddNewComponent<TComponent>(@params.Cast<(string, object)>());

      /// <summary>
      /// replace an existing component
      /// </summary>
      protected virtual void UpdateComponent(IModel.IComponent toUpdate) {
        ReadableComponentStorageExtensions.UpdateComponent(this, toUpdate);
      }

      /// <summary>
      /// update an existing component, given it's current data
      /// </summary>
      protected virtual void UpdateComponent<TComponent>(System.Func<TComponent, TComponent> UpdateComponent)
        where TComponent : IModel.IComponent {
        ReadableComponentStorageExtensions.UpdateComponent(this, UpdateComponent);
      }

      /// <summary>
      /// Add or replace a component
      /// </summary>
      protected virtual void AddOrUpdateComponent(IModel.IComponent toSet) {
        if (toSet is IModel.IComponent.IAmRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toSet.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }
        ReadableComponentStorageExtensions.AddOrUpdateComponent(this, toSet);
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(IModel.IComponent toRemove)
        => ReadableComponentStorageExtensions.RemoveComponent(this, toRemove.Key);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>()
        where TComponent : IModel.IComponent<TComponent>
          => ReadableComponentStorageExtensions.RemoveComponent<TComponent>(this);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>(out IComponent removed)
        where TComponent : IModel.IComponent<TComponent> {
        if (ReadableComponentStorageExtensions.RemoveComponent<TComponent>(this, out XBam.IComponent found)) {
          removed = found as IModel.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove)
        => ReadableComponentStorageExtensions.RemoveComponent(this, toRemove);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove, out IComponent removed) {
        if (ReadableComponentStorageExtensions.RemoveComponent(this, toRemove, out XBam.IComponent found)) {
          removed = found as IModel.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove and get an existing component
      /// </summary>
      protected virtual bool RemoveComponent(string componentKeyToRemove, out IModel.IComponent removedComponent) {
        if (ReadableComponentStorageExtensions.RemoveComponent(this, componentKeyToRemove, out XBam.IComponent component)) {
          removedComponent = component as IModel.IComponent;
          return true;
        }

        removedComponent = null;
        return false;
      }

      #endregion

      #endregion
    }
  }
}