using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {
  /// <summary>
  /// A list of indexed stacks.
  /// </summary>
  public class StackedCollection<TValue> : IStackedCollection<TValue> where TValue : IStackable {
    internal List<ValueStack<TValue>?> _values
      = new();

    ///<summary><inheritdoc/></summary>
    public int Count 
      => _values.Count;

    ///<summary><inheritdoc/></summary>
    public bool IsReadOnly 
      => (_values as IList).IsReadOnly;

    /// <summary>
    /// Make a new stacked collection.
    /// </summary>
    public StackedCollection() {}

    /// <summary>
    /// Make a new stacked collection.
    /// </summary>
    public StackedCollection(IEnumerable<TValue> initialItems) : this() {
      initialItems.ForEach(v => Add(v));
    }

    /// <summary>
    /// Make a new stacked collection.
    /// </summary>
    public StackedCollection(IEnumerable<ValueStack<TValue>?> initialStacks) : this() {
      _values = initialStacks is List<ValueStack<TValue>?> list ? list : initialStacks.ToList();
    }

    /// <summary>
    /// Adds the items from the provided stack to the current collection
    /// </summary>
    public virtual void Add(TValue item, int count = 1)
      => _add(new(item, count));

    /// <summary>
    /// Adds the items from the provided stack to the current collection
    /// </summary>
    public virtual void Add(ValueStack<TValue>? items)
      => _add(items);

    /// <summary>
    /// Adds the items from the provided stack to the current collection
    /// </summary>
    internal void _add(ValueStack<TValue>? items) {
      if (items is null) {
        _values.Add(null);
        return;
      }

      int existingStackIndex = _values.FindIndex(v => v.HasValue && !v.Value.IsFull && v.Value.Value.CanStackWith(items.Value.Value));
      if (existingStackIndex > 0) {
        if (!_values[existingStackIndex].Value.Add(items.Value.Count, out var remainder)) {
          Add(items.Value.Value, remainder);
        }
      } else {
        _values.Add(new(items));
      }
    }

    ///<summary><inheritdoc/></summary>
    public virtual bool Remove(TValue value, int count, out int remainder) {
      int existingStackIndex;
      remainder = count;
      while (remainder > 0 && (existingStackIndex = _values.FindIndex(v => v.HasValue && v.Value.Value.CanStackWith(value))) >= 0) {
        _values[existingStackIndex].Value.Remove(remainder, out remainder);
        if (!_values[existingStackIndex].Value.Any()) {
          _values[existingStackIndex] = null;
        }
      }

      return remainder == 0;
    }

    ///<summary><inheritdoc/></summary>
    public void Clear()
      => _values.Clear();

    ///<summary><inheritdoc/></summary>
    public bool Contains(TValue item)
      => _values.Any(e => e.HasValue && item.CanStackWith(e.Value.Value));

    ///<summary><inheritdoc/></summary>
    public void CopyTo(ValueStack<TValue>?[] array, int arrayIndex)
      => _values.CopyTo(array, arrayIndex);

    ///<summary><inheritdoc/></summary>
    public IEnumerator<ValueStack<TValue>?> GetEnumerator()
      => _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
      => GetEnumerator();
  }
}
