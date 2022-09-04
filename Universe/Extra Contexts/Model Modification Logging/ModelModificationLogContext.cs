using System;
using Meep.Tech.Collections.Generic;

namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Adds logging to models.
  /// </summary>
  public partial class ModelModificationLogContext : Universe.ExtraContext {

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, Type, Archetype, Exception, bool> OnLoaderArchetypeInitializationComplete
      => (success, type, archetype, error, isSplayed) => {
        if (success && archetype is not null) {
          AddInitialModelComponentToArchetypes(
            archetype.AsSingleItemEnumerable(),
            Components<ModelLog>.Key
          );
        }
      };
  }
}

