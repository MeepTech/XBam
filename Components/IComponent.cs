using Meep.Tech.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// This is the non-generic for utility reasons.
  /// </summary>
  public partial interface IComponent : IModel {

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public new Universe Universe {
      get;
      internal protected set;
    } Universe IModel.Universe {
      get => Universe;
      set => Universe = value;
    }

    /// <summary>
    /// Access to the builder factory for this type of component
    /// </summary>
    new XBam.IComponent.IFactory Factory {
      get;
      protected internal set;
    }

    XBam.IFactory IModel.Factory
      => Factory;

    /// <summary>
    /// A key used to index this type of component. There can only be one componet per key on a model or archetype
    /// This is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
    /// </summary>
    string Key
      => Factory.Key;

    /// <summary>
    /// optional finalization logic for components pre-attached to models after the model has been finalized
    /// </summary>
    /*public XBam.IComponent FinalizeAfterParent(IModel parent, IBuilder builder)
      => this;*/

    internal static void _setUniverse(ref XBam.IComponent component, Universe universe) {
      component.Universe = universe;
    }

    #region Json Serialization

    /// <summary>
    /// Turn the component into a serialized data object.
    /// </summary>
    /*public JObject ToJson(JsonSerializer overrideSerializer = null) {
      JObject.FromObject(this, overrideSerializer ?? (Universe ?? Components.DefaultUniverse).ModelSerializer.JsonSerializer);
      if (json is JObject jsonObject) {
        jsonObject.Add(Model.Serializer.ComponentKeyPropertyName, Key);
        return jsonObject;
      } else
        if (json is JArray jsonArray)
        return new JObject {
            {Model.Serializer.ComponentValueCollectionPropertyName, jsonArray },
            {Model.Serializer.ComponentKeyPropertyName, Key }
          };
      else
        throw new NotImplementedException($"Component of type {Key} must be serializable to a JObject or JArray by default using it's Universe.ModelSerializer.ComponentJsonSerializer.");
    }*/

    JObject IModel.ToJson(JsonSerializer overrideSerializer = null)
      => ToJson();

    /// <summary>
    /// Make a component from a jobject
    /// </summary>
    public static XBam.IComponent FromJson(
      JObject jObject,
      IModel ontoParent = null,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      params (string key, object value)[] withConfigurationParameters
    ) => FromJson(
      jObject,
      ontoParent,
      deserializeToTypeOverride,
      universeOverride,
      (IEnumerable<(string key, object value)>)withConfigurationParameters
    );

    /// <summary>
    /// Make a component from a jobject
    /// </summary>
    public static XBam.IComponent FromJson(
      JObject jObject,
      IModel ontoParent = null,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      IEnumerable<(string key, object value)> withConfigurationParameters = null
    ) {
      var component = (IComponent)FromJson(jObject, deserializeToTypeOverride, universeOverride);
      IBuilder builder = null;

      if (withConfigurationParameters?.Any() ?? false) {
        builder 
          = (IBuilder)
            (component.Factory as Archetype)
              .GetGenericBuilderConstructor()(
                (component.Factory as Archetype),
                withConfigurationParameters.ToDictionary(
                  p => p.key,
                  p => p.value
                )
              );

        builder.Parent = ontoParent;
      }

      IBuilder.InitalizeComponent(ref component, builder, (Archetype)component.Factory, universeOverride ?? component.Factory.Id.Universe);

      (ontoParent as IWriteableComponentStorage)?.AddComponent(component);

      return component;
    }

    #endregion
  }

  /// <summary>
  /// The base interface for components without branching archet
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent 
    where TComponentBase : IComponent<TComponentBase> {

    /// <summary>
    /// For overriding base configure calls
    /// </summary>
    XBam.IComponent OnInitialized(XBam.IBuilder builder)
      => this;

    ///<summary><inheritdoc/></summary>
    IModel IModel.OnInitialized(Archetype archetype, Universe? universe, XBam.IBuilder? builder) {
      Universe = universe ?? builder?.Archetype.Id.Universe ?? Universe.Default;
      ((XBam.IComponent)(this)).Factory = (IFactory)(archetype ?? builder?.Archetype);

      return OnInitialized(builder);
    }
  }

  /// <summary>
  /// Serialization Related Component Extensions
  /// </summary>
  public static class ComponentExtensions {

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    public static JObject ToJson(this IComponent component)
      => component.ToJson(null);

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    public static JObject ToJson(this IComponent component, Universe universe)
      => component.ToJson(universe);

    /// <summary>
    /// Helper function to fetch the key for this component type
    /// A key used to index this type of component. There can only be one componet per key on a model or archetype
    /// The Key is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
    /// </summary>
    public static string GetKey(this IComponent component)
    => component.Key;
  }
}
