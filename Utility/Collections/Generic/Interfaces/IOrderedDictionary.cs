using System.Collections.Generic;

namespace Meep.Tech.Collections.Generic {
  /// <summary>
  /// A collection of items, each indexed by a key and in list order by an index.
  /// Read + Writeable
  /// </summary>
  public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyOrderedDictionary<TKey, TValue> {

    /// <summary>
    /// Sets the value at the index specified, maintaining the same key that's currently pointing at this index.
    /// </summary>
    void SetByIndex(int index, TValue value);

    /// <summary>
    /// set a value by it's key, adding a new entry if the key isn't present at the end of the list.
    /// </summary>
    void SetByKey(TKey key, TValue value);

    /// <summary>
    /// Insert an item into the collection at an existing index.
    /// </summary>
    void InsertByIndex(int index, TKey key, TValue value);

    /// <summary>
    /// Remove the value at the given index.
    /// </summary>
    void RemoveAt(int index);

    ///<summary>
    /// Remove using the key.
    /// This can help as an alternative to Remove if the key is an int
    /// </summary>
    bool RemoveByKey(TKey key);
  }
}