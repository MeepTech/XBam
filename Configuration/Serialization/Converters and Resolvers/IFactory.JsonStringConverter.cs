using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.XBam.Json.Configuration {

  /// <summary>
  /// Constants and converters for Archetypes.
  /// </summary>
  public static partial class Archetypes {

    /// <summary>
    /// Used to convert an Archetype to a general string for storage
    /// </summary>
    public class JsonStringConverter : JsonConverter<IFactory> {

      /// <summary>
      /// The universe this is for
      /// </summary>
      public Universe Universe {
        get;
      }

      ///<summary><inheritdoc/></summary>
      public JsonStringConverter(Universe universe) {
        Universe = universe;
      }

      ///<summary><inheritdoc/></summary>
      public override void WriteJson(JsonWriter writer, [AllowNull] IFactory value, JsonSerializer serializer) {
        if (value is not null) {
          writer.WriteValue(value?.Id.Key);
        } else writer.WriteNull();
      }

      ///<summary><inheritdoc/></summary>
      public override IFactory ReadJson(JsonReader reader, Type objectType, [AllowNull] IFactory existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if(reader.Value is not string key) {
          throw new ArgumentNullException("archetype");
        }
        try {
          return Universe.Archetypes.Id[key].Archetype;
        } catch(Exception) {
          throw new KeyNotFoundException($"Archetype with key: {key}, not found");
        }
      }
    }
  } 
}
