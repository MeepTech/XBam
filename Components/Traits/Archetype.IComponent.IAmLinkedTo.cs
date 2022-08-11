namespace Meep.Tech.XBam {

  public abstract partial class Archetype {

    public partial interface IComponent {

      /// <summary>
      /// Can be used to link an archetype component to a model component
      /// </summary>
      public interface IAmLinkedTo<TLinkedModelComponent>
        : ILinkedComponent,
          Archetype.IComponent
        where TLinkedModelComponent : IModel.IComponent<TLinkedModelComponent> {

        /// <summary>
        /// Build and get a default model component that is linked to this archetype component.
        /// This behavior can be overriden by default if you choose. It could even just be a ctor call.
        /// </summary>
        public new TLinkedModelComponent BuildDefaultModelComponent(IComponent.IBuilder builder, Universe universe = null)
          => ((universe.Components.GetFactory<TLinkedModelComponent>() ?? Components<TLinkedModelComponent>.Factory)
            as XBam.IComponent<TLinkedModelComponent>.Factory)
             .Make((IBuilder<TLinkedModelComponent>)builder);

        /// <summary>
        /// Build and get a default model component that is linked to this archetype component.
        /// </summary>
        IModel.IComponent ILinkedComponent.BuildDefaultModelComponent(IComponent.IBuilder builder, Universe universe)
          => BuildDefaultModelComponent(builder, universe);
      }

      /// <summary>
      /// Can be used to link an archetype component to a model component
      /// </summary>
      public interface ILinkedComponent
          : Archetype.IComponent {

        /// <summary>
        /// Build and get a default model component that is linked to this archetype component.
        /// </summary>
        public IModel.IComponent BuildDefaultModelComponent(IComponent.IBuilder builder, Universe universe = null)
          => null;
      }
    }
  }
}
