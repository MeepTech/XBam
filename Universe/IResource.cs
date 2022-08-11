namespace Meep.Tech.XBam {

  /// <summary>
  /// A resource within a universe.
  /// Can be an Archetype, Model, Component, or Enumeration.
  /// </summary>
  public interface IResource {

    /// <summary>
    /// The universe this resource belongs to
    /// </summary>
    Universe Universe {
      get;
    }
  }
}
