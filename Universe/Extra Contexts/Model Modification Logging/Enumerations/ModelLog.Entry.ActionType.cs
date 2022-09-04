namespace Meep.Tech.XBam.Configuration {
  public partial class ModelLog {

    public partial struct Entry {
      /// <summary>
      /// Action types for log entries
      /// </summary>
      public class ActionType : Enumeration<ActionType> {

        public static ActionType Built { get; } = new(nameof(Built), "Default");

        public static ActionType Copied { get; } = new(nameof(Copied), "Default");

        public static ActionType Deserialized { get; } = new(nameof(Deserialized), "Default");

        public static ActionType Serialized { get; } = new(nameof(Serialized), "Default");

        protected ActionType(string key, string prefix, Universe universe = null)
          : base(prefix + "." + key, universe) { }
      }
    }
  }
}