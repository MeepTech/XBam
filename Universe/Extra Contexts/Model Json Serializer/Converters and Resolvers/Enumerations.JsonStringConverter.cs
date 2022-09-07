using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Meep.Tech.XBam.Json.Configuration {

  public static partial class Enumerations {
    /// <summary>
    /// Json Converter for Enumerations
    /// </summary>
    public class JsonStringConverter : Newtonsoft.Json.JsonConverter<Enumeration?> {
      readonly bool _allowObjectDeserialization;

      /// <summary>
      /// The key used for the field containing the type data for an enum
      /// </summary>
      public const char EnumKeyAndTypeSplitterCharacter = '|';

      /// <summary>
      /// The universe this is for
      /// </summary>
      public Universe Universe {
        get;
      }

      ///<summary><inheritdoc/></summary>
      public JsonStringConverter(Universe universe, bool allowObjectDeserialization = true) {
        Universe = universe;
        _allowObjectDeserialization = allowObjectDeserialization;
      }

      ///<summary><inheritdoc/></summary>
      public override Enumeration? ReadJson(JsonReader reader, Type objectType, [AllowNull] Enumeration existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) {
          return null;
        } else if (reader.TokenType == JsonToken.String) {
          return ReadString(reader, Universe);
        } else if (_allowObjectDeserialization && reader.TokenType == JsonToken.StartObject) {
          return JsonObjectConverter.ReadObject(reader, serializer, Universe);
        }  else throw new JsonException();
      }

      /// <summary>
      /// Used to read an enum from a json string.
      /// </summary>
      protected internal static Enumeration? ReadString(JsonReader reader, Universe universe) {
        string? keyAndType = reader.ReadAsString();

        if (keyAndType is null) {
          return null;
        }

        var parts = keyAndType.Split(EnumKeyAndTypeSplitterCharacter);
        return universe.Enumerations.Get(
          parts.Last(),
          parts.First()
        );
      }

      ///<summary><inheritdoc/></summary>
      public override void WriteJson(JsonWriter writer, [AllowNull] Enumeration? value, JsonSerializer serializer) {
        if (value is null) {
          writer.WriteNull();
          return;
        }

        writer.WriteValue($"{value.ExternalId}{EnumKeyAndTypeSplitterCharacter}{value.EnumBaseType.FullName}");
      }
    }
  }
}
