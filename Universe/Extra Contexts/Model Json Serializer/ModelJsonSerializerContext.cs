using Meep.Tech.XBam.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;

namespace Meep.Tech.XBam.Json {

  namespace Configuration {

    /// <summary>
    /// Logic and Settings Used To Serialize Models
    /// </summary>
    public partial class ModelJsonSerializerContext : Universe.ExtraContext, IModelJsonSerializer {
      IModelJsonSerializer.ISettings IModelJsonSerializer.Options
        => Options;
      JsonSerializerSettings? _modelJsonSerializerSettings;

      /// <summary>
      /// The key used for the field containing the type data for an enum
      /// </summary>
      public const string EnumTypePropertyName = "__type_";

      /// <summary>
      /// The serializer options
      /// </summary>
      public new Settings Options
        => base.Options as Settings
          ?? throw new ArgumentNullException(nameof(Options));

      ///<summary><inheritdoc/></summary>
      public JsonSerializerSettings JsonSettings
        => _modelJsonSerializerSettings
          ??= new JsonSerializerSettings()
            .Modify(s => Options.ConfigureJsonSerializerSettings(Options, s));

      ///<summary><inheritdoc/></summary>
      public JsonSerializer JsonSerializer { get; private set; }

      ///<summary><inheritdoc/></summary>
      public Func<IModel, IModel> CopyMethod {
        get;
        internal set;
      }

      /// <summary>
      /// Make a new serializer for a universe
      /// </summary>
      public ModelJsonSerializerContext(Settings? options = null)
        : base(options ?? new Settings()) { }

      /// <summary>
      /// Used to serialize a model with this archetype to a jobject by default
      /// </summary>
      protected internal virtual JObject SerializeModelToJson(Archetype archetype, IModel model, JsonSerializer? serializerOverride = null)
        => JObject.FromObject(
          model,
          serializerOverride ?? Universe.TryToGetExtraContext<IModelJsonSerializer>()?.JsonSerializer
        );

      /// <summary>
      /// Used to deserialize a model with this archetype from a json string by default
      /// </summary>
      protected virtual internal IModel? DeserializeModelFromJson(Archetype archetype, JObject json, Type? deserializeToTypeOverride = null, JsonSerializer? serializerOverride = null, params (string key, object value)[] withConfigurationParameters) {
        deserializeToTypeOverride
          ??= Universe.Models.GetModelTypeProducedBy(archetype);

        IModel? model = DeserializeModelFromJson(json, archetype, deserializeToTypeOverride, serializerOverride);

        if (model is null) {
          return default;
        }

        var builder = (IModel.IBuilder?)ConstructBuilderForModelDeserialization(archetype, withConfigurationParameters);
        IModel.IBuilder.InitalizeModel(ref model, builder, archetype, Universe);

        return model;
      }

      /// <summary>
      /// Helper containing the basic deserialization logic used by components and models.
      /// </summary>
      protected IModel? DeserializeModelFromJson(JObject json, Archetype archetype, System.Type deserializeToType, JsonSerializer? serializerOverride) {
        IModel? model
          = (IModel?)json.ToObject(
            deserializeToType,
            serializerOverride ?? Universe.TryToGetExtraContext<IModelJsonSerializer>()?.JsonSerializer
          );

        if (model is null) {
          return default;
        }

        // default init and configure.
        if (!model.Factory.Id.Equals(archetype.Id)) {
          throw new InvalidCastException($"Tried to use Archetype: {archetype.Id.Key}. To deserialize model with Archetype: {model.Factory.Id}");
        }

        return model;
      }

      /// <summary>
      /// Helper function for making a builder for post-deserialization initialization.
      /// </summary>
      protected IBuilder? ConstructBuilderForModelDeserialization(Archetype archetype, (string key, object value)[] withConfigurationParameters) 
        => withConfigurationParameters.Any()
          ? archetype.GetGenericBuilderConstructor()(archetype, withConfigurationParameters.ToDictionary(p => p.key, p => p.value))
          : archetype.GetGenericBuilderConstructor()(archetype, null);

      /// <summary>
      /// Used to deserialize a model with this archetype from a json string by default
      /// </summary>
      protected virtual internal IComponent? DeserializeComponentFromJson(Archetype archetype, JObject json, IReadableComponentStorage? ontoParent = null, Type? deserializeToTypeOverride = null, JsonSerializer? serializerOverride = null, params (string key, object value)[] withConfigurationParameters) {
        deserializeToTypeOverride
          ??= Universe.Models.GetModelTypeProducedBy(archetype);

        var component = (IComponent?)DeserializeModelFromJson(json, archetype, deserializeToTypeOverride, serializerOverride);

        if (component is null) {
          return default;
        }

        var builder = ((IComponent.IBuilder?)ConstructBuilderForModelDeserialization(archetype, withConfigurationParameters))
          ?.Modify(b => b.Parent = (IModel?)ontoParent);
        IComponent.IBuilder.InitalizeComponent(ref component, builder, ontoParent, archetype, Universe);
        (ontoParent as IWriteableComponentStorage)?.AddComponent(component);

        return component;
      }

      JObject IModelJsonSerializer.SerializeModelToJson(Archetype archetype, IModel model, JsonSerializer? serializerOverride) => SerializeModelToJson(archetype, model, serializerOverride);

      IModel? IModelJsonSerializer.DeserializeModelFromJson(Archetype archetype, JObject json, Type? deserializeToTypeOverride, JsonSerializer? serializerOverride, params (string key, object value)[] withConfigurationParameters)
        => DeserializeModelFromJson(archetype, json, deserializeToTypeOverride, serializerOverride, withConfigurationParameters);

      IComponent? IModelJsonSerializer.DeserializeComponentFromJson(Archetype archetype, JObject json, IReadableComponentStorage? ontoParent = null, Type? deserializeToTypeOverride = null, JsonSerializer? serializerOverride = null, params (string key, object value)[] withConfigurationParameters)
        => DeserializeComponentFromJson(archetype, json, ontoParent, deserializeToTypeOverride, serializerOverride, withConfigurationParameters);

      #region Loader Context Overrides

      ///<summary><inheritdoc/></summary>
      protected internal override Action<Universe> OnLoaderInitializationComplete
        => universe => JsonSerializer = JsonSerializer.Create(JsonSettings);

      ///<summary><inheritdoc/></summary>
      protected internal override Action OnLoaderFinalizeStart
        => () => {
          // add the converters to the default json serializer settings.
          bool defaultExists;
          JsonSerializerSettings @default;
          if (JsonConvert.DefaultSettings is not null) {
            @default = JsonConvert.DefaultSettings();
            defaultExists = true;
          }
          else {
            @default = new JsonSerializerSettings();
            defaultExists = false;
          }

          JsonConvert.DefaultSettings 
            = () => @default.UpdateJsonSerializationSettings(Universe, !defaultExists, extraContext: this);
        };

      #endregion
    }
  }
}