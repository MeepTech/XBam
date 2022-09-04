namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Base class for interfaces for extra contexts.
  /// </summary>
  public interface IExtraUniverseContextType {

    /// <summary>
    /// Unique id.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The universe this context is for.
    /// </summary>
    Universe Universe { get; }
  }
}
