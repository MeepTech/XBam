using Meep.Tech.XBam.Logging.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.Logging {

  /// <summary>
  /// Log for a model
  /// </summary>
  public partial class ModelLog : IModel.IComponent<ModelLog>, IModel.IComponent.IKnowMyParentModel {
    Universe IComponent.Universe { get; set; }
    IComponent.IFactory IComponent.Factory { get; set; }
    IModel.IReadableComponentStorage IModel.IComponent.IKnowMyParentModel.Container { get; set; }
    List<Entry> _log = new();

    /// <summary>
    /// The log entries
    /// </summary>
    public IReadOnlyList<Entry> Log {
      get => _log;
      private set => _log = value.ToList();
    }

    ModelLog() {}

    /// <summary>
    /// Append a new log entry.
    /// </summary>
    public Entry AppendActionToLog(Entry.ActionType type, string originalModelJson = "null", IEnumerable<KeyValuePair<string, object>>? providedParameters = null, IReadOnlyDictionary<string, object>? metadata = null) {
      var entry = new Entry(type, originalModelJson, this.GetParent(), providedParameters, metadata);
      _log.Add(entry);

      if (this.GetContainer().Universe.Loader.IsFinished && this.GetContainer().Universe.TryToGetExtraContext<ConsoleProgressLogger>(out var logger)) {
        if (logger!.VerboseModeForNonErrors) {
          logger.WriteMessage(entry.ToString(), "Model Edits");
        }
      }

      return entry;
    }
  }
}