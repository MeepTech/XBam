using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {
  public static class ListExtensions {
    static readonly Random _random = new();

    /// <summary>
    /// Get a random entry.
    /// </summary>
    public static T RandomEntry<T>(this IEnumerable<T> entries, Random randomGenerator = null)
      => entries.Count() == 0 
        ? default 
        : (entries.Count() == 1 
            ? entries.First() 
            : entries.ElementAt((randomGenerator ?? _random)
              .Next(0, entries.Count() - 1)));

    public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> original) {
      if (original is null)
        throw new ArgumentNullException(nameof(original));

      return original as IReadOnlyList<T> 
        ?? new ReadOnlyListAdapter<T>(original);
    }

    sealed class ReadOnlyListAdapter<T> : IReadOnlyList<T> {

      public int Count 
        => _values.Count;

      public T this[int index] 
        => _values[index];

      readonly IList<T> _values;

      public ReadOnlyListAdapter(IList<T> source) {
        _values = source;
      }

      public IEnumerator<T> GetEnumerator() 
        => _values.GetEnumerator();

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
    }

    /// <summary>
    /// spread opperators
    /// </summary>
    public static void Deconstruct<T>(this IList<T> list, out T first, out IList<T> rest) {
      first = list.Count > 0 
        ? list[0] 
        : default;

      rest = list.Skip(1).ToList();
    }

    /// <summary>
    /// spread opperators
    /// </summary>
    public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out IList<T> rest) {
      first = list.Count > 0 
        ? list[0] 
        : default;
      second = list.Count > 1 
        ? list[1] 
        : default;

      rest = list.Skip(2).ToList();
    }
  }
}
