using System;

namespace Meep.Tech.XBam {

  /// <summary>
  /// These make models
  /// </summary>
  public partial interface IFactory {

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public Archetype.Identity Id {
      get;
    }

    /// <summary>
    /// Overrideable Model Constructor
    /// </summary>
    internal Func<IBuilder, IModel> _modelConstructor {
      get;
      set;
    }

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal abstract IModel Make();

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal abstract IModel Make(IBuilder builder);

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal abstract IModel Make(Func<IBuilder, IBuilder> builderConfiguration);
  }
}
