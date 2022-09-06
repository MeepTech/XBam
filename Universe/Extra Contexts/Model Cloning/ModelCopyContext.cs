using Meep.Tech.XBam.Cloning.Configuration;
using Meep.Tech.XBam.Json;
using Meep.Tech.XBam.Logging;
using System;
using System.Collections.Generic;

namespace Meep.Tech.XBam.Cloning {
  namespace Configuration {

    /// <summary>
    /// Used to set up debugging and progress loading bars for xbam.
    /// </summary>
    public class ModelCopyContext : Universe.ExtraContext, IModelCopier {
      IModelCopier.ISettings? IModelCopier.Options
          => Options;

      /// <summary>
      /// Settings for the ModelCopyContext
      /// </summary>
      public new class Settings : Universe.ExtraContext.Settings, IModelCopier.ISettings {

        /// <summary>
        /// The default way models are copied
        /// </summary>
        public Func<IModel, IModel> DefaultCopyMethod {
          get;
          set;
        } = model => Deserialize.ToModel(model.ToJson());
      }

      ///<summary><inheritdoc/></summary>
      public new Settings Options
        => (Settings)base.Options!;

      ///<summary><inheritdoc/></summary>
      public Func<IModel, IModel> CopyMethod { get; private set; }

      /// <summary>
      /// The model copy context.
      /// </summary>
      public ModelCopyContext(Settings? options = null)
        : base(options ?? new Settings()) { }

      ///<summary><inheritdoc/></summary>
      protected internal override Action OnLoaderFinalizeStart
        => () => CopyMethod = Options.DefaultCopyMethod
          ?? throw new ArgumentNullException(nameof(Settings.DefaultCopyMethod));

    }
  }

  public static class ModelCloningExtensions {

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// </summary>
    public static IModel Copy(this IModel @this) {
      string originalJson = null;
      if (@this.CanLog(out var logger)) {
        originalJson = @this.ToJson().ToString();
        logger.AppendActionToLog(
          ModelLog.Entry.ActionType.Copied,
          originalJson,
          null,
          new Dictionary<string, object> { { ModelLog.Entry.MetadataField.IsACopy.Key, false } }
        );
      }

      var copy = @this.Universe.GetExtraContext<ModelCopyContext>().CopyMethod(@this);

      if (logger is not null) {
        ((IReadableComponentStorage)copy).Log(
          ModelLog.Entry.ActionType.Copied,
          "null",
          new KeyValuePair<string, object>[] { new("original", originalJson) },
          new Dictionary<string, object> { { ModelLog.Entry.MetadataField.IsACopy.Key, true } }
        );
      }

      return copy;
    }

    /// <summary>
    /// Copy the unique model by serializing and deserializing it.
    /// </summary>
    public static IUnique Copy(this IUnique @this, bool newUniqueId = true) {
      IUnique copy = @this.Copy();
      if (newUniqueId) {
        copy.InitializeId();
      }

      return copy;
    }
  }
}
