using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A list of indexed stacks.
  /// </summary>
  public class StackedList<TValue> : StackedCollection<TValue>, IList<ValueStack<TValue>?> where TValue : IStackable {

    /// <summary>
    /// All the values within the stacks, splayed out and ordered by the stack they were in
    /// </summary>
    public IEnumerable<TValue> Values
      => _values.Where(c => c.HasValue).SelectMany(c => c.Value);

    ///<summary><inheritdoc/></summary>
    public virtual ValueStack<TValue>? this[int index] { 
      get => _values[index];
      set => _values[index] = value; 
    }

    ///<summary><inheritdoc/></summary>
    public StackedList() 
      : base() {}

    ///<summary><inheritdoc/></summary>
    public StackedList(IEnumerable<TValue> initialItems) 
      : base(initialItems) {}

    ///<summary><inheritdoc/></summary>
    public StackedList(IEnumerable<ValueStack<TValue>?> initialStacks) 
      : base(initialStacks) {}

    ///<summary><inheritdoc/></summary>
    public IEnumerable<int> IndexesOf(TValue item)
      => _values.Select((v, i) => (v, i))
        .Where(e => e.v.HasValue ? e.v.Value.Value.CanStackWith(item is not null ? item : null) : item is null)
        .Select(e => e.i);

    ///<summary><inheritdoc/></summary>
    int IList<ValueStack<TValue>?>.IndexOf(ValueStack<TValue>? item)
      => _values.FindIndex(v => (v.HasValue ? v.Value.Value.CanStackWith(item.HasValue ? item.Value.Value : null) : !item.HasValue) && (v?.Count == item?.Count));

    ///<summary><inheritdoc/></summary>
    public int FirstIndexOf(ValueStack<TValue>? item)
      => _values.FindIndex(v => (v.HasValue ? v.Value.Value.CanStackWith(item.HasValue ? item.Value.Value : null) : !item.HasValue));

    ///<summary><inheritdoc/></summary>
    public void Insert(int index, ValueStack<TValue>? item)
      => _values.Insert(index, item);

    ///<summary><inheritdoc/></summary>
    public void RemoveAt(int index)
      => _values.RemoveAt(index);
  }
}
