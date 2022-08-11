using Meep.Tech.Collections.Generic;
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
    public class JsonConverter : Newtonsoft.Json.JsonConverter<Enumeration> {
      public override Enumeration ReadJson(JsonReader reader, Type objectType, [AllowNull] Enumeration existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JObject value = serializer.Deserialize<JObject>(reader);
        string key = value.Value<string>(Model.Serializer.EnumTypePropertyName);
        string[] parts = key.Split('@');
        Universe universe = parts.Length == 1 
          ? Archetypes.DefaultUniverse 
          : Universe.s.TryToGet(parts.Last())
            ?? Archetypes.DefaultUniverse;

        return universe.Enumerations.Get(
          parts.First(),
          value.Value<string>("externalId")
        );
      }

      public override void WriteJson(JsonWriter writer, [AllowNull] Enumeration value, JsonSerializer serializer) {
        serializer.Converters.Remove(this);
        JObject serialized = JObject.FromObject(value, serializer);
        serializer.Converters.Add(this);
        string key = value.Universe.Key.Equals(Archetypes.DefaultUniverse.Key)
            ? $"{value.EnumBaseType.FullName}@{value.Universe.Key}"
            : value.EnumBaseType.FullName;

        serialized.Add(
          Model.Serializer.EnumTypePropertyName,
          key
        );
        serializer.Serialize(writer, serialized);
      }
    }
  }
}
