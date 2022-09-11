using Meep.Tech.XBam.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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
      public JsonSerializer JsonSerializer 
        { get; private set; } = null!;

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
          serializerOverride ?? Universe.TryToGetExtraContext<IModelJsonSerializer>()?.JsonSerializer!
        );

      /// <summary>
      /// Used to deserialize a model with this archetype from a json string by default
      /// </summary>
      protected virtual internal IModel? DeserializeModelFromJson(Archetype archetype, JObject json, Type? deserializeToTypeOverride = null, JsonSerializer? serializerOverride = null) {
        deserializeToTypeOverride
          ??= Universe.Models.GetModelTypeProducedBy(archetype);

        if (!archetype.ModelTypeProduced.IsAssignableFrom(deserializeToTypeOverride)) {
          throw new JsonException($"Cannot deserialize a Model of expected type:{archetype.ModelBaseType.FullName}, to a parent Model class({deserializeToTypeOverride.FullName}) explicitly.");
        }

        IModel? model = DeserializeModelFromJson(json, archetype, deserializeToTypeOverride, serializerOverride);

        if (model is null) {
          return default;
        }

        IModel.IBuilder.InitalizeModel(ref model, null, archetype, Universe);

        return model;
      }

      /// <summary>
      /// Helper containing the basic deserialization logic used by components and models.
      /// </summary>
      protected IModel? DeserializeModelFromJson(JObject json, Archetype archetype, System.Type deserializeToType, JsonSerializer? serializerOverride) {
        IModel? model
          = (IModel?)json.ToObject(
            deserializeToType,
            serializerOverride ?? Universe.TryToGetExtraContext<IModelJsonSerializer>()?.JsonSerializer!
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
      protected IBuilder? ConstructBuilderForModelDeserialization(Archetype archetype) 
        => archetype.GetGenericBuilderConstructor()(archetype, null);

      /// <summary>
      /// Used to deserialize a model with this archetype from a json string by default
      /// </summary>
      protected virtual internal IComponent? DeserializeComponentFromJson(Archetype archetype, JObject json, IReadableComponentStorage? ontoParent = null, Type? deserializeToTypeOverride = null, JsonSerializer? serializerOverride = null) {
        deserializeToTypeOverride
          ??= archetype.ModelTypeProduced;

        if (!archetype.ModelTypeProduced.IsAssignableFrom(deserializeToTypeOverride)) {
          throw new JsonException($"Cannot deserialize a Component of expected type:{archetype.ModelBaseType.FullName}, to a parent Component class({deserializeToTypeOverride.FullName}) explicitly.");
        }

        var component = (IComponent?)DeserializeModelFromJson(json, archetype, deserializeToTypeOverride, serializerOverride);

        if (component is null) {
          return default;
        }

        var builder = ((IComponent.IBuilder?)ConstructBuilderForModelDeserialization(archetype))
          ?.Modify(b => b.Parent = (IModel?)ontoParent);
        IComponent.IBuilder.InitalizeComponent(ref component, builder, ontoParent, archetype, Universe);
        (ontoParent as IWriteableComponentStorage)?.AddComponent(component);

        return component;
      }

      JObject IModelJsonSerializer.SerializeModelToJson(Archetype archetype, IModel model, JsonSerializer? serializerOverride) => SerializeModelToJson(archetype, model, serializerOverride);

      IModel? IModelJsonSerializer.DeserializeModelFromJson(Archetype archetype, JObject json, Type? deserializeToTypeOverride, JsonSerializer? serializerOverride)
        => DeserializeModelFromJson(archetype, json, deserializeToTypeOverride, serializerOverride);

      IComponent? IModelJsonSerializer.DeserializeComponentFromJson(Archetype archetype, JObject json, IReadableComponentStorage? ontoParent, Type? deserializeToTypeOverride, JsonSerializer? serializerOverride)
        => DeserializeComponentFromJson(archetype, json, ontoParent, deserializeToTypeOverride, serializerOverride);

      #region Extra Context Events

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