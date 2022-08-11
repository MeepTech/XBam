public static class RangeUtilities {

  /// <summary>
  /// Scale a float value to a new set of maxes and mins.
  /// </summary>
  public static float Clamp(this float value, float newMax, float newMin, float originalMin = 0, float originalMax = 1.0f) {
    float scaled = newMin + (value - originalMax) / (originalMin - originalMax) * (newMax - newMin);
    return scaled;
  }

  /// <summary>
  /// Clamp a value between two numbers with scaling
  /// </summary>
  public static double Clamp(this double value, double targetMin, double targetMax, double originalMin = 0, double originalMax = 1) {
    return (targetMax - targetMin)
      * ((value - originalMin) / (originalMax - originalMin))
      + targetMin;
  }

  /// <summary>
  /// fast clamp float to short with scaling
  /// </summary>
  public static short ClampToShort(float value, float minFloat = 0.0f, float maxFloat = 1.0f) {
    return (short)((short.MaxValue - short.MinValue)
      * ((value - minFloat) / (maxFloat - minFloat))
      + short.MinValue);
  }

  /// <summary>
  /// normalize the value to a float to between 0 and 1
  /// </summary>
  public static float Normalize(float value, int minValue, int maxValue) {
    return (
      (value - minValue)
      / (maxValue - minValue)
    );
  }

  /// <summary>
  /// Limit the value between the given numbers in a non scaling way.
  /// </summary>
  public static float Box(this float number, float min, float max) {
    if (number < min)
      return min;
    else if (number > max)
      return max;
    else
      return number;
  }
}