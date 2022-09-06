namespace Meep.Tech.XBam {

  public abstract partial class Model {

    /// <summary>
    /// The base class for interface based models.
    /// Extend Model<TModelBase, TArchetypeBase>.IFromInterface instead
    /// </summary>
    public interface IFromInterface : IModel {

      /// <summary>
      /// The base interface type model.
      /// </summary>
      System.Type InterfaceBaseType {
        get;
        internal set;
      }
    }
  }

  public abstract partial class Model<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase, TArchetypeBase> 
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {
    /// <summary>
    /// Used to make Model[TModelBase, TArchetypeBase] but from an interface. For struct based models, or if you want to use your own base type.
    /// </summary>
    public new interface IFromInterface : IModel<TModelBase, TArchetypeBase>, Model.IFromInterface {

      /// <summary>
      /// The archetype for this model
      /// </summary>
      new TArchetypeBase Archetype {
        get;
        protected set;
      }

      ///<summary><inheritdoc/></summary>
      TArchetypeBase IModel<TModelBase, TArchetypeBase>.Archetype {
        get => Archetype;
        set => Archetype = value;
      }

      ///<summary><inheritdoc/></summary>
      System.Type Model.IFromInterface.InterfaceBaseType {
        get => typeof(IModel<TModelBase, TArchetypeBase>);
        set { }
      }

      /// <summary>
      /// For the base configure calls
      /// </summary>
      IFromInterface OnInitialized(XBam.IBuilder builder)
        => this;

      IModel IModel.OnInitialized(Archetype archetype, Universe universe, XBam.IBuilder builder) {
        Archetype = (TArchetypeBase)(archetype ?? builder?.Archetype);
        Universe = universe ?? builder?.Archetype.Universe ?? Universe.Default;

        return OnInitialized(builder);
      }

      /// <summary>
      /// For the base configure calls
      /// </summary>
      new IModel OnFinalized(XBam.IBuilder builder)
        => this;

      IModel IModel.OnFinalized(XBam.IBuilder builder) {
        return OnFinalized(builder);
      }
    }
  }
}