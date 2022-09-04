using System;

namespace Meep.Tech.XBam.Configuration {
  public partial class ModelLog {
    public partial struct Entry {
      /// <summary>
      /// standardized keys for metadata fields
      /// </summary>
      public class MetadataField : Enumeration<MetadataField> {

        public static MetadataField AutoBuilderUsed { get; } = new(nameof(AutoBuilderUsed), typeof(bool));

        public static MetadataField IsACopy { get; } = new(nameof(IsACopy), typeof(bool));

        public static MetadataField WasDeserialized { get; } = new(nameof(WasDeserialized), typeof(bool));

        public static MetadataField FromJson { get; } = new(nameof(FromJson), typeof(bool));

        public static MetadataField Description { get; } = new(nameof(Description), typeof(string));

        public string Key
          => (string)ExternalId;

        public Type ValueType { get; }

        protected MetadataField(string fieldKey, System.Type valueType, Universe universe = null)
          : base(fieldKey, universe) {
          ValueType = valueType;
        }
      }
    }
  }
}
