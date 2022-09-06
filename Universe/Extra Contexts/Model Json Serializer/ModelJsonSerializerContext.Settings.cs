using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Meep.Tech.XBam.Json.Configuration {

  public partial class ModelJsonSerializerContext {

    /// <summary>
    /// Settings for the Model Serializer
    /// </summary>
    public new class Settings : Universe.ExtraContext.Settings, IModelJsonSerializer.ISettings {

      /// <summary>
      /// Helper function to set the default json serializer settings for models.
      /// </summary>
      public Action<Settings, JsonSerializerSettings> ConfigureJsonSerializerSettings {
        get;
        set;
      } = (options, settings) => {
        settings.ContractResolver = options.ConfigureJsonContractResolver(options.Universe);
        settings.Formatting = Formatting.Indented;
        settings.Converters = options.GetDefaultJsonCoverters(options.Universe).ToList();
        settings.Context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.Other, "json");
      };

      /// <summary>
      /// Configuration for the default contract resolver.
      /// </summary>
      public Func<Universe, DefaultContractResolver> ConfigureJsonContractResolver {
        get;
        set;
      } = universe => new DefaultContractResolver(universe);

      /// <summary>
      /// The default json converters to include
      /// </summary>
      public Func<Universe, IEnumerable<Newtonsoft.Json.JsonConverter>> GetDefaultJsonCoverters {
        get;
        set;
      } = universe => new Newtonsoft.Json.JsonConverter[] {
        new Enumeration.JsonConverter(universe)
      };

      /// <summary>
      /// If true, properies need to opt out to avoid being serialized into json using JsonIgnore. Even private properties.
      /// </summary>
      public bool PropertiesMustOptOutForJsonSerialization {
        get;
        set;
      } = true;
    }
  }
}