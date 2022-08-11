using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Nodepad.Server {

  /// <summary>
  /// A simple and easy auto converter class to help get a list of items from what could be a single object or list of the given object in json.
  /// </summary>
  public class JsonArrayOrSingleObject<T> 
    : List<T> 
  {

    /// <summary>
    /// Json converter for ListOrSingleJsonObject
    /// </summary>
    public class Converter : JsonConverter<JsonArrayOrSingleObject<T>> {

      ///<summary><inheritdoc/></summary>
      public override JsonArrayOrSingleObject<T> ReadJson(JsonReader reader, Type objectType, [AllowNull] JsonArrayOrSingleObject<T> existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.StartArray) {
          var list = new JsonArrayOrSingleObject<T>();
          serializer.Deserialize<List<T>>(reader)
            .ForEach(list.Add);
          return list;
        } else {
          return new JsonArrayOrSingleObject<T> {
            serializer.Deserialize<T>(reader)
          };
        }
      }

      ///<summary><inheritdoc/></summary>
      public override void WriteJson(JsonWriter writer, [AllowNull] JsonArrayOrSingleObject<T> value, JsonSerializer serializer) {
        if (value?.Count > 1) {
          serializer.Serialize(writer, value, typeof(List<T>));
        } else {
          serializer.Serialize(writer, value.FirstOrDefault(), typeof(T));
        }
      }
    }
  }
}