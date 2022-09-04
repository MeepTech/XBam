using Meep.Tech.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Meep.Tech.XBam.Configuration {
  public static class LoaderConfigurationExtensions {

    /// <summary>
    /// Update json serialization settings to include xbam settings.
    /// </summary>
    public static JsonSerializerSettings UpdateJsonSerializationSettings(this JsonSerializerSettings @default, Universe universe = null, bool overrideDefaultResolver = true, IEnumerable<JsonConverter> extraConverters = null) {
      universe ??= Archetypes.DefaultUniverse;
      @default.ContractResolver = overrideDefaultResolver
        ? @default.ContractResolver ?? new Model.Serializer.DefaultContractResolver(universe)
        : new Model.Serializer.DefaultContractResolver(universe);

      universe.ModelSerializer.JsonSettings.Converters
        .ConcatIfNotNull(extraConverters)
          .ForEach(@default.Converters.Add);

      // only added if the contract resolver isn't there:
      if(@default.ContractResolver is not Model.Serializer.DefaultContractResolver) {
        @default.Converters.Add(new Model.JsonConverter());
      }

      return @default;
    }
  }
}