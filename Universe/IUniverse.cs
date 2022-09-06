using Meep.Tech.XBam.Configuration;

namespace Meep.Tech.XBam {

  /// <summary>
  /// A collection of XBam Resources
  /// </summary>
  public interface IUniverse {

    /// <summary>
    /// The unique key of this universe.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Enumerations Data
    /// </summary>
    Universe.EnumerationData Enumerations { get; }

    /// <summary>
    /// Archetypes data
    /// </summary>
    Universe.ArchetypesData Archetypes { get; }

    /// <summary>
    /// Models data
    /// </summary>
    Universe.ModelsData Models { get; }

    /// <summary>
    /// Components data
    /// </summary>
    Universe.ComponentsData Components { get; }

    /// <summary>
    /// The extra contexts
    /// </summary>
    Universe.ExtraContextsData ExtraContexts { get; }

    /// <summary>
    /// The loader used to build this universe
    /// </summary>
    Loader Loader { get; }

    /// <summary>
    /// Get an extra context item that was assigned to this universe.
    /// </summary>
    TExtraContext GetExtraContext<TExtraContext>()
      where TExtraContext : IExtraUniverseContextType;

    /// <summary>
    /// Get an extra context item that was assigned to this universe.
    /// </summary>
    bool HasExtraContext<TExtraContext>()
      where TExtraContext : IExtraUniverseContextType;

    /// <summary>
    /// Try to get an extra context item that was assigned to this universe.
    /// </summary>
    bool TryToGetExtraContext<TExtraContext>(out TExtraContext? extraContext)
      where TExtraContext : IExtraUniverseContextType;

    /// <summary>
    /// Try to get an extra context item that was assigned to this universe.
    /// </summary>
    TExtraContext? TryToGetExtraContext<TExtraContext>()
      where TExtraContext : IExtraUniverseContextType;
  }
}