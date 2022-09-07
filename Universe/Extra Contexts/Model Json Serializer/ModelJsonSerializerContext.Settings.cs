using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Reflection;

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
        new Enumerations.JsonObjectConverter(universe),
        new Archetypes.JsonStringConverter(universe)
      };

      /// <summary>
      /// Executed once for each json property created when scanning models with the default contract resolver.
      /// </summary>
      /// <remarks>
      /// Use += to add functionality
      /// </remarks>
      public virtual Action<MemberInfo, JsonProperty>? OnLoaderModelJsonPropertyCreationComplete 
        { get; set; } = (_,_) => { };

      /// <summary>
      /// If true, properies need to opt out to avoid being serialized into json using JsonIgnore. Even private properties.
      /// </summary>
      public bool PropertiesMustOptOutForJsonSerialization {
        get;
        init;
      } = true;

      /// <summary>
      /// If true, properies need to opt out to avoid being serialized into json using JsonIgnore. Even private properties.
      /// </summary>
      public bool IncludeUniverseKey {
        get;
        init;
      } = false;
    }
  }
}