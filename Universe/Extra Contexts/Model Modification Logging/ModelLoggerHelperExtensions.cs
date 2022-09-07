using System.Collections.Generic;

namespace Meep.Tech.XBam.Logging {
  public static class ModelLoggerHelperExtensions {

    /// <summary>
    /// Check if this model can log it's progress.
    /// </summary>
    public static bool CanLog(this IModel model, out ModelLog loggingComponent) {
      if (model is IReadableComponentStorage storage
        && storage.TryToGetComponent<ModelLog>(out loggingComponent)
      ) {
        return true;
      }

      loggingComponent = null;
      return false;
    }

    /// <summary>
    /// Log something for this model.
    /// Throws if there's no logger.
    /// </summary>
    public static void Log(this IReadableComponentStorage model, ModelLog.Entry.ActionType type, string originalModelJson, IEnumerable<KeyValuePair<string, object>>? providedParameters = null, IReadOnlyDictionary<string, object>? metadata = null) {
      var loggingComponent = model.GetComponent<ModelLog>();
      loggingComponent.AppendActionToLog(
        type,
        originalModelJson,
        providedParameters,
        metadata
      );
    }

    /// <summary>
    /// Try to log something for this model (if the logger exists)
    /// </summary>
    public static bool TryToLog(this IModel model, ModelLog.Entry.ActionType type, string originalModelJson, IEnumerable<KeyValuePair<string, object>>? providedParameters = null, IReadOnlyDictionary<string, object>? metadata = null) {
      if (CanLog(model, out var loggingComponent)) {
        loggingComponent.AppendActionToLog(
          type,
          originalModelJson,
          providedParameters,
          metadata
        );

        return true;
      }

      return false;
    }
  }
}

