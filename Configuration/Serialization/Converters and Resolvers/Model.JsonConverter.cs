using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Meep.Tech.XBam {

  public partial class Model {

    /// <summary>
    /// Defalt converter for any type of model.
    /// This isn't added to the settings by default because the contract resolver handles it.
    /// </summary>
    public class JsonConverter : JsonConverter<IModel> {

      ///<summary><inheritdoc/></summary>
      public override IModel ReadJson(
        JsonReader reader,
        Type objectType,
        [AllowNull] IModel existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
      ) => IModel.FromJson(serializer.Deserialize<JObject>(reader), objectType, existingValue?.Universe);

      ///<summary><inheritdoc/></summary>
      public override void WriteJson(
        JsonWriter writer, 
        [AllowNull] IModel value,
        JsonSerializer serializer
      ) => serializer.Serialize(writer, value.ToJson());
    }
  }
}