using Meep.Tech.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  public partial interface IModel {

    /// <summary>
    /// A modifyable parameter container that is used to build a model.
    /// The non-generic base class for utility.
    /// </summary>
    public partial interface IBuilder: XBam.IBuilder {}
  }

  public partial interface IModel<TModelBase> 
    where TModelBase : IModel<TModelBase> 
  {

    /// <summary>
    /// A modifyable parameter container that is used to build a model.
    /// </summary>
    public partial struct Builder : IModel.IBuilder, IBuilder<TModelBase> {
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

      ///<summary><inheritdoc/></summary>
      public IEnumerable<KeyValuePair<string, object>> Parameters
        => this;


      /// <summary>
      /// Empty new builder
      /// </summary>
      public Builder(XBam.Archetype type, Universe universe = null) {
        Archetype = type;
        Universe = universe ?? type.Id.Universe;
        __parameters = null;
      }

      /// <summary>
      /// New builder from a collection of param names
      /// </summary>
      public Builder(XBam.Archetype type, IEnumerable<KeyValuePair<string, object>> @params, Universe universe = null) {
        Archetype = type;
        Universe = universe ?? Archetype.Id.Universe;
        __parameters = @params is not null 
          ? new(@params) 
          : null;
      }


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

      ///<summary><inheritdoc/></summary>
      public IEnumerable<string> Keys
        => ((IReadOnlyDictionary<string, object>)_parameters).Keys;

      ///<summary><inheritdoc/></summary>
      public IEnumerable<object> Values
        => ((IReadOnlyDictionary<string, object>)_parameters).Values;

      ///<summary><inheritdoc/></summary>
      public int Count
        => ((IReadOnlyCollection<KeyValuePair<string, object>>)_parameters).Count;

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
      public IBuilder<TModelBase> Add(KeyValuePair<string, object> parameter)
        => (IBuilder<TModelBase>)(this as IBuilder).Add(parameter);

      ///<summary><inheritdoc/></summary>
      public IBuilder<TModelBase> Add(string key, object value) {
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

      /// <summary>
      /// Build the model.
      /// </summary>
      public TModelBase Make() {
        TModelBase model = (TModelBase)((XBam.IFactory)Archetype)._modelConstructor(this);

        // init
        // Unique ID logic:
        if (model is IUnique unique) {
          model = (TModelBase)unique.InitializeId(this.TryToGet<string>(nameof(IUnique.Id)));
        }

        model = (TModelBase)(model as IModel).OnInitialized(Archetype, Universe, this);

        // configure
        model = (TModelBase)Archetype.ConfigureModel(this, model);

        IReadableComponentStorage componentStorage = model as IReadableComponentStorage;
        if(componentStorage != null) {
          model = _initializeModelComponents(componentStorage);
        }

#if DEBUG
        // Warns you if you've got Model Component settings in the Archetype but no Component Storage on the Model.
        else if(Archetype.InitialUnlinkedModelComponents.Any() || Archetype.ModelLinkedComponents.Any()) {
          Console.WriteLine($"The Archetype of Type: {Archetype}, provides components to set up on the produced model of type :{model.GetType()}, but this model does not inherit from the interface {nameof(IReadableComponentStorage)}. Maybe consider adding .WithComponents to the Model<[,]> base class you are inheriting from, or removing model components added to any of the Initial[Component...] properties of the Archetype");
#warning An archetype with a Model Base Type that does not inherit from IReadableComponentStorage has been provided with InitialUnlinkedModelComponentCtors values. These components may never be applied to the desired model if it does not inhert from IReadableComponentStorage
        }
#endif

        // finalize the parent model
        model = (TModelBase)(model as IModel).OnFinalized(this);
        model = (TModelBase)Archetype.FinalizeModel(this, model);

        // finalize the child components:
        if(componentStorage != null) {
          model = _finalizeModelComponents(componentStorage);
        }

        return model;
      }

      /// <summary>
      /// Loop though each model component and initialize them.
      /// This also adds all model data componnets linked to an archetype component first.
      /// </summary>
      TModelBase _initializeModelComponents(IReadableComponentStorage model) {
        var Parent = model as IModel;

        // add components built from a given ctor
        foreach ((string key, Func<IComponent.IBuilder, IModel.IComponent> ctor) in Archetype.InitialUnlinkedModelComponents) {
          XBam.IComponent.IBuilder componentBuilder = _makeComponentBuilder(Parent, key, out Type componentType);

          // no provided ctor, we need to get the default one.
          if (ctor == null) {
            // build the component:
            IModel.IComponent component = (XBam.Components.GetFactory(componentType) as XBam.Archetype)
              .Make(componentBuilder) as IModel.IComponent;

            model.AddComponent(component);
          } // else use the provided ctor:
          else {
            model.AddComponent(ctor(componentBuilder));
          }
        }

        /// add link components from the archetype
        foreach (XBam.Archetype.IComponent.ILinkedComponent linkComponent in Archetype.ModelLinkedComponents) {
          XBam.IComponent.IBuilder componentBuilder = _makeComponentBuilder(Parent, linkComponent.Key, out _);
          model.AddComponent(linkComponent.BuildDefaultModelComponent(componentBuilder, Archetype.Id.Universe));
        }

        return (TModelBase)model;
      }

      /// <summary>
      /// Loop though each model component and finalize them.
      /// </summary>
      TModelBase _finalizeModelComponents(IReadableComponentStorage model) {
        var Parent = model as IModel;
        foreach (IModel.IComponent component in model.ComponentsByBuilderKey.Values) {
          XBam.IComponent.IBuilder componentBuilder = _makeComponentBuilder(Parent, component.Key, out _);
          component.FinalizeAfterParent(model as IModel, componentBuilder);
        }

        return (TModelBase)model;
      }

      readonly XBam.IComponent.IBuilder _makeComponentBuilder(IModel Parent, string key, out Type componentType) {
        // TOOD: cache this
        componentType = Components.DefaultUniverse.Components.Get(key);
        // Make a builder to match this component with the params from the parent:
        var componentBuilder = ((IBuilderSource)Components.GetFactory(componentType))
          .Build() as IComponent.IBuilder;
        componentBuilder.__parameters = __parameters;
        componentBuilder.Parent = Parent;

        return componentBuilder;
      }
    }
  }
}