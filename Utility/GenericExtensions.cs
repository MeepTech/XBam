using System;

namespace Meep.Tech.XBam.Utility {

  /// <summary>
  /// Extensions for any class or struct
  /// </summary>
  public static class GenericExtensions {

    /// <summary>
    /// Modify and return something.
    /// </summary>
    public static T Modify<T>(this T @object, Func<T, T> modifier)
      => modifier(@object);

    /// <summary>
    /// Modify and return an object.
    /// </summary>
    public static T Modify<T>(this T @object, Action<T> modifier)
      where T : class {
      modifier(@object);
      return @object;
    }

    /// <summary>
    /// Modify and return an object.
    /// </summary>
    public static O As<I, O>(this I @object, Func<I, O> modifier) {
      return modifier(@object);
    }

    /// <summary>
    /// Modify and return an object.
    /// </summary>
    public static O ThenReturn<I, O>(this I @object, Func<I, O> modifier)
      => As(@object, modifier);

    /// <summary>
    /// do something with this object.
    /// </summary>
    public static void ThenDo<T>(this T @object, Action<T> @do)
      => @do(@object);
  }
}
