using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Meep.Tech.XBam.Json.Configuration {

  /// <summary>
  /// Enumerations based Json Classes and Helpers
  /// </summary>
  public static partial class Enumerations {

    /// <summary>
    /// Json Converter for Enumerations
    /// </summary>
    public class JsonObjectConverter : JsonConverter<Enumeration?> {
      readonly bool _allowStringDeserialization;

      /// <summary>
      /// The key used for the field containing the type data for an enum
      /// </summary>
      public const string EnumTypePropertyName = "type";

      /// <summary>
      /// The key used for the field containing the value/externalId/key data for the enum
      /// </summary>
      public const string EnumKeyPropertyName = "key";

      /// <summary>
      /// The universe this is for
      /// </summary>
      public Universe Universe {
        get;
      }

      ///<summary><inheritdoc/></summary>
      public JsonObjectConverter(Universe universe, bool allowStringDeserialization = true) {
        Universe = universe;
        _allowStringDeserialization = allowStringDeserialization;
      }

      ///<summary><inheritdoc/></summary>
      public override Enumeration? ReadJson(JsonReader reader, Type objectType, [AllowNull] Enumeration existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) {
          return null;
        } else if (reader.TokenType == JsonToken.StartObject) { 
          return ReadObject(reader, serializer, Universe);
        } else if (_allowStringDeserialization && reader.TokenType == JsonToken.String) {
          return JsonStringConverter.ReadString(reader, Universe);
        } else throw new JsonException();
      }

      /// <summary>
      /// Read the jobject as an enum.
      /// </summary>
      internal protected static Enumeration? ReadObject(JsonReader reader, JsonSerializer serializer, Universe universe) {
        JObject? value = serializer.Deserialize<JObject>(reader);

        if (value is null || value.Type == JTokenType.Null) {
          return null;
        }

        string typeName = value.Value<string>(EnumTypePropertyName);
        return universe.Enumerations.Get(
          typeName,
          value.Value<string>(EnumKeyPropertyName)
        );
      }

      ///<summary><inheritdoc/></summary>
      public override void WriteJson(JsonWriter writer, [AllowNull] Enumeration? value, JsonSerializer serializer) {
        if (value is null) {
          writer.WriteNull();
          return;
        }

        /*serializer.Converters.Remove(this);*/
        JObject serialized = JObject.FromObject(value, serializer);
        /*serializer.Converters.Add(this);*/

        serializer.Serialize(writer, serialized);
      }
    }
  }
}
