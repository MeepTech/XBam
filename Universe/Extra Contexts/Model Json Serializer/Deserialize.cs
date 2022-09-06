using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Meep.Tech.XBam.Json {
  /// <summary>
  /// Access default json deserialize logic
  /// </summary>
  public static class Deserialize {

    /// <summary>
    /// Deserialize a jobject to a model
    /// </summary>
    public static IModel? ToModel(
      JObject jObject,
      Type? deserializeToType = default,
      Universe? universe = default,
      JsonSerializer? serializer = default,
      params (string key, object value)[] withConfigurationParameters
    ) => jObject.ToModel(deserializeToType, serializer, universe, withConfigurationParameters);

    /// <summary>
    /// Deserialize a string to a model
    /// </summary>
    public static IModel? ToModel(
      string json,
      Type? deserializeToType = default,
      Universe? universe = default,
      JsonSerializer? serializer = default,
      params (string key, object value)[] withConfigurationParameters
    ) => json.ToModelFromJson(deserializeToType, serializer, universe, withConfigurationParameters);

    /// <summary>
    /// Deserialize a jobject to a model
    /// </summary>
    public static TModel? ToModel<TModel>(
      JObject jObject,
      Type? deserializeToType = default,
      Universe? universe = default,
      JsonSerializer? serializer = default,
      params (string key, object value)[] withConfigurationParameters
    ) where TModel : IModel
      => jObject.ToModel<TModel>(deserializeToType, serializer, universe, withConfigurationParameters);

    /// <summary>
    /// Deserialize a string to a model
    /// </summary>
    public static TModel? ToModel<TModel>(
      string json,
      Type? deserializeToType = default,
      Universe? universe = default,
      JsonSerializer? serializer = default,
      params (string key, object value)[] withConfigurationParameters
    ) where TModel : IModel
      => json.ToModelFromJson<TModel>(deserializeToType, serializer, universe, withConfigurationParameters);

    /// <summary>
    /// Deserialize a jobject to a component
    /// </summary>
    public static IComponent? ToComponent(
      JObject jObject,
      Type? deserializeToType = default,
      Universe? universe = default,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializer = default,
      params (string key, object value)[] withConfigurationParameters
    ) => jObject.ToComponent(deserializeToType, ontoParent, serializer, universe, withConfigurationParameters);

    /// <summary>
    /// Deserialize a string to a component
    /// </summary>
    public static IComponent? ToComponent(
      string json,
      Type? deserializeToType = default,
      Universe? universe = default,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializer = default,
      params (string key, object value)[] withConfigurationParameters
    ) => json.ToComponentFromJson(deserializeToType, ontoParent, serializer, universe, withConfigurationParameters);

    /// <summary>
    /// Deserialize a jobject to a component
    /// </summary>
    public static TComponent? ToComponent<TComponent>(
      JObject jObject,
      Type? deserializeToType = default,
      Universe? universe = default,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializer = default,
      params (string key, object value)[] withConfigurationParameters
    ) where TComponent : IComponent
      => jObject.ToComponent<TComponent>(deserializeToType, ontoParent, serializer, universe, withConfigurationParameters);

    /// <summary>
    /// Deserialize a string to a component
    /// </summary>
    public static TComponent? ToComponent<TComponent>(
      string json,
      Type? deserializeToType = default,
      Universe? universe = default,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializer = default,
      params (string key, object value)[] withConfigurationParameters
    ) where TComponent : IComponent
      => json.ToComponentFromJson<TComponent>(deserializeToType, ontoParent, serializer, universe, withConfigurationParameters);
  }
}