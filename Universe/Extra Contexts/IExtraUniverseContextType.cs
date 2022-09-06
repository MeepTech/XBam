namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Base class for interfaces for extra contexts.
  /// </summary>
  public interface IExtraUniverseContextType {

    /// <summary>
    /// Unique id.
    /// </summary>
    string Id 
      { get; }

    /// <summary>
    /// The universe this context is for.
    /// </summary>
    Universe Universe 
      { get; }

    /// <summary>
    /// The options passed in to this extra context on creation.
    /// </summary>
    Universe.ExtraContext.Settings? Options
      { get; }
  }
}
