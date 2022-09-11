using Meep.Tech.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using static Meep.Tech.XBam.Archetype.ISplayed;

namespace Meep.Tech.XBam {

  /// <summary>
  /// A singleton data store and factory.
  /// </summary>
  public abstract partial class Archetype : IArchetype, IResource, IEquatable<Archetype>, IBuilderSource {
    Func<IBuilder, IModel?> IFactory._modelConstructor 
      {get; set; } = null!;
    Type _modelTypeProduced
      = null!;
    internal HashSet<IComponent.ILinkedComponent> _modelLinkedComponents
      = new(); 
    internal Dictionary<string, object>? _defaultTestParams 
      = null;
    readonly HashSet<ITag> _tags 
      = new();
    internal Dictionary<string, Func<XBam.IComponent.IBuilder, IModel.IComponent>> _initialUnlinkedModelComponents
      = new();
    internal Dictionary<string, IComponent> _initialComponents 
      = new();

    #region Archetype Data Members

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public Identity Id {
      get;
    }

    /// <summary>
    /// The universe this archetype is part of.
    /// </summary>
    public Universe Universe { 
      get; 
    }

    ///<summary><inheritdoc/></summary>
    public abstract Type BaseArchetype {
      get;
    }

    ///<summary><inheritdoc/></summary>
    public abstract Type ModelBaseType {
      get;
      internal set;
    }

    ///<summary><inheritdoc/></summary>
    public Type ModelTypeProduced {
      get => _modelTypeProduced ??= ModelBaseType;
      internal set => _modelTypeProduced = value;
    }

    ///<summary><inheritdoc/></summary>
    public Type Type {
      get;
    }

    ///<summary><inheritdoc/></summary>
    public bool IsBaseArchetype
      => BaseArchetype?.Equals(null) ?? true;

    ///<summary><inheritdoc/></summary>
    public Collection Types {
      get;
      internal set;
    } = null!;

    ///<summary><inheritdoc/></summary>
    public virtual IEnumerable<ITag> Tags
      => _tags;

    #endregion

    #region Archetype Configuration Settings

    /// <summary>
    /// The initial default components to add to this archetype on it's creation, indexed by their keys. 
    /// </summary>
    protected internal virtual IReadOnlyDictionary<string, IComponent> InitialComponents
      => _initialComponents;

    /// <summary>
    /// The Archetype components linked to model components
    /// </summary>
    protected internal IEnumerable<IComponent> ModelLinkedComponents
      => _modelLinkedComponents;

    /// <summary>
    /// Components by key, with optional constructors used to set up the default components on a model made by this Archetype,
    /// Usually you'll want to use an Archetype.ILinkedComponent but this is here too for model components. not linked to an archetype component.
    /// If the constructor function is left null, the default component ctor is used.
    /// </summary>
    protected internal virtual IReadOnlyDictionary<string, Func<XBam.IComponent.IBuilder, IModel.IComponent>> InitialUnlinkedModelComponents 
      => _initialUnlinkedModelComponents;

    /// <summary>
    /// If this is true, this Archetype can have it's component collection modified before load by mods and other libraries.
    /// This does not affect the ability to inherit and override InitialComponents for an archetype.
    /// </summary>
    protected internal virtual bool AllowExternalComponentConfiguration
      => true;

    /// <summary>
    /// If this is true, this archetype and children of it can be initialized after the loader has finished.
    /// Be careful with these, it's up to you to maintain singleton patters.
    /// </summary>
    protected internal virtual bool AllowInitializationsAfterLoaderFinalization
      => false;

    /// <summary>
    /// If this is true, this archetype and children of it can be deinitialized after the loader has finished.
    /// Be careful with these, it's up to you to maintain singleton patters.
    /// </summary>
    protected internal virtual bool AllowDeInitializationsAfterLoaderFinalization
      => false;

