namespace Meep.Tech.XBam.Utility {

  /// <summary>
  /// Some string utilities i use a lot
  /// </summary>
  public static class StringUtilities {

    /// <summary>
    /// Limit a string to a max size, with or without elipses.
    /// </summary>
    public static string LimitTo(this string value, int maxSize, bool withElipsies = true) {
      if ((value.Length + (withElipsies ? 3 : 0)) >= maxSize) {
        value = value[0..(withElipsies ? maxSize - 3 : maxSize)];
      }
      return withElipsies ? value + "..." : value;
    }

    /// <summary>
    /// Converts this pascal case string to sentence case with spaced words.
    /// From: https://www.codeproject.com/Articles/108996/Splitting-Pascal-Camel-Case-with-RegEx-Enhancement
    /// </summary>
    public static string ToSentenceCase(this string PascalCaseString)
      =>  System.Text.RegularExpressions.Regex.Replace(
        PascalCaseString,
        "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
        " $1",
        System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
      };
}
