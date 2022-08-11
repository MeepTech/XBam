namespace Meep.Tech.XBam {

  public partial interface IModel {

    /// <summary>
    /// Just makes the struct based model or component use the default universe so you don't need to set it yourself
    /// </summary>
    public interface IUseDefaultUniverse : IModel {

      /// <summary>
      /// This can be overriden if you want, but by default, struct based components don't have universe info at hand
      /// </summary>
      Universe IModel.Universe {
        get => Models.DefaultUniverse;
        set => _ = value;
      }
    }
  }

  public partial interface IComponent {

    /// <summary>
    /// Just makes the struct based model or component use the default universe so you don't need to set it yourself
    /// </summary>
    public new interface IUseDefaultUniverse : XBam.IComponent {

      /// <summary>
      /// This can be overriden if you want, but by default, struct based components don't have universe info at hand
      /// </summary>
      Universe IModel.Universe {
        get => Models.DefaultUniverse;
        set => _ = value;
      }

      /// <summary>
      /// This can be overriden if you want, but by default, struct based components don't have universe info at hand
      /// </summary>
      Universe XBam.IComponent.Universe {
        get => Components.DefaultUniverse;
        set => _ = value;
      }

      IFactory XBam.IComponent.Factory {
        get => (Universe ?? Components.DefaultUniverse).Components.GetFactory(GetType());
        set { }
      }
    }
  }
}