    /// <summary>
    /// Default params for testing
    /// </summary>
    internal protected virtual Dictionary<string, object>? DefaultTestParams {
      get => _defaultTestParams;
      init => _defaultTestParams = value;
    } 

    /// <summary>
    /// Finish setting this up
    /// </summary>
    protected internal virtual void Finish() {}

    /// <summary>
    /// Try to unload this archetype
    /// </summary>
    protected internal abstract bool TryToUnload();

    #endregion

    #region Initialization

    /// <summary>
    /// Make a new archetype
    /// </summary>
    internal Archetype(Identity id, Universe? universe, Func<Archetype, Identity> getDefaultId) {
      Universe ??= universe ?? Universe.Default;
      Id = id ?? getDefaultId(this);

      if(Id is null) {
        throw new ArgumentException($"Id is null. The passed in ID May not be of the expected type. Expected:{typeof(Identity).FullName}, provided: {id.GetType().FullName}.");
      }

      Type = GetType();
    }

    /// <summary>
    /// helper for getting the builder constructor from the non-generic base class
    /// </summary>
    /// <returns></returns>
    internal protected abstract Func<Archetype, IEnumerable<KeyValuePair<string, object>>?, IBuilder> GetGenericBuilderConstructor();

    #endregion

    #region Hash and Equality

    ///<summary><inheritdoc/></summary>
    public override int GetHashCode() 
      => Id.GetHashCode();

    ///<summary><inheritdoc/></summary>
    public override bool Equals(object? obj) 
      => (obj as Archetype)?.Equals(this) ?? false;

    ///<summary><inheritdoc/></summary>
    public override string ToString() {
      return $"+{Id}+";
    }

    ///<summary><inheritdoc/></summary>
    public bool Equals(Archetype other) 
      => Id.Key == other?.Id.Key;

    ///<summary><inheritdoc/></summary>
    public static bool operator ==(Archetype a, Archetype b)
      => a?.Equals(b) ?? (b is null);

    ///<summary><inheritdoc/></summary>
    public static bool operator !=(Archetype a, Archetype b)
      => !(a == b);

    #endregion

    #region Configuration Helper Functions

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// This is called right after the model is initialized.
    /// </summary>
    internal virtual IModel? OnModelInitialized(IBuilder builder, IModel? model)
      => model;

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// This is called after all logic except finalization of model components.
    /// </summary>
    internal virtual IModel? OnModelFinalized(IBuilder builder, IModel? model)
      => model;

    #endregion

    #region Make/Model Construction

    IBuilder IBuilderSource.Build(IEnumerable<KeyValuePair<string, object>> initialParams)
      => throw new NotImplementedException();

    /// <summary>
    /// Simple helper to get the Make function internally
    /// </summary>
    protected internal abstract IModel Make(IBuilder? builder = null);

    /// <summary>
    /// Overrideable Model Construction logic.
    /// </summary>
    protected internal abstract Func<IBuilder, IModel?> ModelConstructor { get; internal set; }

    #endregion

    #region Default Component Implimentations

    /// <summary>
    /// Publicly readable components
    /// </summary>
    public IReadOnlyDictionary<string, IComponent> Components
      => _components.ToDictionary(x => x.Key, y => (IComponent)y.Value);

    /// <summary>
    /// The accessor for the default Icomponents implimentation
    /// </summary>
    Dictionary<string, XBam.IComponent> IReadableComponentStorage.ComponentsByBuilderKey
      => _components;
    
    /// <summary>
    /// The accessor for the default Icomponents implimentation
    /// </summary>
    Dictionary<Type, ICollection<XBam.IComponent>> IReadableComponentStorage.ComponentsWithWaitingContracts { get; }
      = new();

    /// <summary>
    /// Internally stored components
    /// </summary>
    Dictionary<string, XBam.IComponent> _components {
      get;
    } = new Dictionary<string, XBam.IComponent>();

    #region Read

