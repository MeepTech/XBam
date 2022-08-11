using System.Collections.Generic;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A collection of stacked items.
  /// </summary>
  public interface IStackedCollection<TValue> 
    : ICollection<ValueStack<TValue>?> where TValue : IStackable 
  {
    /// <summary>
    /// Check if the collection contains a stack of the item.
    /// </summary>
    bool Contains(TValue item);

    bool ICollection<ValueStack<TValue>?>.Contains(ValueStack<TValue>? item)
      => Contains(item.Value);

    /// <summary>
    /// Remove the given number of the given item from this collection.
    /// </summary>
    /// <param name="remainder">any quantity not able to be removed.</param>
    bool Remove(TValue? value, int count, out int remainder);

    bool ICollection<ValueStack<TValue>?>.Remove(ValueStack<TValue>? item)
      => Remove(item.HasValue ? (TValue?)item.Value.Value : default, item.Value.Count, out _);
  }
}
