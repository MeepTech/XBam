using System;

namespace Meep.Tech.Noise {

  /// <summary>
  /// Simple Global Random Value Generation.
  /// Mostly used for seedless random number generation.
  /// Should not be used for cryptography or multi-threading/specific seed based generation.
  /// </summary>
  public static class RNG {

    /// <summary>
    /// Overrideable uniqe id function
    /// </summary>
    public static Func<string> GenerateNextGuid { get; set; }
      = () => Guid.NewGuid().ToString();

    /// <summary>
    /// Get the next globally unique string id.
    /// </summary>
    public static string NextGuid
      => GenerateNextGuid();

    /// <summary>
    /// Used to get the seed for the static randoness function.
    /// </summary>
    public static int GenerateRandomessSeed()
      => System.Security.Cryptography.RandomNumberGenerator.GetInt32(Int32.MinValue, Int32.MaxValue);

    /// <summary>
    /// Default static randomness generator
    /// </summary>
    public static System.Random Static {
      get;
    } = new Random(GenerateRandomessSeed());

    /// <summary>
    /// Generate a sort of normal random new word.
    /// </summary>
    public static string GenerateRandomNewWord(int? length = null, System.Random random = null) {
      random ??= Static;
      length ??= random.Next(3, 9);
      string[] consonants = { "b", "br", "c", "cr", "d", "dr", "f", "fr", "fn", "g", "gr", "h", "j", "k", "kr", "l", "m", "n", "ng", "p", "pr", "pf", "q", "r", "s", "sr", "st", "sp", "sh", "zh", "t", "th", "v", "w", "x", "z" };
      string[] vowels = { "a", "e", "i", "o", "u", "ae", "y", "oo", "ae" };
      string word = "";
      word += consonants[random.Next(consonants.Length)].ToUpper();
      word += vowels[random.Next(vowels.Length)];
      int lettersAdded = 2;
      while (lettersAdded < length) {
        word += consonants[random.Next(consonants.Length)];
        lettersAdded++;
        word += vowels[random.Next(vowels.Length)];
        lettersAdded++;
      }

      return word;
    }

    /// <summary>
    /// Get the next random int value between and including 0 and 100
    /// </summary>
    public static int NextPercent(this System.Random random)
      => (int)Math.Round(random.NextDouble() * 100);
  }
}
