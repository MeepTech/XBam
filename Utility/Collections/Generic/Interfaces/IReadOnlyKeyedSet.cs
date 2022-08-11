using System.Collections.Generic;

namespace Meep.Tech.Collections.Generic {
  /// <summary>
  /// A Keyed Set, but readonly.
  /// </summary>
  public interface IReadOnlyKeyedSet<TKey, TItem>
    : IReadOnlyCollection<TItem>, IReadOnlyList<TItem> {}
}