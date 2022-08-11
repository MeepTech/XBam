using Newtonsoft.Json;

namespace Meep.Tech.XBam {

  public partial class Model {

    /// <summary>
    /// Logic and Settings Used To Serialize Models
    /// </summary>
    public partial class Serializer {

      /// <summary>
      /// The key used for the field containing the type data for an enum
      /// </summary>
      public const string EnumTypePropertyName = "__type_";

      /// <summary>
      /// The key used for the field containing the data for the type of component
      /// </summary>
      public const string ComponentKeyPropertyName = "__key_";

      /// <summary>
      /// The key used for the field containing the value collection for a component if it serializes to a colleciton by default
      /// </summary>
      public const string ComponentValueCollectionPropertyName = "__values_";

      /// <summary>
      /// The serializer options
      /// </summary>
      public Settings Options {
        get;
      }

      /// <summary>
      /// Compiled model serializer from the settings
      /// </summary>
      public JsonSerializer JsonSerializer {
        get => _modelJsonSerializer ??= JsonSerializer
          .Create(Options.JsonSerializerSettings);
      } JsonSerializer _modelJsonSerializer;

      /// <summary>
      /// Make a new serializer for a universe
      /// </summary>
      internal Serializer(Settings options, Universe universe) {
        Options = options ?? new Settings();
        Options._universe = universe;
      }
    }
  }
}