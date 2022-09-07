using System;

namespace Meep.Tech.XBam {

  public partial interface IModel {

    /// <summary>
    /// The base interface for Model Components.
    /// </summary>
    /// <remarks>
    /// Use the generic interface: IModel.IComponent[TComponentBase] to make new components instead
    /// </remarks>
    /// <see cref="IComponent{TComponentBase}"/>
    public partial interface IComponent 
      : XBam.IComponent {}

    /// <summary>
    /// A Component for an Model. Contains datas. Logic should usually be kept to Archetypes
    /// </summary>
    public partial interface IComponent<TComponentBase> 
      : IComponent,
        XBam.IComponent<TComponentBase>
      where TComponentBase : IComponent<TComponentBase> 
    {

      /// <summary>
      /// Can be used to set the model ctor during initalization.
      /// This should be used in the Static constructor for this type only.
      /// </summary>
      protected static void SetDefaultXBamConstructor(IComponent<TComponentBase>.IFactory factory, Func<IBuilder<TComponentBase>, TComponentBase> constructor) {
        // TODO: I wonder if i can throw an error if this isn't called from the right static ctor
        if (((Archetype)factory).AllowInitializationsAfterLoaderFinalization || !factory.Universe.Loader.IsFinished)
          Components<TComponentBase>.Factory.ModelConstructor = b => constructor((IBuilder<TComponentBase>)b);
        else throw new AccessViolationException($"Cannot modify a sealed component factory: {factory}");
      }
    }
  }
}