using Meep.Tech.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.XBam {
  public partial class Archetype {
  }


  public partial interface IFactory {

    /// <summary>
    /// Used to convert an Archetype to a general string for storage
    /// </summary>
    public class JsonStringConverter : JsonConverter<IFactory> {

      public override void WriteJson(JsonWriter writer, [AllowNull] IFactory value, JsonSerializer serializer) {
        writer.WriteValue(value.Id.Key + (!string.IsNullOrEmpty(value.Id.Universe.Key)
          ? "@" + value.Id.Universe.Key
          : "")
        );
      }

      public override IFactory ReadJson(JsonReader reader, Type objectType, [AllowNull] IFactory existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if(reader.Value is not string key) {
          throw new ArgumentNullException("archetype");
        }

        string[] parts = key.Split("@");
        if(parts.Length == 1) {
          try {
            return Archetypes.Id[key].Archetype;
          } catch(Exception) {
            throw new KeyNotFoundException($"Archetype with key: {key}, not found");
          }
        } else if(parts.Length == 2) {
          Universe universe;
          try {
            universe = Universe.s.TryToGet(parts[1]);
          } catch(Exception) {
            throw new KeyNotFoundException($"Universe with name: {parts[1]}, not found.");
          }
          try {
            return universe.Archetypes.Id[parts[0]].Archetype;
          } catch(Exception) {
            throw new KeyNotFoundException($"Archetype with name: {parts[0]}, not found in universe: {parts[1]},");
          }

        } else
          throw new ArgumentException($"JsonStringConverter Not Parse an existing Archetype from the key: '{key}'");
      }
    }
  } 
}
