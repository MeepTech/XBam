using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using Meep.Tech.XBam.Json;

namespace Meep.Tech.XBam {
  /*
  public partial class Model {

    /// <summary>
    /// Defalt converter for any type of model.
    /// This isn't added to the settings by default because the contract resolver handles it.
    /// </summary>
    public class JsonConverter : JsonConverter<IModel> {

      public Universe Universe {
        get;
      }

      public JsonConverter(Universe universe) {
        Universe = universe;
      }

      ///<summary><inheritdoc/></summary>
      public override IModel ReadJson(
        JsonReader reader,
        Type objectType,
        [AllowNull] IModel existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
      ) => Deserialize.ToModel(serializer.Deserialize<JObject>(reader), objectType, Universe, serializer);

      ///<summary><inheritdoc/></summary>
      public override void WriteJson(
        JsonWriter writer, 
        [AllowNull] IModel value,
        JsonSerializer serializer
      ) => serializer.Serialize(writer, value.ToJson());
    }
  }*/
}