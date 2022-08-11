using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {
  /// <summary>
  /// Can be used to make a stackable who's stack has an unique key
  /// </summary>
  public interface IKeyedStackable : IStackable {

    /// <summary>
    /// Gets a key to index this stackable's stack with when this is the value.
    /// </summary>
    public string StackKey {
      get;
    }

    /// <summary>
    /// By default, keyed stackableso only stack with IKeyedStackable items with the same key.
    /// </summary>
    bool IStackable.CanStackWith(IStackable other)
      => StackKey == (other as IKeyedStackable)?.StackKey;
  }

  public static class KeyedStackableExtensions {

    /// <summary>
    /// Get a set of stackable items as stacks.
    /// </summary>
    public static IEnumerable<ValueStack<TValue>> ToStacks<TValue>(this IEnumerable<TValue> values) where TValue : IKeyedStackable {
      var types = values.Select(v => new KeyValuePair<string, TValue>(v.StackKey, v)).Bucketize();
      List<ValueStack<TValue>> results = new();
      foreach (var type in types) {
        var current = new ValueStack<TValue>(type.Value.First());
        IEnumerable<TValue> ofThisTypeLeftToAdd = type.Value;
        while (ofThisTypeLeftToAdd.Any()) {
          if (!current.Add(ofThisTypeLeftToAdd, out ofThisTypeLeftToAdd)) {
            results.Add(current);
            current = new(current.Value);
          }
        }
      }

      return results;
    }
  }
}