    /// <summary>
    /// Get a component if it exists. Throws if it doesn't
    /// </summary>
    public IComponent GetComponent(string componentKey)
      => (IComponent)(this as IReadableComponentStorage).GetComponent(componentKey);

    /// <summary>
    /// Get a component if it exists. Throws if it doesn't
    /// </summary>
    public IComponent GetComponent<TComponent>(string componentKey)
      where TComponent : IComponent
        => (IComponent)(this as IReadableComponentStorage).GetComponent(componentKey);

    /// <summary>
    /// Get a component if it exists. Throws if it doesn't
    /// </summary>
    public TComponent GetComponent<TComponent>()
      where TComponent : IComponent<TComponent>
        => (this as IReadableComponentStorage).GetComponent<TComponent>();

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public bool TryToGetComponent(Type componentType, out IComponent? component) {
      if(this.TryToGetComponent(componentType, out XBam.IComponent found)) {
        component = (IComponent)found;
        return true;
      }

      component = null;
      return false;
    }

    /// <summary>
    /// Check if this has a given component by base type
    /// </summary>
    public bool HasComponent(Type componentType)
      => (this as IReadableComponentStorage).HasComponent(componentType);

    /// <summary>
    /// Check if this has a component matching the given object
    /// </summary>
    public bool HasComponent(string componentBaseKey)
      => (this as IReadableComponentStorage).HasComponent(componentBaseKey);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public bool TryToGetComponent(string componentBaseKey, out IComponent? component) {
      if(this.TryToGetComponent(componentBaseKey, out XBam.IComponent found)) {
        component = (IComponent)found;
        return true;
      }

      component = null;
      return false;
    }

