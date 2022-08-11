using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A stack of values
  /// </summary>
  public struct ValueStack<TValue> : IEnumerable<TValue> where TValue : IStackable {

    /// <summary>
    /// The number of copies of the valie in the stack
    /// </summary>
    public int Count {
      get;
      private set;
    }

    /// <summary>
    /// The value of all items in the stack.
    /// </summary>
    public TValue Value {
      get;
    }

    /// <summary>
    /// If this stack is full
    /// </summary>
    public bool IsFull
      => Value.MaxQuantityPerStack.HasValue && Count >= Value.MaxQuantityPerStack;

    /// <summary>
    /// Make a new stack of values.
    /// </summary>
    public ValueStack(TValue value, int count = 0) {
      if (value.MaxQuantityPerStack.HasValue && count > value.MaxQuantityPerStack.Value) {
        throw new ArgumentException($"cannot add more than {value.MaxQuantityPerStack.Value} items to a stack of items of type: {value}. Tried to add: {count}");
      }
      Value = value;
      Count = count;
    }

    /// <summary>
    /// Make a new stack of values. 
    /// If there's anything unique among these values, the unique attributes will be destroyed and replaced with the attributes of the first item in the collection!
    /// </summary>
    public ValueStack(IEnumerable<TValue> values) {
      if (values.First().MaxQuantityPerStack.HasValue && values.Count() > values.First().MaxQuantityPerStack.Value) {
        throw new ArgumentException($"cannot add more than {values.First().MaxQuantityPerStack.Value} items to a stack of items of type: {values.First()}. Tried to add: {values.Count()}");
      }
      Value = values.First();
      Count = values.Count();
    }

    /// <summary>
    /// Add the count to the stack. This takes into account IStackable.MaxQuantityPerStack
    /// </summary>
    /// <param name="count">How many items to add</param>
    /// <param name="remainder">The count of items that couldn't be added</param>
    /// <returns>true if all values were added, false if some couldn't be added</returns>
    public bool Add(int count, out int remainder) {
      int newCount = count + Count;
      if (Value.MaxQuantityPerStack.HasValue && newCount > Value.MaxQuantityPerStack) {
        remainder = newCount - Value.MaxQuantityPerStack.Value;
        newCount = Value.MaxQuantityPerStack.Value;
      } else {
        remainder = 0;
      }

      Count = newCount;
      return remainder == 0;
    }

    /// <summary>
    /// Remove the count from the stack.
    /// </summary>
    /// <param name="count">How many items to remove</param>
    /// <param name="remainder">The count of items that couldn't be removed (because none were left to remove)</param>
    /// <returns>true if all values were removed, false if some not be</returns>
    public bool Remove(int count, out int remainder) {
      int newCount = Count - count;
      if (newCount < 0) {
        remainder = count + newCount;
        newCount = 0;
      } else {
        remainder = 0;
      }

      Count = newCount;
      return remainder == 0;
    }

    /// <summary>
    /// Add the values to the stack. This takes into account IStackable.MaxQuantityPerStack
    /// If there's anything unique among these values, the unique attributes will be destroyed and replaced with the attributes of the first item in the existing stack!
    /// </summary>
    /// <param name="values">The values to add</param>
    /// <param name="remainder">The count of items that couldn't be added</param>
    /// <returns>true if all values were added, some if count not be</returns>
    public bool Add(IEnumerable<TValue> values, out IEnumerable<TValue> remainder) {
      if (!Add(values.Count(), out int remainingCount)) {
        remainder = values.Skip(values.Count() - remainingCount);
        return false;
      } else {
        remainder = Enumerable.Empty<TValue>();
        return true;
      }
    }

    ///<summary><inheritdoc/></summary> 
    public IEnumerator<TValue> GetEnumerator() {
      for(int i = 0; i < Count; i++) {
        yield return Value;
      }
    }

    /// <summary>
    /// Clear this stack of items.
    /// </summary>
    internal void Clear()
      => Count = 0;

    IEnumerator IEnumerable.GetEnumerator()
      => GetEnumerator();
  }
}
