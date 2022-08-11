namespace Meep.Tech.XBam {

  public abstract partial class Model<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase, TArchetypeBase> 
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {
    /// <summary>
    /// Used to make Model[TModelBase, TArchetypeBase] but from an interface. For struct based models, or if you want to use your own base type.
    /// </summary>
    public interface IFromInterface : IModel<TModelBase, TArchetypeBase> {

      /// <summary>
      /// The archetype for this model
      /// </summary>
       new TArchetypeBase Archetype {
        get;
        protected set;
      }

      TArchetypeBase IModel<TModelBase, TArchetypeBase>.Archetype {
        get => Archetype;
        set => Archetype = value;
      }

      /// <summary>
      /// For the base configure calls
      /// </summary>
      IFromInterface OnInitialized(XBam.IBuilder builder)
        => this;

      IModel IModel.OnInitialized(Archetype archetype, Universe universe, XBam.IBuilder builder) {
        Archetype = (TArchetypeBase)(archetype ?? builder?.Archetype);
        Universe = universe ?? builder?.Archetype.Id.Universe ?? Universe.Default;

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