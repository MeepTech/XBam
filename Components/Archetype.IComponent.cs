namespace Meep.Tech.XBam {

  public abstract partial class Archetype {

    /// <summary>
    /// A Component for an archetype. Contains data and system logics.
    /// This is the non-generic base class for utility
    /// </summary>
    public partial interface IComponent
      : XBam.IComponent {}

    /// <summary>
    /// A Component for an archetype. Contains data and system logics.
    /// </summary>
    public partial interface IComponent<TComponentBase> 
      : XBam.IComponent<TComponentBase>, IComponent
      where TComponentBase: IComponent<TComponentBase>
    {}
  }
}
