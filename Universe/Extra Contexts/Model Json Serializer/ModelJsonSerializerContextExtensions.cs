using Meep.Tech.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.Json {

  namespace Configuration {
    /// <summary>
    /// Extensions for ModelJsonSerializerContext
    /// </summary>
    public static class ModelJsonSerializerContextExtensions {

      /// <summary>
      /// Update json serialization settings to include xbam settings.
      /// </summary>
      public static JsonSerializerSettings UpdateJsonSerializationSettings(this JsonSerializerSettings @default, Universe? universe = null, bool overrideDefaultResolver = true, IEnumerable<JsonConverter>? extraConverters = null, IModelJsonSerializer? extraContext = null) {
        universe ??= Universe.Default;
        extraContext ??= universe.TryToGetExtraContext<IModelJsonSerializer>();

        @default.ContractResolver = overrideDefaultResolver
          ? @default.ContractResolver ?? (extraContext?.Options as ModelJsonSerializerContext.Settings)?.ConfigureJsonContractResolver(universe) ?? new ModelJsonSerializerContext.DefaultContractResolver(universe)
          : (extraContext?.Options as ModelJsonSerializerContext.Settings)?.ConfigureJsonContractResolver(universe) ?? new ModelJsonSerializerContext.DefaultContractResolver(universe);

        (extraContext?.JsonSettings.Converters ?? Enumerable.Empty<JsonConverter>())
          .ConcatIfNotNull(extraConverters)
            .ForEach(@default.Converters.Add);

        // only added if the contract resolver isn't there:
        /*if (@default.ContractResolver is not ModelJsonSerializerContext.DefaultContractResolver) {
          @default.Converters.Add(new Model.JsonConverter());
        }*/

        return @default;
      }
    }
  }
}