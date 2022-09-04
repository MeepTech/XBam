using Meep.Tech.Collections.Generic;
using Meep.Tech.XBam.Configuration;
using Meep.Tech.XBam.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Meep.Tech.XBam {

  /// <summary>
  /// The base interface for all XBam Models.
  /// A Model is a mutable grouping of data fields that can be produced by an Archetype.
  /// This is the non generic for Utility only, don't inherit from this direclty; Extend IModel[], Model[], or Model[,], or  Model[,].IFromInterface instead.
  /// </summary>
  /// <see cref="IModel{TModelBase}"/>
  /// <see cref="Model{TModelBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}.IFromInterface"/>
  public partial interface IModel: IResource {
    Universe IResource.Universe => Universe;

    /// <summary>
    /// The factory or archetype used to build this model
    /// </summary>
    public XBam.IFactory Factory { get; }

    /// <summary>
    /// The universe this model was made in
    /// </summary>
    public new Universe Universe {
      get;
      internal set;
    }

    /// <summary>
    /// Overrideable static initializer for model classes.
    /// Called right after the static initializer
    /// </summary>
    /// <param name="universe">The current universe being set up</param>
    protected internal static void Setup(Universe universe) { }

    /// <summary>
    /// Initializes the universe, archetype, factory and other built in model links.
    /// To set the archetype of a model in a custom public constructor, call this method.
    /// </summary>
    protected internal IModel OnInitialized([NotNull] Archetype factoryArchetype, Universe? universe = null, XBam.IBuilder? builder = null)
      => this;

    /// <summary>
    /// Can be overriden to finalize the model on build.
    /// </summary>
    protected internal IModel OnFinalized(XBam.IBuilder? builder)
      => this;

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// </summary>
    public IModel Copy() {
      string originalJson = null;
      if (this.CanLog(out var logger)) {
        originalJson = ToJson().ToString();
        logger.AppendActionToLog(
          Configuration.ModelLog.Entry.ActionType.Copied,
          originalJson,
          null,
          new Dictionary<string, object> { { Configuration.ModelLog.Entry.MetadataField.IsACopy.Key, false } }
        );
      }

      var copy = Universe.ModelSerializer.ModelCopyMethod(this);

      if (logger is not null) { 
        ((IReadableComponentStorage)copy).Log(
          Configuration.ModelLog.Entry.ActionType.Copied,
          "null",
          new KeyValuePair<string, object>[] { new("original", originalJson) },
          new Dictionary<string, object> { { Configuration.ModelLog.Entry.MetadataField.IsACopy.Key, true} }
        );
      }

      return copy;
    }

    #region Json Serialization

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    public JObject ToJson(JsonSerializer serializerOverride = null) 
      => ((Archetype)Factory).SerializeModelToJson(this, serializerOverride);
    

    /// <summary>
    /// Deserialize a model from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    /// <param name="withConfigurationParameters">configuration paramaters to use while re-building the model.</param>
    public static IModel FromJson(
      string json,
      Type deserializeToTypeOverride = null,
      Universe universe = default,
      params (string key, object value)[] withConfigurationParameters
    ) {
      universe ??= Models.DefaultUniverse;
      string key = json.GetFirstJsonPropertyInstance<string>(nameof(Archetype).ToLower());
      return universe.Archetypes.All.Get(key)
        .DeserializeModelFromJson(json, deserializeToTypeOverride, withConfigurationParameters);
    }

    /// <summary>
    /// Deserialize a model from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    /// <param name="withConfigurationParameters">configuration paramaters to use while re-building the model.</param>
    public static IModel FromJson(
      JObject jObject,
      Type deserializeToTypeOverride = null,
      Universe universe = default,
      params (string key, object value)[] withConfigurationParameters
    ) {
      universe ??= Models.DefaultUniverse;
      string key = jObject.Value<string>(nameof(Archetype).ToLower());
      return universe.Archetypes.All.Get(key)
        .DeserializeModelFromJson(jObject, deserializeToTypeOverride, withConfigurationParameters);
    }

    #endregion
  }

  /// <summary>
  /// The base interface for all 'Simple' XBam Models.
  /// A Model is a mutable grouping of data fields that can be produced by an Archetype.
  /// Simple Models have a single non-branching Archetype called a BuilderFactory.
  /// </summary>
  /// <see cref="Model{TModelBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}.IFromInterface"/>
  public partial interface IModel<TModelBase>
    : IModel
    where TModelBase : IModel<TModelBase>
  {

    /// <summary>
    /// For the base configure calls
    /// </summary>
    IModel IModel.OnInitialized(Archetype archetype, Universe? universe, XBam.IBuilder? builder) {
      Universe = universe ?? builder?.Archetype.Id.Universe ?? Universe.Default;

      return this;
    }

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModelBase FromJson(
       JObject jObject,
       Type deserializeToTypeOverride = null,
       Universe universe = default,
       params (string key, object value)[] withConfigurationParameters
     ) => (TModelBase)IModel.FromJson(jObject, deserializeToTypeOverride, universe, withConfigurationParameters);

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModelBase FromJson(
       string json,
       Type deserializeToTypeOverride = null,
       Universe universe = default,
       params (string key, object value)[] withConfigurationParameters
     ) => (TModelBase)IModel.FromJson(json, deserializeToTypeOverride, universe, withConfigurationParameters);
  }

  /// <summary>
  /// The base interface for all Branchable XBam Models.
  /// A Model is a mutable grouping of data fields that can be produced by an Archetype.
  /// Brnachable XBam models can have branching inheritance trees for both the Archetypes that produce the Models, and the Models produced.
  /// This is the nbase interface for utility only, don't inherit from this directly; Extend IModel[], Model[], or Model[,], or  Model[,].IFromInterface instead.
  /// </summary>
  /// <see cref="IModel{TModelBase}"/>
  /// <see cref="Model{TModelBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}.IFromInterface"/>
  public interface IModel<TModelBase, TArchetypeBase>
    : IModel<TModelBase>
    where TModelBase : IModel<TModelBase, TArchetypeBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// The archetype for this model
    /// </summary>
    public TArchetypeBase Archetype {
      get;
      internal set;
    }
    XBam.IFactory IModel.Factory
      => Archetype;

    /// <summary>
    /// For the base configure calls
    /// </summary>
    IModel IModel.OnInitialized(Archetype archetype, Universe? universe, XBam.IBuilder? builder) {
      Archetype = (TArchetypeBase)(archetype ?? builder?.Archetype);
      Universe = universe ?? builder?.Archetype.Id.Universe ?? Universe.Default;

      return this;
    }

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    JObject IModel.ToJson(JsonSerializer serializerOverride = null)
      => Archetype.SerializeModelToJson(this, serializerOverride);

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModelBase FromJson(
       JObject jObject,
       Type deserializeToTypeOverride = null,
       Universe universeOverride = null,
       params (string key, object value)[] withConfigurationParameters
     ) => IModel<TModelBase>.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);
  }

  /// <summary>
  /// Extension methods for models
  /// </summary>
  /// <see cref="IModel"/>
  public static class IModelExtensions {

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    public static JObject ToJson(this IModel model, JsonSerializer serializerOverride = null)
      => model.ToJson(serializerOverride);

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// Overrideable via IModel.copy()
    /// </summary>
    public static IModel Copy(this IModel original)
      => original.Copy();
  }
}
