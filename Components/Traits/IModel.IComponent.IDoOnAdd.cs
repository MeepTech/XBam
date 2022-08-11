namespace Meep.Tech.XBam {

  public partial interface IComponent {

    /// <summary>
    /// Interface indicating this component should do something when added to a model.
    /// </summary>
    public interface IDoOnAdd {

      /// <summary>
      /// Executed when this is added to a model.
      /// </summary>
      internal protected void ExecuteWhenAdded(XBam.IReadableComponentStorage parent);
    }
  }
}
