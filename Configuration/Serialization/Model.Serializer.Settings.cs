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

        /// <summary>
        /// The Universe these settings belong to.
        /// </summary>
        public Universe Universe {
          get;
          internal set;
        }

        /// <summary>
        /// Helper function to set the default json serializer settings for models.
        /// </summary>
        public Action<Settings, JsonSerializerSettings> ConfigureJsonSerializerSettings {
          get;
          set;
        } = (options, settings) => {
          settings.ContractResolver = options.ConfigureJsonContractResolver(options.Universe);
          settings.Formatting = Formatting.Indented;
          settings.Converters = options.DefaultJsonCoverters.ToList();
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
        public Func<IModel, IModel> ModelCopyMethod {
          get;
          set;
        } = model => IModel.FromJson(model.ToJson());

        public Settings(Universe universe = null) {
          Universe = universe ?? Universe.Default;
        }
      }
    }
  }
}