using Meep.Tech.Reflection;
using Meep.Tech.XBam.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  public partial interface IComponent {

    /// <summary>
    /// A builder for components.
    /// </summary>
    public new interface IBuilder : XBam.IBuilder {

      /// <summary>
      /// The parent model.
      /// </summary>
      IModel? Parent {
        get;
        internal set;
      }
      
      /// <summary>
      /// internal setter for parameters.
      /// </summary>
      internal protected Dictionary<string, object>? __parameters
        { set; }

      /// <summary>
      /// Used to initialize the component with or without a builder.
      /// </summary>
      internal protected static void InitalizeComponent(ref XBam.IComponent? component, IBuilder? builder, XBam.IReadableComponentStorage? ontoParent = null, Archetype? archetype = null, Universe? universe = null) {
        universe = builder?.Universe ?? universe ?? throw new ArgumentNullException();
        archetype = builder?.Archetype ?? archetype ?? throw new ArgumentNullException();

        component ??= (XBam.IComponent?)((XBam.IFactory)(builder?.Archetype ?? archetype))._modelConstructor(builder 
          ?? throw new ArgumentNullException(nameof(builder)));

        if (component is null) {
          if (builder is not null) {
            component = (XBam.IComponent?)archetype.OnModelInitialized(builder, component);

            if (component is not null) {
              component = (XBam.IComponent)component.OnFinalized(builder);
            }

            component = (XBam.IComponent?)archetype.OnModelFinalized(builder, component);
          }
        }
        else {
          component = (XBam.IComponent)component.OnInitialized((builder?.Archetype ?? archetype), (builder?.Universe ?? universe), builder);

          if (component is IUnique unique) {
            component = (XBam.IComponent)unique.InitializeId(builder?.TryToGet<string>(nameof(IUnique.Id)));
          }

          if (component is IModel.IComponent.IKnowMyParentModel child) {
            child.Container = (IReadableComponentStorage?)(ontoParent ?? ((IReadableComponentStorage?)builder?.Parent))!;
          }

          if (builder is not null) {
            component = (XBam.IComponent)(builder?.Archetype ?? archetype).OnModelInitialized(builder!, component)!;
          }

          component = (XBam.IComponent)component.OnFinalized(builder);

          if (builder is not null) {
            component = (XBam.IComponent)(builder?.Archetype ?? archetype).OnModelFinalized(builder!, component)!;
          }
        }
      }
    }
  }

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent
    where TComponentBase : XBam.IComponent<TComponentBase> {

    /// <summary>
    /// Default builder class for components. Pretty much the same as the model based one.
    /// </summary>
    public new partial struct Builder : XBam.IBuilder<TComponentBase>, IBuilder {
      Dictionary<string, object> _parameters
        => __parameters ??= new();
      internal Dictionary<string, object>? __parameters;

      /// <summary>
      /// The archetype/factory using this builder.
      /// </summary>
      public XBam.Archetype Archetype {
        get;
      }

      /// <summary>
      /// The universe this builder is building in
      /// </summary>
      public Universe Universe {
        get;
      }

      /// <summary>
      /// The parent model of this component, if there is one.
      /// </summary>
      public IModel? Parent {
        get;
        internal set;
      }

      IModel? IBuilder.Parent { 
        get => Parent;
        set => Parent = value;
      }

      ///<summary><inheritdoc/></summary>
      public IEnumerable<KeyValuePair<string, object>> Parameters
        => this;

      ///<summary><inheritdoc/></summary>
      public IEnumerable<string> Keys
        => ((IReadOnlyDictionary<string, object>)_parameters).Keys;

      ///<summary><inheritdoc/></summary>
      public IEnumerable<object> Values
        => ((IReadOnlyDictionary<string, object>)_parameters).Values;

      ///<summary><inheritdoc/></summary>
      public int Count
        => ((IReadOnlyCollection<KeyValuePair<string, object>>)_parameters).Count;

      Dictionary<string, object> IBuilder.__parameters 
        { set => __parameters = value; }

      /// <summary>
      /// get the param
      /// </summary>
      public object this[Param param]
        => this[param.Key].CastTo(param.ValueType);

      /// <summary>
      /// get the param
      /// </summary>
      public object this[string param]
        => this[param];

      /// <summary>
      /// Empty new builder
      /// </summary>
      public Builder(XBam.Archetype type, IModel? parent = null, Universe? universe = null) {
        Archetype = type;
        Universe = universe ?? type.Universe;
        Parent = parent;
        __parameters = null;
      }

      /// <summary>
      /// New builder from a collection of param names
      /// </summary>
      public Builder(XBam.Archetype type, IEnumerable<KeyValuePair<string, object>>? @params, IModel? parent = null, Universe? universe = null) {
        Archetype = type;
        Universe = universe ?? Archetype.Universe;
        Parent = parent;
        __parameters = @params is not null 
          ? new(@params) 
          : null;
      }

      ///<summary><inheritdoc/></summary>
      public bool ContainsKey(string key) {
        return ((IReadOnlyDictionary<string, object>)_parameters).ContainsKey(key);
      }

      ///<summary><inheritdoc/></summary>
      public bool TryGetValue(string key, out object value) {
        return ((IReadOnlyDictionary<string, object>)_parameters).TryGetValue(key, out value);
      }

      ///<summary><inheritdoc/></summary>
      public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
        return ((IEnumerable<KeyValuePair<string, object>>)_parameters).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)_parameters).GetEnumerator();
      }

      ///<summary><inheritdoc/></summary>
      public IBuilder<TComponentBase> Add(KeyValuePair<string, object> parameter)
        => (IBuilder<TComponentBase>)(this as IBuilder).Add(parameter);

      ///<summary><inheritdoc/></summary>
      public IBuilder<TComponentBase> Add(string key, object value) {
        _parameters.Add(key, value);
        return this;
      }

      ///<summary><inheritdoc/></summary>
      public object Get(string key)
        => _parameters[key];

      ///<summary><inheritdoc/></summary>
      public bool TryToGet(string key, out object value)
        => _parameters.TryGetValue(key, out value);

      ///<summary><inheritdoc/></summary>
      public bool Has(string key)
        => _parameters.ContainsKey(key);

      /// <summary>
      /// Check if the builder has this key
      /// </summary>
      public bool Has(Param param)
        => _parameters.ContainsKey(param.Key);

      ///<summary><inheritdoc/></summary>
      public TComponentBase Make() {
        XBam.IComponent producedComponent = default!;
        IBuilder.InitalizeComponent(ref producedComponent!, this);

        // loging
        producedComponent.TryToLog(
          ModelLog.Entry.ActionType.Built,
          "null",
          this,
          new Dictionary<string, object>() {
            { ModelLog.Entry.MetadataField.AutoBuilderUsed.Key, Archetype.Universe.TryToGetExtraContext<Configuration.IModelAutoBuilder>()?.HasAutoBuilderSteps(Archetype) ?? false }
          }
        );

        return (TComponentBase)producedComponent;
      }
    }
  }
}