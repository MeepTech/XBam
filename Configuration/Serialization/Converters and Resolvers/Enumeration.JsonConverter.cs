using Meep.Tech.Collections.Generic;
using Meep.Tech.XBam.Json.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Meep.Tech.XBam {

  public abstract partial class Enumeration {

    /// <summary>
    /// Json Converter for Enumerations
    /// </summary>
    public class JsonConverter : Newtonsoft.Json.JsonConverter<Enumeration?> {

      /// <summary>
      /// The universe this is for
      /// </summary>
      public Universe Universe {
        get;
      }

      ///<summary><inheritdoc/></summary>
      public JsonConverter(Universe universe) {
        Universe = universe;
      }

      ///<summary><inheritdoc/></summary>
      public override Enumeration? ReadJson(JsonReader reader, Type objectType, [AllowNull] Enumeration existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JObject? value = serializer.Deserialize<JObject>(reader);

        if (value is null || value.Type == JTokenType.Null) {
          return null;
        }

        string key = value.Value<string>(ModelJsonSerializerContext.EnumTypePropertyName);
        string[] parts = key.Split('@');

        return Universe.Enumerations.Get(
          parts.First(),
          value.Value<string>("externalId")
        );
      }

      ///<summary><inheritdoc/></summary>
      public override void WriteJson(JsonWriter writer, [AllowNull] Enumeration? value, JsonSerializer serializer) {
        serializer.Converters.Remove(this);
        JObject serialized = JObject.FromObject(value, serializer);
        serializer.Converters.Add(this);
        string key = value.EnumBaseType.FullName;

        serialized.Add(
          ModelJsonSerializerContext.EnumTypePropertyName,
          key
        );
        serializer.Serialize(writer, serialized);
      }
    }
  }
}
