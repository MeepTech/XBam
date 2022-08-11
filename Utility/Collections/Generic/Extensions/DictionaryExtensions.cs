using Meep.Tech.XBam;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {
  public static class DictionaryExtensions {

    /// <summary>
    /// Try tho get a value. Returns default on failure.
    /// </summary>
    public static TValue TryToGet<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        => dictionary.TryGetValue(key, out var found) ? found : default;

    /// <summary>
    /// Try tho get a value. Returns default on failure.
    /// </summary>
    public static TValue TryToGet<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key)
      where TDictionary : IReadOnlyDictionary<TKey, TValue>
        => dictionary.TryGetValue(key, out var found) ? found : default;

    /// <summary>
    /// Try tho get a value. Returns default on failure.
    /// </summary>
    public static TValue TryToGet<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, TValue @default)
      where TDictionary : IDictionary<TKey, TValue>
        => dictionary.TryGetValue(key, out var found) ? found : (dictionary[key] = @default);

    /// <summary>
    /// Add an item inline without needing to make it if it contains it's own key
    /// </summary>
    public static void Add<TDictionary, TKey, TValue>(this TDictionary dictionary, TValue value, Func<TValue, TKey> getKey)
      where TDictionary : IDictionary<TKey, TValue>
        => dictionary.Add(getKey(value), value);

    /// <summary>
    /// Set ([]=) alias.
    /// </summary>
    public static void Set<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, TValue value)
      where TDictionary : IDictionary<TKey, TValue> 
    {
      dictionary[key] = value;
    }

    #region ValueCollectionManipulation

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AddToValueCollection<TKey, TValue>(this IDictionary<TKey, ICollection<TValue>> dictionary, TKey key, TValue value) {
      if (dictionary.TryGetValue(key, out ICollection<TValue> valueCollection)) {
        valueCollection.Add(value);
      }
      else
        dictionary.Add(key, new List<TValue> { value });
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AddToValueCollection<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value) {
      if (dictionary.TryGetValue(key, out List<TValue> valueCollection)) {
        valueCollection.Add(value);
      }
      else
        dictionary.Add(key, new List<TValue> { value });
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AddToValueCollection<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, TValue value) 
      where TDictionary : IDictionary<TKey, ICollection<TValue>> 
    {
      if (dictionary.TryGetValue(key, out ICollection<TValue> valueCollection)) {
        valueCollection.Add(value);
      }
      else
        dictionary.Add(key, new List<TValue> { value });
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AppendToValueCollection<TKey, TValue>(this IDictionary<TKey, IEnumerable<TValue>> dictionary, TKey key, TValue value)  {
      if(dictionary.TryGetValue(key, out IEnumerable<TValue> currentValueCollection)) {
       dictionary[key] = currentValueCollection.Append(value);
      }
      else
        dictionary.Add(key, new List<TValue> { value });
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AppendToValueCollection<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, TValue value) where TDictionary : IDictionary<TKey, IEnumerable<TValue>> {
      if (dictionary.TryGetValue(key, out IEnumerable<TValue> currentValueCollection)) {
        dictionary[key] = currentValueCollection.Append(value);
      }
      else
        dictionary.Add(key, new List<TValue> { value });
    }

    /// <summary>
    /// Remove an item from an ICollection within a dictionary at the given key
    /// </summary>
    public static bool RemoveFromValueCollection<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, TValue value) where TDictionary : IDictionary<TKey, ICollection<TValue>> {
      if(dictionary.TryGetValue(key, out ICollection<TValue> valueCollection)) {
        if(valueCollection.Remove(value)) {
          if(!valueCollection.Any()) {
            dictionary.Remove(key);
          }
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Remove an item from an ICollection within a dictionary at the given key
    /// </summary>
    public static bool RemoveFromValueCollection<TKey, TValue>(this IDictionary<TKey, ICollection<TValue>> dictionary, TKey key, TValue value) {
      if (dictionary.TryGetValue(key, out ICollection<TValue> valueCollection)) {
        if (valueCollection.Remove(value)) {
          if (!valueCollection.Any()) {
            dictionary.Remove(key);
          }
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Remove an item from an ICollection within a dictionary at the given key
    /// </summary>
    public static bool RemoveFromValueCollection<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value) {
      if (dictionary.TryGetValue(key, out List<TValue> valueCollection)) {
        if (valueCollection.Remove(value)) {
          if (!valueCollection.Any()) {
            dictionary.Remove(key);
          }
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AddToInnerHashSet<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, TValue value) where TDictionary : IDictionary<TKey, HashSet<TValue>> {
      if (dictionary.TryGetValue(key, out HashSet<TValue> valueCollection)) {
        valueCollection.Add(value);
      } else
        dictionary.Add(key, new HashSet<TValue> { value });
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AddToInnerHashSet<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value) {
      if (dictionary.TryGetValue(key, out HashSet<TValue> valueCollection)) {
        valueCollection.Add(value);
      }
      else
        dictionary.Add(key, new HashSet<TValue> { value });
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static bool RemoveFromInnerHashSet<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, TValue value) where TDictionary : IDictionary<TKey, HashSet<TValue>> {
      if (dictionary.TryGetValue(key, out HashSet<TValue> valueCollection)) {
        return valueCollection.Remove(value);
      } else return false;
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static bool RemoveFromInnerHashSet<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value) {
      if (dictionary.TryGetValue(key, out HashSet<TValue> valueCollection)) {
        return valueCollection.Remove(value);
      }
      else return false;
    }

    /// <summary>
    /// Bucketize a collecton of keys and values.
    /// </summary>
    public static Dictionary<TKey, ICollection<TValue>> Bucketize<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items) {
      Dictionary<TKey, ICollection<TValue>> @return = new();
      foreach (var item in items) {
        @return.AddToValueCollection(item.Key, item.Value);
      }

      return @return;
    }

    #endregion

    #region Add

    /// <summary>
    /// Append a value to a hash set and return the collection
    /// </summary>
    public static HashSet<TValue> WithPair<TValue>(this HashSet<TValue> current, TValue value) {
      current.Add(value);
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static TDictionary WithAddedPair<TDictionary, TKey, TValue>(this TDictionary current, TKey key, TValue value) 
      where TDictionary : IDictionary<TKey, TValue> 
    {
      current.Add(key, value);
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static Dictionary<TKey, TValue> Append<TDictionary, TKey, TValue>(this TDictionary current, TKey key, TValue value)
      where TDictionary : IReadOnlyDictionary<TKey, TValue> 
        => new Dictionary<TKey, TValue>(current).WithAddedPair(key, value);

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static Dictionary<TKey, TValue> Append<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> current, TKey key, TValue value)
        => new Dictionary<TKey, TValue>(current).WithAddedPair(key, value);

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static TDictionary WithSetPair<TDictionary, TKey, TValue>(this TDictionary current, TKey key, TValue value) 
      where TDictionary : IDictionary<TKey, TValue> {
      current[key] = value;
      return current;
    }

    #endregion

    #region Merge 

    /// <summary>
    /// Merge dictionaries together, overriding any values with the same key.
    /// </summary>
    public static TDictionary WithVauesFrom<TDictionary, TKey, TValue>(this TDictionary baseDict, params IReadOnlyDictionary<TKey, TValue>[] dictionariesToMergeIn)
      where TDictionary : IDictionary<TKey, TValue>
    {
      if(dictionariesToMergeIn == null)
        return baseDict;
      foreach(Dictionary<TKey, TValue> dictionary in dictionariesToMergeIn) {
        foreach(KeyValuePair<TKey, TValue> entry in dictionary) {
          baseDict[entry.Key] = entry.Value;
        }
      }

      return baseDict;
    }

    /// <summary>
    /// Merge dictionaries together, returning a new dictionary with the combined values.
    /// The new dictionaries override values as they're added.
    /// </summary>
    public static Dictionary<TKey, TValue> Merge<TDictionary, TKey, TValue>(this TDictionary baseDict, params IReadOnlyDictionary<TKey, TValue>[] dictionariesToMergeIn)
      where TDictionary : IReadOnlyDictionary<TKey, TValue> 
    {
      if(dictionariesToMergeIn == null)
        return new(baseDict);
      Dictionary<TKey, TValue> result = new(baseDict);
      foreach(var additionalDictionary in dictionariesToMergeIn) {
        foreach(KeyValuePair<TKey, TValue> entry in additionalDictionary) {
          result[entry.Key] = entry.Value;
        }
      }

      return result;
    }

    #endregion
  }
}
