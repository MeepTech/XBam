namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Configures the Auto Builder on Models.
  /// </summary>
  public partial interface IModelAutoBuilder : IExtraUniverseContextType {

    ///<summary><inheritdoc/></summary>
    new ISettings Options 
      { get; }

    /// <summary>
    /// Check if an archetype has auto builder steps included.
    /// </summary>
    bool HasAutoBuilderSteps<TArchetype>(TArchetype archetype) where TArchetype : Archetype;
  }
}