    /// <summary>
    /// Check if this has a component matching the given object
    /// </summary>
    public bool HasComponent(IComponent componentModel)
      => (this as IReadableComponentStorage).HasComponent(componentModel);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public bool TryToGetComponent(IComponent componentModel, out IComponent? component) {
      if((this as IReadableComponentStorage).TryToGetComponent(componentModel, out XBam.IComponent found)) {
        component = found as IComponent;
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
    protected void AddComponent(IComponent toAdd) {
      if(toAdd is IComponent.IAmRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
        throw new ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
      }

      (this as IReadableComponentStorage).AddComponent(toAdd);
    }

    /// <summary>
    /// replace an existing component
    /// </summary>
    protected void UpdateComponent(IComponent toUpdate) {
      (this as IReadableComponentStorage).UpdateComponent(toUpdate);
    }

    /// <summary>
    /// update an existing component, given it's current data
    /// </summary>
    protected void UpdateComponent<TComponent>(Func<TComponent, TComponent> UpdateComponent)
      where TComponent : IComponent {
      (this as IReadableComponentStorage).UpdateComponent(UpdateComponent);
    }

    /// <summary>
    /// Add or replace a component
    /// </summary>
    protected void AddOrUpdateComponent(IComponent toSet) {
      if(toSet is IComponent.IAmRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
        throw new ArgumentException($"Component of type {toSet.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
      }
      (this as IReadableComponentStorage).AddOrUpdateComponent(toSet);
    }

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent(IComponent toRemove)
      => this.RemoveComponent(toRemove.Key);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent<TComponent>()
      where TComponent : IComponent<TComponent>
        => (this as IReadableComponentStorage).RemoveComponent<TComponent>();

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent<TComponent>(out IComponent? removed)
      where TComponent : IComponent<TComponent> {
      if(this.RemoveComponent<TComponent>(out XBam.IComponent found)) {
        removed = found as IComponent;
        return true;
      }

      removed = null;
      return false;
    }

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent(Type toRemove)
      => (this as IReadableComponentStorage).RemoveComponent(toRemove);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent(Type toRemove, out IComponent? removed) {
      if(this.RemoveComponent(toRemove, out XBam.IComponent found)) {
        removed = found as IComponent;
        return true;
      }

      removed = null;
      return false;
    }

    /// <summary>
    /// Remove and get an existing component
    /// </summary>
    protected bool RemoveComponent(string componentKeyToRemove, out IComponent? removedComponent) {
      if(this.RemoveComponent(componentKeyToRemove, out XBam.IComponent component)) {
        removedComponent = component as IComponent;
        return true;
      }

      removedComponent = null;
      return false;
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// An Id unique to each Archetype.
  /// Can be used as a static key.
  /// </summary>
  public abstract partial class Archetype<TModelBase, TArchetypeBase> 
    : Archetype, IFactory, IBuilderSource
    where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> 
  {

    #region Archetype Data Members

    /// <summary>
    /// The collection containing this archetype
    /// </summary>
    public new Collection Types 
      => (Collection)base.Types;

    /// <summary>
    /// The base archetype that all of the ones like it are based on.
    /// </summary>
    public sealed override Type BaseArchetype
      => typeof(TArchetypeBase);

    /// <summary>
    /// The most basic model that this archetype can produce.d
    /// This is used to generat the default model constructor.
    /// </summary>
    public sealed override Type ModelBaseType {
      get => _ModelBaseType;
      internal set => _ModelBaseType = value;
    } Type _ModelBaseType
      = typeof(TModelBase);

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public new Identity Id
      => (Identity)base.Id;

    /// <summary>
    /// Just used to get Id in case the Id namespace is overriden.
    /// </summary>
    public Identity GetId()
      => Id;

    #endregion

    #region Archetype Initialization

    /// <summary>
    /// Used to get a default Id for this type of archetype.
    /// </summary>
    protected static Archetype.Identity GetDefaultIdentityKey(Archetype @this) {
      var typeToName = @this.GetType();
      var key = typeToName.Name;
      if (key == "Type") {
        typeToName = typeToName.DeclaringType;
        key = typeToName.Name;
      }

      return new Identity(key);
    }

    /// <summary>
    /// The base for making a new archetype.
    /// </summary>
    internal Archetype(Archetype.Identity id, Universe? universe, Func<Universe, Collection>? getCollection)
      : base(id, universe, GetDefaultIdentityKey) {
      if (Universe.Loader.IsFinished && !AllowInitializationsAfterLoaderFinalization) {
        throw new InvalidOperationException($"Tried to initialize archetype of type {id} while the loader was sealed");
      }

      var collection = getCollection?.Invoke(Universe) 
        ?? (Collection)
          // if the base of this is registered somewhere, get the registered one by default
          (Universe.Archetypes.TryToGetCollection(GetType(), out var found)
            ? found is Collection
              ? found
              : Universe.Archetypes._collectionsByRootArchetype[typeof(TArchetypeBase).FullName]
                = new Collection()
            // else this is the base and we need a new one
            : Universe.Archetypes._collectionsByRootArchetype[typeof(TArchetypeBase).FullName]
              = new Collection());

      collection.Universe.Archetypes._registerArchetype(this, collection);
      _initialize();
    }

    /// <summary>
    /// The base for making a new archetype.
    /// </summary>
    protected Archetype(Archetype.Identity id, Universe? universe = null) 
      : this(id, universe, null) 
    {}

    /// <summary>
    /// Initialize this Archetype internally
    /// </summary>
    void _initialize() {
      _initializeInitialComponents();
    }

    /// <summary>
    /// Add all initial components
    /// </summary>
    void _initializeInitialComponents() {
      foreach(IComponent component in InitialComponents.Values) {
        AddComponent(component);
        if(component is IComponent.ILinkedComponent linkedComponent) {
          _modelLinkedComponents.Add(linkedComponent);
        }
      }
    }

    /// <summary>
    /// Deinitialize this Archetype internally
    /// </summary>
    void _deInitialize() {
      ModelInitializer = null;
      _deInitializeInitialComponents();
    }

    /// <summary>
    /// remove all components
    /// </summary>
    void _deInitializeInitialComponents() {
      foreach(IComponent component in InitialComponents.Values) {
        RemoveComponent(component);
        if(component is IComponent.ILinkedComponent linkedComponent) {
          _modelLinkedComponents.Remove(linkedComponent);
        }
      }
    }

    #endregion

    #region Model Construction 

    #region Model Constructor Settings

    /// <summary>
    /// Overrideable Model Construction logic.
    /// </summary>
    protected internal override Func<IBuilder, IModel?> ModelConstructor {
      get;
      internal set;
    } = null!;

    /// <summary>
    /// Model Constructon and Initialization logic.
    /// </summary>
    protected virtual Func<IBuilder<TModelBase>, TModelBase?> ModelInitializer {
      get => b => (TModelBase?)ModelConstructor?.Invoke(b);
      set {
        if (Universe.Loader.IsFinished && !AllowInitializationsAfterLoaderFinalization) {
          throw new InvalidOperationException($"Tried to set constructor for Archetype:{Id.Key} while the loader was sealed");
        }

        Universe.ExtraContexts.OnLoaderArchetypeModelConstructorSetStart!(this);
        ModelConstructor = value is null ? null! : b => value.Invoke((IBuilder<TModelBase>)b);
        Universe.ExtraContexts.OnLoaderArchetypeModelConstructorSetComplete!(this);
      }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    Func<IBuilder, IModel?> IFactory._modelConstructor {
      get => ModelConstructor is null
        ? null!
        : builder => ModelInitializer((IBuilder<TModelBase>)builder);
      set {
        if (value is null) {
          ModelInitializer = null!;
        }
        else {
          ModelInitializer = b => (TModelBase?)value.Invoke(b);
        }
      }
    }

    #endregion

    #region Build/Make

    #region Builder Setup 
    IBuilder IBuilderSource.Build(IEnumerable<KeyValuePair<string, object>>? initialParams)
      => Build();
    internal Func<Archetype, IEnumerable<KeyValuePair<string, object>>?, Universe, IBuilder<TModelBase>> _defaultBuilderCtor
      = null!;

    /// <summary>
    /// Start a model builder
    /// </summary>
    internal protected virtual IBuilder<TModelBase> Build(IEnumerable<KeyValuePair<string, object>>? initialParams = null)
      => (IBuilder<TModelBase>)GetGenericBuilderConstructor()(this, initialParams);

    /// <summary>
    /// The default way a new builder is created.
    /// The dictionary passed in has the potential to be null
    /// </summary>
    internal protected virtual Func<Archetype, IEnumerable<KeyValuePair<string, object>>?, Universe, IBuilder<TModelBase>> BuilderConstructor {
      get => _defaultBuilderCtor ??= (archetype, @params, universe) 
        => @params is not null 
          ? new IModel<TModelBase>.Builder(archetype, @params, universe)
          : new IModel<TModelBase>.Builder(archetype, universe); 
      set => _defaultBuilderCtor = value;
    }

    /// <summary>
    /// helper for getting the builder constructor from the non-generic base class
    /// </summary>
    protected internal override Func<Archetype, IEnumerable<KeyValuePair<string, object>>?, IBuilder> GetGenericBuilderConstructor()
      => (archetype, @params) => BuilderConstructor(archetype, @params, Universe);

    #region Configuration Helper Functions

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    protected internal virtual TModelBase? OnModelInitialized(IBuilder<TModelBase> builder, TModelBase? model)
      => model;

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    protected internal virtual TModelBase? OnModelFinalized(IBuilder<TModelBase> builder, TModelBase? model)
      => model;

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    internal override IModel? OnModelInitialized(IBuilder builder, IModel? model)
      => OnModelInitialized((IBuilder<TModelBase>)builder, (TModelBase?)model);

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    internal override IModel? OnModelFinalized(IBuilder builder, IModel? model)
      => OnModelFinalized((IBuilder<TModelBase>)builder, (TModelBase?)model);

    #endregion

    #endregion

    #region List Based

    /// <summary>
    /// Make a new model via builder with the given params.
    /// </summary>
    protected internal virtual TModelBase Make(IEnumerable<KeyValuePair<string, object>>? @params)
      => BuildModel(Build(@params));

    /// <summary>
    /// Make a new model via builder with the given params.
    /// </summary>
    protected internal virtual TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<string, object>> @params)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(Build(@params));

    /// <summary>
    /// Make a new model via builder with the given params.
    /// </summary>
    protected internal TModelBase Make(IEnumerable<(string key, object value)>? @params)
      => Make(@params?.Select(entry => new KeyValuePair<string,object>(entry.key, entry.value)));

    /// <summary>
    /// Make a new model via builder with the given params.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(IEnumerable<(string key, object value)> @params)
      where TDesiredModel : TModelBase
      => (TDesiredModel)Make(@params?.Select(entry => new KeyValuePair<string,object>(entry.key, entry.value)));

    /// <summary>
    /// Make a new model via builder with the given params.
    /// </summary>
    protected internal TModelBase Make(params (string key, object value)[] @params)
      => Make((IEnumerable<(string key, object value)>)@params);

    /// <summary>
    /// Make a new model via builder with the given params.
    /// </summary>
    /// <returns></returns>
    protected internal TDesiredModel Make<TDesiredModel>(params (string key, object value)[] @params)
      where TDesiredModel : TModelBase
        => (TDesiredModel)Make((IEnumerable<(string key, object value)>)@params);

    #endregion

    #region Builder Based

    /// <summary>
    /// Build the model with the builder.
    /// </summary>
    protected internal virtual TModelBase BuildModel(IBuilder? builder = null) {
      builder ??= Build();
      var model = builder.Make();

      return (TModelBase)model;
    }

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>()
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(null);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    protected internal override IModel Make(IBuilder? builder = null)
      => BuildModel(builder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(IBuilder builder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(builder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    protected internal TModelBase Make(IBuilder<TModelBase> builder)
      => BuildModel(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(IBuilder<TModelBase> builder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(IModel<TModelBase>.Builder builder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(builder);

    /// <summary>
    /// Make a model that requires a struct based builder:
    /// </summary>
    protected internal TModelBase Make(Func<IBuilder, IBuilder> configureBuilder)
      => Make(builder => (IBuilder<TModelBase>)configureBuilder(Build()));

    /// <summary>
    /// Make a model that requires a struct based builder:
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(Func<IBuilder, IBuilder> configureBuilder)
      where TDesiredModel : TModelBase
        => Make<TDesiredModel>(builder => (IBuilder<TModelBase>)configureBuilder(Build()));

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    protected internal TModelBase Make(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      => BuildModel(configureBuilder(Build()));

    /// <summary>
    /// Make a model that requires a struct based builder"
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(configureBuilder(Build()));

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    protected internal TModelBase Make(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      => BuildModel(configureBuilder((IModel<TModelBase>.Builder)Build()));

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(configureBuilder((IModel<TModelBase>.Builder)Build()));

    #endregion

    #endregion

    #endregion

    #region Archetype DeInitialization

    /// <summary>
    /// Called on unload before the type is actually un-registered from the universe.
    /// the base version of this calls OnUnload for all extra contexts, if there are any.
    /// </summary>
    protected virtual void OnUnloadFrom(Universe universe)
      => universe.ExtraContexts.OnUnloadArchetype!(this);

    /// <summary>
    /// Attempts to unload this archetype from the universe and collections it's registered to
    /// </summary>
    protected internal sealed override bool TryToUnload() {
      if (!Universe.Loader.IsFinished || AllowDeInitializationsAfterLoaderFinalization) {
        Universe universe = Universe;
        OnUnloadFrom(universe);
        universe.Archetypes._unRegisterArchetype(this);
        Id._deRegister(universe);
        _deInitialize();

        return true;
      }
      else return false;
    }

    #endregion
  }
}
