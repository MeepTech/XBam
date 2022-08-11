namespace Meep.Tech.XBam.Utility {

  /// <summary>
  /// Verbose(in-code) Comparitors
  /// </summary>
  public static class Comparitors {

    /// <summary>
    /// Verbose identity comparitor
    /// </summary>
    public static T Identity<T>(this T obj)
      => obj;
  }
}
