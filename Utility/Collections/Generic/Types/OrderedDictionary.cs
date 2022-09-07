using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// An ordered dictionary.
  /// </summary>
  public class OrderedDictionary<TKey, TValue> 
    : IOrderedDictionary<TKey, TValue>,
      IDictionary<TKey, TValue>,
      ICollection<KeyValuePair<TKey, TValue>>
  {

    #region Fields and Parameters

    KeyedSet<TKey, KeyValuePair<TKey, TValue>> _collection
      = new(pair => pair.Key);

    #region Public Readonly

    ///<summary><inheritdoc/></summary>
    public ICollection<TKey> Keys
      => _collection.Select(e => e.Key).ToArray();

    ///<summary><inheritdoc/></summary>
    public ICollection<TValue> Values
      => _collection.Select(e => e.Value).ToArray();

    ///<summary><inheritdoc/></summary>
    public int Count
      => _collection.Count;

    ///<summary><inheritdoc/></summary>
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
      => Keys;

    ///<summary><inheritdoc/></summary>
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
      => Values;

    /// <summary>
    /// If this has been marked readonly
    /// </summary>
    public bool IsReadOnly {
      get;
      private set;
    }

    #endregion

    #endregion

    #region Constructors

    /// <summary>
    /// Make an ordered dictionary.
    /// </summary>
    public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> orderedValues = null) {
      orderedValues?.ForEach(e => Add(e.Key, e.Value));
    }

    #endregion

    #region Methods

    #region Bracket Overrides

    ///<summary><inheritdoc/></summary>
    public TValue this[TKey key] {
      get => GetValueWithKey(key);
      set => SetByKey(key, value);
    }

    ///<summary><inheritdoc/></summary>
    public TValue this[int index] {
      get => GetValueAtIndex(index);
      set => SetByIndex(index, value);
    }

    #endregion

    #region Get

    ///<summary><inheritdoc/></summary>
    public bool ContainsKey(TKey key)
      => _collection.Contains(key);

    ///<summary><inheritdoc/></summary>
    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
      => ContainsKey(key);

    ///<summary><inheritdoc/></summary>
    public bool Contains(KeyValuePair<TKey, TValue> pair)
      => ContainsKey(pair.Key) && ContainsValue(pair.Value);

    ///<summary><inheritdoc/></summary>
    public bool ContainsValue(TValue item)
      => _collection.Any(e => item.Equals(e.Value));

    /// <summary>
    /// Gets the index of the key specified.
    /// </summary>
    /// <returns>Returns the index of the key specified if found. -1 if the key could not be located.</returns>
    public int IndexOf(TKey key) {
      if(_collection.Contains(key)) {
        return _collection.IndexOf(_collection[key]);
      } else {
        return -1;
      }
    }

    /// <summary>
    /// Get the key at the given index
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"/>
    public TValue GetValueAtIndex(int index) {
      if(index < 0 || index >= _collection.Count) {
        throw new IndexOutOfRangeException($"The index is outside of the bounds of the Ordered Dictionary: {index}");
      }
      return _collection[index].Value;
    }

    /// <summary>
    /// Get the key at the given index
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"/>
    public TKey GetKeyAtIndex(int index) {
      if(index < 0 || index >= _collection.Count) {
        throw new IndexOutOfRangeException($"The index is outside of the bounds of the Ordered Dictionary: {index}");
      }
      return _collection[index].Key;
    }

    /// <summary>
    /// Get the value with the given key
    /// </summary>
		/// <exception cref="KeyNotFoundException"/>
    public TValue GetValueWithKey(TKey key) {
      if(_collection.Contains(key) == false) {
        throw new KeyNotFoundException($"The given key was not present in the Ordered Dictionary: {key}");
      }

      KeyValuePair<TKey, TValue> pair = _collection[key];
      return pair.Value;
    }

    /// <summary>
    /// Get the key value pair at the given index.
    /// <exception cref="IndexOutOfRangeException"/>
    /// </summary>
    public KeyValuePair<TKey, TValue> GetPairAtIndex(int index) {
      if(index < 0 || index >= _collection.Count) {
        throw new IndexOutOfRangeException($"The index is outside of the bounds of the Ordered Dictionary: {index}");
      }

      return _collection[index];
    }

    ///<summary><inheritdoc/></summary>
    public bool TryGetValue(TKey key, out TValue value)
      => (value = ContainsKey(key)
        ? this[key]
        : default
      ) != null;
    
    ///<summary>
    /// Try to get the value with the given key.
    /// Default if not found.
    /// </summary>
    public TValue TryGetValue(TKey key)
      => TryGetValue(key, out var found)
        ? found
        : default;

    ///<summary>
    /// Try to get the value at the given index.
    /// Default if not found.
    /// </summary>
    public bool TryGetValueAtIndex(int index, out TValue value) {
      if(index < 0 || index >= _collection.Count) {
        value = default;
        return false;
      }

      value = _collection[index].Value;
      return true;
    }

    ///<summary>
    /// Try to get the value at the given index.
    /// Default if not found.
    /// </summary>
    public TValue TryGetValueAtIndex(int index) {
      if(index < 0 || index >= _collection.Count) {
        return default;
      }

      return _collection[index].Value;
    }

    ///<summary>
    /// Try to get the key at the given index.
    /// Default if not found.
    /// </summary>
    public TKey TryGetKeyAtIndex(int index) {
      if(index < 0 || index >= _collection.Count) {
        return default;
      }

      return _collection[index].Key;
    }

    ///<summary><inheritdoc/></summary>
    public bool TryGetKeyAtIndex(int index, out TKey value) {
      if(index < 0 || index >= _collection.Count) {
        value = default;
        return false;
      }

      value = _collection[index].Key;
      return true;
    }

    /// <summary>
    /// Get the key value pair at the given index.
    /// </summary>
    public KeyValuePair<TKey, TValue> TryToGetPairAtIndex(int index) {
      if(index < 0 || index >= _collection.Count) {
        return default;
      }

      return _collection[index];
    }

    /// <summary>
    /// Get the key value pair at the given index.
    /// </summary>
    public bool TryToGetPairAtIndex(int index, out KeyValuePair<TKey, TValue> value) {
      if(index < 0 || index >= _collection.Count) {
        value = default;
        return false;
      }

      value = _collection[index];
      return true;
    }

    #region Copy

    /// <summary>
    /// Get as readonly
    /// </summary>
    /// <returns></returns>
    public IReadOnlyOrderedDictionary<TKey, TValue> AsReadOnly() {
      IsReadOnly = true;
      return this;
    }

    ///<summary><inheritdoc/></summary>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
      => _collection.CopyTo(array, arrayIndex);

    #endregion

    #endregion

    #region Set

    ///<summary><inheritdoc/></summary>
    /// <exception cref="AccessViolationException"/>
    public virtual void Add(TKey key, TValue value) {
      if(IsReadOnly) {
        throw new AccessViolationException($"Tried to Add a Value: {value}, with Key: {key}, to Readonly Ordered Dictionary");
      }
      _collection.Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    ///<summary><inheritdoc/></summary>
    public void Add(KeyValuePair<TKey, TValue> item)
      => Add(item.Key, item.Value);

    ///<summary><inheritdoc/></summary>
    /// <exception cref="AccessViolationException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    public void SetByIndex(int index, TValue value) {
      if(IsReadOnly) {
        throw new AccessViolationException($"Tried to Set a Value: {value}, to Readonly Ordered Dictionary at index: {index}");
      }
      if(index < 0 || index >= _collection.Count) {
        throw new IndexOutOfRangeException($"The index is outside the bounds of the Ordered Dictionary: {index}");
      }

      KeyValuePair<TKey, TValue> pair = new(_collection[index].Key, value);
      _collection[index] = pair;
    }

    ///<summary><inheritdoc/></summary>
    /// <exception cref="AccessViolationException"/>
    public void SetByKey(TKey key, TValue value) {
      if(IsReadOnly) {
        throw new AccessViolationException($"Tried to Add a Value: {value}, with Key: {key}, to Readonly Ordered Dictionary");
      }

      int existing;
      if((existing = IndexOf(key)) >= 0) {
        _collection[existing] = new KeyValuePair<TKey, TValue>(key, value);
      } else
        _collection.Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    ///<summary><inheritdoc/></summary>
    /// <exception cref="AccessViolationException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    public void InsertByIndex(int index, TKey key, TValue value) {
      if(IsReadOnly) {
        throw new AccessViolationException($"Tried to Set a Value: {value}, with Key: {key}, to Readonly Ordered Dictionary at Index: {index}");
      }
      if(index < 0 || index >= _collection.Count) {
        throw new IndexOutOfRangeException($"The index is outside the bounds of the Ordered Dictionary: {index}");
      }

      _collection.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
    }

    #endregion

    #region Remove

    ///<summary><inheritdoc/></summary>
    public virtual void RemoveAt(int index) 
      => _collection.RemoveAt(index);

    ///<summary><inheritdoc/></summary>
    public bool Remove(TKey key) {
      if(_collection.Contains(key)) {
        _collection.Remove(key);
        return true;
      }

      return false;
    }

    ///<summary><inheritdoc/></summary>
    public bool RemoveByKey(TKey key) {
      if(_collection.Contains(key)) {
        _collection.Remove(key);
        return true;
      }

      return false;
    }

    ///<summary><inheritdoc/></summary>
    public bool Remove(KeyValuePair<TKey, TValue> item)
      => Remove(item.Key);

    ///<summary><inheritdoc/></summary>
    public virtual void Clear()
      => _collection.Clear();

    #endregion

    #region IEnumerator

    ///<summary><inheritdoc/></summary>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
      => _collection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
      => GetEnumerator();

    #endregion

    #endregion

    #region Conversions

    /// <summary>
    /// Auto conver this to a regular dictionary
    /// </summary>
    public static implicit operator Dictionary<TKey, TValue>(OrderedDictionary<TKey, TValue> original)
      => new(original);

    #endregion
  }
}