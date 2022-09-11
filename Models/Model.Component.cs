namespace Meep.Tech.XBam {
  /*
  public partial interface IModel {
    public abstract class Component : IModel.IComponent {
      Universe XBam.IComponent.Universe
        { get => Universe; set => Universe = value; }
      XBam.IComponent.IFactory XBam.IComponent.Factory
        { get => Factory; set => Factory = value; }

      public Universe Universe
        { get; private set; }

      public IModel.IComponent.IFactory Factory 
        { get; private set; }
    }

    public abstract class Component<TComponentBase>
      : Component,
      IModel.IComponent<TComponentBase>
      where TComponentBase : Component<TComponentBase> {
      public new IComponent<TComponentBase>.Factory Factory
        => (IComponent<TComponentBase>.Factory)base.Factory;
    }
  }*/
}