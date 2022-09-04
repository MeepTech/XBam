using Meep.Tech.XBam.Utility;
using Newtonsoft.Json;
using System;

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
      /*
      /// <summary>
      /// The key used for the field containing the data for the type of component
      /// </summary>
      public const string ComponentKeyPropertyName = "__key_";

      /// <summary>
      /// The key used for the field containing the value collection for a component if it serializes to a colleciton by default
      /// </summary>
      public const string ComponentValueCollectionPropertyName = "__values_";*/

      /// <summary>
      /// The universe this is for
      /// </summary>
      public Universe Universe {
        get;
      }

      /// <summary>
      /// The serializer options
      /// </summary>
      public Settings Options {
        get;
      }

      /// <summary>
      /// Compiled model serializer from the settings config function
      /// </summary>
      public JsonSerializerSettings JsonSettings {
        get => _modelJsonSerializerSettings
          ??= new JsonSerializerSettings()
            .Modify(s => Options.ConfigureJsonSerializerSettings(Options, s));
      } JsonSerializerSettings _modelJsonSerializerSettings;

      /// <summary>
      /// Compiled model serializer from the settings
      /// </summary>
      public JsonSerializer JsonSerializer {get;}

      /// <summary>
      /// The default way models are copied
      /// </summary>
      public Func<IModel, IModel> ModelCopyMethod {
        get;
        internal set;
      }

      /// <summary>
      /// Make a new serializer for a universe
      /// </summary>
      internal Serializer(Settings options, Universe universe) {
        Options = options ?? new Settings();
        Options.Universe = universe;
        Universe = universe;
        ModelCopyMethod = options.ModelCopyMethod;
        JsonSerializer = JsonSerializer.Create(JsonSettings);
      }
    }
  }
}