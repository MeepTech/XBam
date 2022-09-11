using Meep.Tech.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.XBam.Json.Configuration {

  /// <summary>
  /// Constnants and json converters for models.
  /// </summary>
  public static partial class Models {

    /// <summary>
    /// Used to convert a collection of components to and from a json object by key
    /// </summary>
    public class ComponentsToJsonConverter : JsonConverter<IReadableComponentStorage.ReadOnlyModelComponentCollection> {

      /// <summary>
      /// The key used for the field containing the key for the component
      /// </summary>
      public const string ComponentKeyPropertyName = "key";

      /// <summary>
      /// The universe this is for
      /// </summary>
      public Universe Universe {
        get;
      }

      ///<summary><inheritdoc/></summary>
      public ComponentsToJsonConverter(Universe universe) {
        Universe = universe;
      }

      ///<summary><inheritdoc/></summary>
      public override void WriteJson(JsonWriter writer, [NotNull] IReadableComponentStorage.ReadOnlyModelComponentCollection? value, JsonSerializer serializer) {
        writer.WriteStartObject();
        value!.ForEach(e => {
          writer.WritePropertyName(e.Key);
          serializer.Serialize(writer, e.Value.ToJson(serializer));
        });
        writer.WriteEndObject();
      }

      ///<summary><inheritdoc/></summary>
      public override IReadableComponentStorage.ReadOnlyModelComponentCollection ReadJson(JsonReader reader, Type objectType, [NotNull] IReadableComponentStorage.ReadOnlyModelComponentCollection? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType != JsonToken.StartObject) {
          throw new JsonException($"Components Field for ECSBAM Models requires an object Jtoken to deserialize");
        }

        JObject componentsJson = serializer.Deserialize<JObject>(reader)!;

        Dictionary<string, IModel.IComponent> components = new();
        foreach (var (key, componentToken) in componentsJson) {
          components.Add(key, (IModel.IComponent)Deserialize.ToComponent((JObject)componentToken!, ontoParent: (IReadableComponentStorage?)existingValue!.Storage)!);
        }

        return new IReadableComponentStorage.ReadOnlyModelComponentCollection(existingValue!.Storage, components);
      }
    }
  }
}
