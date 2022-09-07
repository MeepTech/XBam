using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {

  public static class EnumerableExtensions {

    /// <summary>
    /// Get x of something.
    /// </summary>
    public static IEnumerable<T> Of<T>(this int count, T value = default) {
      for (int x = 0; x < count; x++) {
        yield return value;
      }
    }

    /// <summary>
    /// Get x of something.
    /// </summary>
    public static IEnumerable<T> Of<T>(this int count, Func<T> action) {
      for (int x = 0; x < count; x++) {
        yield return action();
      }
    }

    /// <summary>
    /// Get x of something with the index looping up
    /// </summary>
    public static IEnumerable<T> Of<T>(this int count, Func<int, T> action) {
      for (int x = 0; x < count; x++) {
        yield return action(x);
      }
    }

    /// <summary>
    /// Turn one item into an enumerable contaiing itself.
    /// </summary>
    public static IEnumerable<T> AsSingleItemEnumerable<T>(this T singleItem)
      => new[] { singleItem };

    /// <summary>
    /// Turn one item into an enumerable contaiing itself.
    /// </summary>
    public static T ThenIf<T>(this T original, bool isTrue, Func<T, T> doTransformation)
      => isTrue
        ? original
        : doTransformation(original);

    /// <summary>
    /// Turn one item into an enumerable contaiing itself.
    /// </summary>
    public static void ThenIf<T>(this T original, bool isTrue, Action<T> @do) {
      if (isTrue) @do(original);
    }

    /// <summary>
    /// do on each
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> @do) {
      foreach(T @value in enumeration) {
        @do(value);
      }
    }

    /// <summary>
    /// do on each with an index too
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T, int> @do) {
      int index = 0;
      foreach(T @value in enumeration) {
        @do(value, index++);
      }
    }
    
    /// <summary>
    /// do on each
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> enumeration, Func<T, bool> @doUntilFalse) {
      foreach(T @value in enumeration) {
        if(!@doUntilFalse(value)) {
          break;
        }
      }
    }

    /// <summary>
    /// do on each with an index too
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> enumeration, Func<T, int, bool> @doUntilFalse) {
      int index = 0;
      foreach(T @value in enumeration) {
        if(!@doUntilFalse(value, index++)) {
          break;
        }
      }
    }

    /// <summary>
    /// append a value if the condition is true
    /// </summary>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumeration, bool condition, T item)
      => condition ? enumeration.Append(item) : enumeration;

    /// <summary>
    /// append a value if the condition is true
    /// </summary>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumeration, Func<bool> @if, T item) 
      => @if() ? enumeration.Append(item) : enumeration;

    /// <summary>
    /// append a value if the condition is true
    /// </summary>
    public static IEnumerable<T> ConcatIf<T>(this IEnumerable<T> enumeration, Func<bool> @if, IEnumerable<T> other) 
      => @if() ? enumeration.Concat(other) : enumeration;

    /// <summary>
    /// append a collection to the current one if the other collection is not null
    /// </summary>
    public static IEnumerable<T> ConcatIfNotNull<T>(this IEnumerable<T> enumeration, IEnumerable<T>? other) 
      => other is not null ? enumeration.Concat(other) : enumeration;

    /// <summary>
    /// Reverse the list.
    /// </summary>
    public static IEnumerable<T> Reverse<T>(this IList<T> list) {
      for(int i = list.Count - 1; i >= 0; i--) {
        yield return list[i];
      }
    }

    /// <summary>
    /// Get the values until the desired index. Not including it.
    /// </summary>
    public static IEnumerable<T> Until<T>(this IEnumerable<T> list, int index) {
      int i = 0;
      var enumerator = list.GetEnumerator();
      while(i < index && enumerator.MoveNext()) {
        yield return enumerator.Current;
        i++;
      }
    }
  }
}
