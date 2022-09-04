using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Meep.Tech.XBam {
  public static class JsonExtensions {

    /// <summary>
    /// Try to get a value by type, case insensitive by default.
    /// </summary>
    public static T GetValue<T>(this JObject jObject, string property, StringComparison comparer = StringComparison.OrdinalIgnoreCase, string errorMessageOverride = null) {
      if (jObject.TryGetValue(property, comparer, out JToken valueToken)) {
        return valueToken.Value<T>();
      }

      throw new ArgumentException(errorMessageOverride ?? $"Property {property} not found in JObject. {(comparer == StringComparison.OrdinalIgnoreCase ? " Case Insensitive Search Applied." : "")}");
    }

    /// <summary>
    /// Try to get a value by type, case insensitive by default.
    /// </summary>
    public static T TryGetValue<T>(this JObject jObject, string property, StringComparison comparer = StringComparison.OrdinalIgnoreCase, T @default = default) {
      if (jObject.TryGetValue(property, comparer, out JToken valueToken)) {
        return valueToken.Value<T>();
      }

      return @default;
    }

    /// <summary>
    /// See if this has a property, case insensitive by default.
    /// </summary>
    public static bool HasProperty(this JObject jObject, string propertyName, StringComparison comparer = StringComparison.OrdinalIgnoreCase) {
      if (jObject.TryGetValue(propertyName, comparer, out _)) {
        return true;
      }

      return false;
    }

    /// <summary>
    /// See if this has a property, case insensitive by default.
    /// </summary>
    public static bool HasProperty(this JObject jObject, string propertyName, out string exactPropertyKey, StringComparison comparer = StringComparison.OrdinalIgnoreCase) {
      JProperty found;
      if ((found = jObject.Property(propertyName, comparer)) != null) {
        exactPropertyKey = found.Name;
        return true;
      }

      exactPropertyKey = null;
      return false;
    }

    /// <summary>
    /// remove a property if it exists, case insensitive by default.
    /// </summary>
    public static bool RemoveProperty(this JObject jObject, string propertyName, StringComparison comparer = StringComparison.OrdinalIgnoreCase) {
      if (jObject.Remove(propertyName)) {
        return true;
      }

      JProperty heightConfig = jObject.Property(propertyName, comparer);
      return jObject.Remove(heightConfig.Name);
    }
  }

  namespace Json {
    public static class JsonExtensions {

      /// <summary>
      /// Get the first instance of a property from a json string.
      /// </summary>
      public static T GetFirstJsonPropertyInstance<T>(this string json, string propertyName) {
        using var stringReader = new StringReader(json);
        using var jsonReader = new JsonTextReader(stringReader);

        while (jsonReader.Read()) {
          if (jsonReader.TokenType == JsonToken.PropertyName
            && (string)jsonReader.Value == propertyName
          ) {
            jsonReader.Read();

            JsonSerializer serializer = new();
            return serializer.Deserialize<T>(jsonReader);
          }
        }

        return default;
      }
    }
  }
}
