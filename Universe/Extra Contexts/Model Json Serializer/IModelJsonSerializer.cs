using Meep.Tech.XBam.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Meep.Tech.XBam.Json.Configuration {

  /// <summary>
  /// Logic and Settings Used To Serialize Models to json
  /// </summary>
  public partial interface IModelJsonSerializer : IExtraUniverseContextType {

    /// <summary>
    /// The options
    /// </summary>
    new ISettings Options 
      { get; }

    /// <summary>
    /// Compiled model serializer from the settings config function
    /// </summary>
    JsonSerializerSettings JsonSettings { get; }

    /// <summary>
    /// Compiled model serializer from the settings
    /// </summary>
    JsonSerializer JsonSerializer { get; }

    /// <summary>
    /// Used to serialize a model with this archetype to a jobject by default
    /// </summary>
    protected internal JObject SerializeModelToJson(Archetype archetype, IModel model, JsonSerializer? serializerOverride = null);

    /// <summary>
    /// Used to deserialize a model with this archetype from a json string by default
    /// </summary>
    protected internal IModel? DeserializeModelFromJson(Archetype archetype, JObject json, System.Type? deserializeToTypeOverride = null, JsonSerializer? serializerOverride = null, params (string key, object value)[] withConfigurationParameters);

    /// <summary>
    /// Used to serialize a model with this archetype to a jobject by default
    /// </summary>
    protected internal IComponent? DeserializeComponentFromJson(Archetype archetype, JObject json, IReadableComponentStorage? ontoParent = null, Type? deserializeToTypeOverride = null, JsonSerializer? serializerOverride = null, params (string key, object value)[] withConfigurationParameters);
  }
}