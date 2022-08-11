using System.Collections.Generic;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A type/item that can be placed in a stack of similar items within a StackedCollection.
  /// </summary>
  public interface IStackable {

    /// <summary>
    /// The max value allowed of this item in a single stack.
    /// null is infinite.
    /// </summary>
    public int? MaxQuantityPerStack {
      get;
    }

    /// <summary>
    /// Used to determine if this item can stack with another item.
    /// </summary>
    public bool CanStackWith(IStackable other);
  }

  public static class StackableExtensions {

    /// <summary>
    /// Get a set of stackable items as stacks.
    /// </summary>
    public static IEnumerable<ValueStack<TValue>?> ToKeyedStacks<TValue>(this IEnumerable<TValue> values) where TValue : IStackable
      => new StackedCollection<TValue>(values);
  }
}