using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using KellermanSoftware.CompareNetObjects;
using System.Linq;

namespace Meep.Tech.XBam {

  public partial class Model {
    public partial class Serializer {

      /// <summary>
      /// Settings for the Model Serializer
      /// </summary>
      public class Settings {
        internal Universe _universe;

        /// <summary>
        /// Helper function to set the default json serializer settings for models.
        /// </summary>
        public Func<DefaultContractResolver, IEnumerable<Newtonsoft.Json.JsonConverter>, JsonSerializerSettings> ConstructJsonSerializerSettings {
          get;
          set;
        } = DefaultJsonSerializerSettingsConfigurationLogic;

        /// <summary>
        /// Helper function to configure the json serialization settings for models after it's constructed.
        /// </summary>
        public Action<JsonSerializerSettings> ConfigureJsonSerializerSettings {
          get;
          set;
        } = e => { };

        /// <summary>
        /// Compiled model serializer from the settings config function
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings {
          get => _modelJsonSerializerSettings
            ??= BuildJsonSerializationSettings();
        } JsonSerializerSettings _modelJsonSerializerSettings;

        /// <summary>
        /// The default json converters to include
        /// </summary>
        public IEnumerable<Newtonsoft.Json.JsonConverter> DefaultJsonCoverters {
          get;
          set;
        } = new Newtonsoft.Json.JsonConverter[] {
          new Enumeration.JsonConverter()
        };

        /// <summary>
        /// If true, properies need to opt out to avoid being serialized into json using JsonIgnore. Even private properties.
        /// </summary>
        public bool PropertiesMustOptOutForJsonSerialization {
          get;
          set;
        } = true;

        /// <summary>
        /// The default config used to compare model objects
        /// </summary>
        public ComparisonConfig DefaultComparisonConfig {
          get;
          set;
        } = new ComparisonConfig {
          AttributesToIgnore = new List<Type> {
            typeof(ModelComponentsProperty)
          },
          IgnoreObjectTypes = true
#if DEBUG
          ,
          DifferenceCallback = x => {
            if (System.Diagnostics.Debugger.IsAttached) {
              System.Diagnostics.Debugger.Break();
            }
          }
#endif
        };

        /// <summary>
        /// The default way models are copied
        /// </summary>
        public Func<IModel, IModel> DefaultCopyMethod {
          get;
          set;
        } = model => IModel.FromJson(model.ToJson()); 

        /// <summary>
        /// Can be used to build and configure copies of the built in json serializer settings.
        /// </summary>
        public JsonSerializerSettings BuildJsonSerializationSettings(Action<JsonSerializerSettings> configure = null, Func<DefaultContractResolver, IEnumerable<Newtonsoft.Json.JsonConverter>, JsonSerializerSettings> construct = null) {
          var settings = (construct ?? ConstructJsonSerializerSettings).Invoke(
            new DefaultContractResolver(_universe),
            DefaultJsonCoverters
          );
          (configure ?? ConfigureJsonSerializerSettings).Invoke(settings);

          return settings;
        }

        /// <summary>
        /// The default logic for ConfigureJsonSerializerSettings
        /// </summary>
        public static JsonSerializerSettings DefaultJsonSerializerSettingsConfigurationLogic(DefaultContractResolver contractResolver, IEnumerable<Newtonsoft.Json.JsonConverter> jsonConverters)
          => new() {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented,
            Converters = jsonConverters.ToList()
#if DEBUG
            ,
            Error = (sender, args) => {
              if (System.Diagnostics.Debugger.IsAttached) {
                System.Diagnostics.Debugger.Break();
              }
            }
#endif
          };
      }
    }
  }
}