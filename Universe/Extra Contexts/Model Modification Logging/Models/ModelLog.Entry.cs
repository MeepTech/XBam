using Meep.Tech.XBam.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.Configuration {
  public partial class ModelLog {

    /// <summary>
    /// A log entry
    /// </summary>
    public partial struct Entry {

      public ActionType Type { get; }
      public string OriginalModelJson { get; }
      public IEnumerable<KeyValuePair<string, object>>? ProvidedParameters { get; }
      public string ModifiedModelJson { get; }
      public IReadOnlyDictionary<string, object>? MetaData { get; }
      public DateTime TimeStamp { get; }

      internal Entry(ActionType type, string originalModelJson, IModel currentModel, IEnumerable<KeyValuePair<string, object>>? providedParameters, IReadOnlyDictionary<string, object>? metadata) {
        TimeStamp = DateTime.Now;
        Type = type;
        OriginalModelJson = originalModelJson;
        ProvidedParameters = providedParameters;
        ModifiedModelJson = currentModel.ToJson().ToString();
        MetaData = metadata;
      }

      public override string ToString() {
        var firstLine = "\n+" + Type.ExternalId.ToString() + " @ "  + TimeStamp.ToString("G");
        var description = "\n";

        if (ProvidedParameters?.Any() ?? false) {
          description 
            += "\tParameters:\n\t\t" 
              + string.Join(
                ",\n\t\t", 
                ProvidedParameters.Select(
                  p => $"{p.Key}:\t {_tryToGetStringValue(p.Value).AddTabsToEachNewline(2)}"
                )
              );

          description += "\n";
        }

        description += "\tOriginal:\t" + OriginalModelJson.AddTabsToEachNewline();
        description += "\n";
        description += "\tModified:\t" + ModifiedModelJson.AddTabsToEachNewline();
        description += "\n";

        if (MetaData?.Any() ?? false) {
          description 
            += "\tMetadata:\n\t\t" 
              + string.Join(
                ",\n\t\t", 
                MetaData.Select(
                  p => $"{p.Key}:\t {_tryToGetStringValue(p.Value).AddTabsToEachNewline(2)}\n"
                )
              );
        }

        return firstLine + description;
      }

      static string _tryToGetStringValue(object value) {
        try {
          return JObject.FromObject(value).ToString();
        }
        catch {
          return value?.ToString() ?? "null";
        }
      }

    }
  }
}
