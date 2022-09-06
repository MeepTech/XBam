using System;

namespace Meep.Tech.XBam {

  /// <summary>
  /// These make models
  /// </summary>
  public partial interface IFactory : IResource {

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public Archetype.Identity Id {
      get;
    }

    /// <summary>
    /// Overrideable Model Constructor
    /// </summary>
    internal Func<IBuilder, IModel?> _modelConstructor {
      get;
      set;
    }
  }
}
