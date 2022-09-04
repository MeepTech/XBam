using Meep.Tech.Reflection;
using Meep.Tech.XBam.Configuration;
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
      IModel Parent {
        get;
        internal set;
      }
      
      /// <summary>
      /// internal setter for parameters.
      /// </summary>
      internal protected Dictionary<string, object> __parameters
        { set; }

      internal protected static void InitalizeComponent(ref IComponent component, IBuilder @this, Archetype archetype = null, Universe universe = null) {
        universe = @this?.Universe ?? universe;
        archetype = @this?.Archetype ?? archetype;

        component ??= (IComponent)((XBam.IFactory)(@this?.Archetype ?? archetype))._modelConstructor(@this);
        component = (IComponent)component.OnInitialized((@this?.Archetype ?? archetype), (@this?.Universe ?? universe), @this);

        if (component is IUnique unique) {
          component = (IComponent)unique.InitializeId(@this.TryToGet<string>(nameof(IUnique.Id)));
        }

        if (component is IComponent.IKnowMyParentModel child) {
          child.Container = (IReadableComponentStorage)@this.Parent;
        }

        component = (IComponent)(@this?.Archetype ?? archetype).ConfigureModel(@this, component);

        component = (IComponent)component.OnFinalized(@this);
        component = (IComponent)(@this?.Archetype ?? archetype).FinalizeModel(@this, component);
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
    public new partial struct Builder : XBam.IBuilder<TComponentBase>, IComponent.IBuilder {
      Dictionary<string, object> _parameters
        => __parameters ??= new();
      internal Dictionary<string, object> __parameters;

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
      public IModel Parent {
        get;
        internal set;
      }

      IModel IBuilder.Parent { 
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
      public Builder(XBam.Archetype type, IModel parent = null, Universe universe = null) {
        Archetype = type;
        Universe = universe ?? type.Id.Universe;
        Parent = parent;
        __parameters = null;
      }

      /// <summary>
      /// New builder from a collection of param names
      /// </summary>
      public Builder(XBam.Archetype type, IEnumerable<KeyValuePair<string, object>> @params, IModel parent = null, Universe universe = null) {
        Archetype = type;
        Universe = universe ?? Archetype.Id.Universe;
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
        IComponent producedComponent = default;
        IBuilder.InitalizeComponent(ref producedComponent, this);

        // loging
        producedComponent.TryToLog(
          Configuration.ModelLog.Entry.ActionType.Built,
          "null",
          this,
          new Dictionary<string, object>() {
            { ModelLog.Entry.MetadataField.AutoBuilderUsed.Key, Archetype._modelAutoBuilderSteps?.Any() ?? false }
          }
        );

        return (TComponentBase)producedComponent;
      }
    }
  }
}