using System.Collections.Generic;

namespace Meep.Tech.XBam {
  
  /// <summary>
  /// This can produce model builders.
  /// Used internally.
  /// </summary>
  public interface IBuilderSource {

    /// <summary>
    /// Can be used to make a new builder.
    /// </summary>
    internal IBuilder Build(IEnumerable<KeyValuePair<string, object>> initialParams = null);
  }
}
