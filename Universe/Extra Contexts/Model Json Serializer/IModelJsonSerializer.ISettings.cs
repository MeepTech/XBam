using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Meep.Tech.XBam.Json.Configuration {
  public partial interface IModelJsonSerializer {

    /// <summary>
    /// Settings for the Model Serializer
    /// </summary>
    public interface ISettings {

      /// <summary>
      /// Configuration for the default contract resolver.
      /// </summary>
      Func<Universe, ModelJsonSerializerContext.DefaultContractResolver> ConfigureJsonContractResolver
        { get; set; }

      /// <summary>
      /// Helper function to set the default json serializer settings for models.
      /// </summary>
      Action<ModelJsonSerializerContext.Settings, JsonSerializerSettings> ConfigureJsonSerializerSettings 
        { get; set; }

      /// <summary>
      /// The default json converters to include
      /// </summary>
      Func<Universe, IEnumerable<JsonConverter>> GetDefaultJsonCoverters 
        { get; set; }

      /// <summary>
      /// If true, properies need to opt out to avoid being serialized into json using JsonIgnore. Even private properties.
      /// </summary>
      bool PropertiesMustOptOutForJsonSerialization 
        { get; set; }
    }
  }
}