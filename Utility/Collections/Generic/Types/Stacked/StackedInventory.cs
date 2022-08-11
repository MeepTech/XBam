using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A collection of stacked values that can be quickly retrieved by key.
  /// There can be multiple stacks per key, but only one stack per numerical index.
  /// </summary>
  public class StackedInventory<TValue> : StackedList<TValue> where TValue : IKeyedStackable {
    Dictionary<string, HashSet<int>> _entryIndexes
      = new();

    /// <summary>
    /// Get all the stacks with the matching key
    /// </summary>
    public IEnumerable<ValueStack<TValue>> this[string index] 
      => _entryIndexes[index].Where(i => _values[i].HasValue).Select(i => _values[i].Value);

    ///<summary><inheritdoc/></summary>
    public override ValueStack<TValue>? this[int index] {
      get => base[index]; 
      set {
        if (base[index].HasValue) {
          _entryIndexes.RemoveFromInnerHashSet(base[index].Value.Value.StackKey, index);
        }
        base[index] = value;
        if (value.HasValue) {
          _entryIndexes.AddToInnerHashSet(value.Value.Value.StackKey, index);
        }
      }
    }

    ///<summary><inheritdoc/></summary>
    public StackedInventory() 
      : base() { }

    ///<summary><inheritdoc/></summary>
    public StackedInventory(IEnumerable<TValue> initialItems) 
      : base(initialItems.ToKeyedStacks()) {}

    ///<summary><inheritdoc/></summary>
    public StackedInventory(IEnumerable<ValueStack<TValue>?> initialStacks) 
      : base(initialStacks) {}

    /// <summary>
    /// Get the indexes/indecies of stacks with the given stack key.
    /// </summary>
    public IEnumerable<int> IndexesOf(string stackKey)
      => _entryIndexes[stackKey];

    ///<summary><inheritdoc/></summary>
    public override void Add(TValue item, int count = 1) {
      int currentStacks = Count;
      base.Add(item, count);
      if (currentStacks != Count && item is not null) {
        _entryIndexes.AddToInnerHashSet(item.StackKey, Count - 1);
      }
    }

    ///<summary><inheritdoc/></summary>
    public override void Add(ValueStack<TValue>? items) {
      int currentStacks = Count;
      base.Add(items);
      if (currentStacks != Count && items.HasValue) {
        _entryIndexes.AddToInnerHashSet(items.Value.Value.StackKey, Count - 1);
      }
    }

    ///<summary><inheritdoc/></summary>
    public override bool Remove(TValue value, int count, out int remainder) {
      if (value is null) {
        return base.Remove(value, count, out remainder);
      }

      int existingStackIndex;
      remainder = count;
      while (remainder > 0 && (existingStackIndex = _values.FindIndex(v => v.HasValue && v.Value.Value.CanStackWith(value))) >= 0) {
        _values[existingStackIndex].Value.Remove(remainder, out remainder);
        if (!_values[existingStackIndex].Value.Any()) {
          _values[existingStackIndex] = null;
          _entryIndexes.RemoveFromInnerHashSet(value.StackKey, existingStackIndex);
        }
      }

      return remainder == 0;
    }
  }
}
