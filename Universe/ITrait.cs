namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Used to identify an interface as an Xbam Trait
  /// </summary>
  public interface ITrait<TTraitType> {

    /// <summary>
    /// The name of this type of trait
    /// </summary>
    public string TraitName { get; }

    /// <summary>
    /// The desription of this type of trait
    /// </summary>
    public string TraitDescription { get; }
  }
}
