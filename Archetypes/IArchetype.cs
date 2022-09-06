using System;
using System.Collections.Generic;

namespace Meep.Tech.XBam {

  /// <summary>
  /// A singleton data store and factory.
  /// </summary>
  public interface IArchetype : IFactory, IReadableComponentStorage {

    /// <summary>
    /// The Base Archetype this Archetype derives from.
    /// </summary>
    Type BaseArchetype { get; }

    /// <summary>
    /// The Base type of model that this archetype family produces.
    /// </summary>
    Type ModelBaseType { get; }

    /// <summary>
    /// The Base type of model that this archetype family produces.
    /// </summary>
    Type ModelTypeProduced { get; }

    /// <summary>
    /// The System.Type of this Archetype
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// If this is an archetype that inherits from Archetype[,] directly.
    /// </summary>
    bool IsBaseArchetype { get; }

    /// <summary>
    /// The collection containing this archetype
    /// </summary>
    Archetype.Collection Types { get; }

    /// <summary>
    /// The tags pertaining to this archetype
    /// </summary>
    IEnumerable<ITag> Tags { get; }
  }
}