using Meep.Tech.XBam.Json.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Xml.Linq;

namespace Meep.Tech.XBam.Json {
  public static class ModelJsonExtensions {

    #region Serialize

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    public static JObject ToJson(this IModel model, JsonSerializer? serializerOverride = null) {
      Archetype archetype = (Archetype)model.Factory;

      return archetype is IHaveCustomModelJsonSerializationLogic serializationLogic
        ? serializationLogic.SerializeModelToJson(model, serializerOverride)
        : model.Universe.GetExtraContext<IModelJsonSerializer>()
          .SerializeModelToJson(archetype, model, serializerOverride);
    }

    #endregion

    #region Deserialize

    #region From JObject

    /// <summary>
    /// Deserialize a model from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public static IModel? ToModel(
      this JObject jObject,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null,
      Universe? universe = default
    ) {
      universe ??= Models.DefaultUniverse;
      string key = jObject.Value<string>(nameof(Archetype).ToLower());

      return universe.GetExtraContext<IModelJsonSerializer>()
        .DeserializeModelFromJson(
          Archetypes.All.Get(key),
          jObject,
          deserializeToTypeOverride,
          serializerOverride
        );
    }

    /// <summary>
    /// Deserialize a model from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public static TModel? ToModel<TModel>(
      this JObject jObject,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null,
      Universe? universe = default
    ) where TModel : IModel 
      => (TModel?)ToModel(jObject, deserializeToTypeOverride, serializerOverride, universe);
    
    /// <summary>
    /// Deserialize a component from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetComponentTypeProducedBy</param>
    public static IComponent? ToComponent(
      this JObject jObject,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null,
      Universe? universe = default
    ) {
      universe ??= Components.DefaultUniverse;
      string key = jObject.Value<string>(Configuration.Models.ComponentsToJsonConverter.ComponentKeyPropertyName);

      return universe.GetExtraContext<IModelJsonSerializer>()
        .DeserializeComponentFromJson(
          Archetypes.All.Get(key),
          jObject,
          ontoParent,
          deserializeToTypeOverride,
          serializerOverride
        );
    }

    /// <summary>
    /// Deserialize a component from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetComponentTypeProducedBy</param>
    public static TComponent? ToComponent<TComponent>(
      this JObject jObject,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null,
      Universe? universe = default
    ) where TComponent : IComponent
      => (TComponent?)ToComponent(jObject, deserializeToTypeOverride, ontoParent, serializerOverride, universe);

    #endregion

    #region From String

    /// <summary>
    /// Deserialize a model from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public static IModel? ToModelFromJson(
      this string json,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null,
      Universe? universe = default
    ) {
      universe ??= Models.DefaultUniverse;
      string key = json.GetFirstJsonPropertyInstance<string>(nameof(Archetype).ToLower());

      return universe.GetExtraContext<IModelJsonSerializer>()
        .DeserializeModelFromJson(
          Archetypes.All.Get(key),
          JObject.Parse(json),
          deserializeToTypeOverride,
          serializerOverride
        );
    }

    /// <summary>
    /// Deserialize a model from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public static TModel? ToModelFromJson<TModel>(
      this string json,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null,
      Universe? universe = default
    ) where TModel : IModel
      => (TModel?)ToModelFromJson(json, deserializeToTypeOverride, serializerOverride, universe);

    /// <summary>
    /// Deserialize a component from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetComponentTypeProducedBy</param>
    public static IComponent? ToComponentFromJson(
      this string json,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null,
      Universe? universe = default
    ) {
      universe ??= Components.DefaultUniverse;
      string key = json.GetFirstJsonPropertyInstance<string>(Configuration.Models.ComponentsToJsonConverter.ComponentKeyPropertyName);

      return universe.GetExtraContext<IModelJsonSerializer>()
        .DeserializeComponentFromJson(
          Archetypes.All.Get(key),
          JObject.Parse(json),
          ontoParent,
          deserializeToTypeOverride,
          serializerOverride
        );
    }

    /// <summary>
    /// Deserialize a component from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetComponentTypeProducedBy</param>
    /// <param name="withConfigurationParameters">configuration paramaters to use while re-building the component.</param>
    public static TComponent? ToComponentFromJson<TComponent>(
      this string json,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null,
      Universe? universe = default,
      params (string key, object value)[] withConfigurationParameters
    ) where TComponent : IComponent
      => (TComponent?)ToComponentFromJson(json, deserializeToTypeOverride, ontoParent, serializerOverride, universe);

    #endregion

    #region From Archetype

    #region Models

    /// <summary>
    /// Use an archetype as a guide to make the model from json.
    /// </summary>
    public static IModel? MakeFromJson(
      this Archetype archetype,
      JObject json,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null
    ) => archetype.Universe.GetExtraContext<IModelJsonSerializer>()
      .DeserializeModelFromJson(
        archetype,
        json,
        deserializeToTypeOverride,
        serializerOverride
      );

    /// <summary>
    /// Use an archetype as a guide to make the model from json.
    /// </summary>
    public static TModel? MakeFromJson<TModel>(
      this Archetype archetype,
      JObject json,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null
    ) where TModel : IModel
      => (TModel?)archetype.Universe.GetExtraContext<IModelJsonSerializer>()
        .DeserializeModelFromJson(
          archetype,
          json,
          deserializeToTypeOverride,
          serializerOverride
        );

    /// <summary>
    /// Use an archetype as a guide to make the model from json.
    /// </summary>
    public static TModelBase? MakeFromJson<TModelBase, TArchetypeBase>(
      this Archetype<TModelBase, TArchetypeBase> archetype,
      JObject json,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null
    ) where TModelBase : IModel<TModelBase, TArchetypeBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      => (TModelBase?)archetype.Universe.GetExtraContext<IModelJsonSerializer>()
        .DeserializeModelFromJson(
          archetype,
          json,
          deserializeToTypeOverride,
          serializerOverride
        );

    /// <summary>
    /// Use an archetype as a guide to make the model from json.
    /// </summary>
    public static TModelBase? MakeFromJson<TModelBase, TArchetypeBase>(
      this Archetype<TModelBase, TArchetypeBase> archetype,
      string json,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null
    ) where TModelBase : IModel<TModelBase, TArchetypeBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      => archetype.MakeFromJson(JObject.Parse(json), deserializeToTypeOverride, serializerOverride);

    /// <summary>
    /// Use an archetype as a guide to make the model from json.
    /// </summary>
    public static IModel? MakeFromJson(
      this Archetype archetype,
      string json,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null
    ) => archetype.MakeFromJson(JObject.Parse(json), deserializeToTypeOverride, serializerOverride);

    /// <summary>
    /// Use an archetype as a guide to make the model from json.
    /// </summary>
    public static TModel? MakeFromJson<TModel>(
      this Archetype archetype,
      string json,
      Type? deserializeToTypeOverride = null,
      JsonSerializer? serializerOverride = null
    ) where TModel : IModel
      => archetype.MakeFromJson<TModel>(JObject.Parse(json), deserializeToTypeOverride, serializerOverride);

    #endregion

    #region Components

    /// <summary>
    /// Use an archetype as a guide to make the component from json.
    /// </summary>
    public static IComponent? MakeFromJson(
      this IComponent.IFactory factory,
      JObject json,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null
    ) => factory is Archetype archetype 
      ? factory.Universe.GetExtraContext<IModelJsonSerializer>()
        .DeserializeComponentFromJson(
          archetype,
          json,
          ontoParent,
          deserializeToTypeOverride,
          serializerOverride
        )
      : throw new InvalidOperationException();

    /// <summary>
    /// Use an archetype as a guide to make the component from json.
    /// </summary>
    public static TComponent? MakeFromJson<TComponent>(
      this IComponent.IFactory factory,
      JObject json,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null
    ) where TComponent : IComponent
      => factory is Archetype archetype
        ? (TComponent?)factory.Universe.GetExtraContext<IModelJsonSerializer>()
          .DeserializeComponentFromJson(
            archetype,
            json,
            ontoParent,
            deserializeToTypeOverride,
            serializerOverride
          )
        : throw new InvalidOperationException();

    /// <summary>
    /// Use an archetype as a guide to make the component from json.
    /// </summary>
    public static IComponent? MakeFromJson(
      this IComponent.IFactory factory,
      string json,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null
    ) => factory.MakeFromJson(JObject.Parse(json), deserializeToTypeOverride, ontoParent, serializerOverride);

    /// <summary>
    /// Use an archetype as a guide to make the component from json.
    /// </summary>
    public static TComponent? MakeFromJson<TComponent>(
      this IComponent.IFactory factory,
      string json,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null
    ) where TComponent : IComponent
      => factory.MakeFromJson<TComponent>(JObject.Parse(json), deserializeToTypeOverride, ontoParent, serializerOverride);

    /// <summary>
    /// Use an archetype as a guide to make the model from json.
    /// </summary>
    public static TComponentBase? MakeFromJson<TComponentBase>(
      this IComponent<TComponentBase>.Factory factory,
      JObject json,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null
    ) where TComponentBase : IComponent<TComponentBase>
      => (TComponentBase?)factory.Universe.GetExtraContext<IModelJsonSerializer>()
        .DeserializeComponentFromJson(
          factory,
          json,
          ontoParent,
          deserializeToTypeOverride,
          serializerOverride
        );

    /// <summary>
    /// Use an archetype as a guide to make the model from json.
    /// </summary>
    public static TComponentBase? MakeFromJson<TComponentBase>(
      this IComponent<TComponentBase>.Factory factory,
      string json,
      Type? deserializeToTypeOverride = null,
      IReadableComponentStorage? ontoParent = null,
      JsonSerializer? serializerOverride = null
    ) where TComponentBase : IComponent<TComponentBase>
      => factory.MakeFromJson(JObject.Parse(json), deserializeToTypeOverride, ontoParent, serializerOverride);

    #endregion

    #endregion

    #endregion
  }
}