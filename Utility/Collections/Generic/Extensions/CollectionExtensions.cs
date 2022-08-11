using System;
using System.Collections.Generic;

namespace Meep.Tech.Collections.Generic {
  public static class CollectionExtensions {
    public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> original) {
      if (original is null)
        throw new ArgumentNullException(nameof(original));

      return original as IReadOnlyCollection<T> 
        ?? new ReadOnlyCollectionAdapter<T>(original);
    }

    sealed class ReadOnlyCollectionAdapter<T> : IReadOnlyCollection<T> {

      public int Count 
        => _values.Count;

      readonly ICollection<T> _values;

      public ReadOnlyCollectionAdapter(ICollection<T> source) {
        _values = source;
      }

      public IEnumerator<T> GetEnumerator() 
        => _values.GetEnumerator();

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
    }
  }
}
