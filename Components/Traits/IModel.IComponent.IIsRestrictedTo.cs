namespace Meep.Tech.XBam {

  public partial interface IModel {

    public partial interface IComponent {

      /// <summary>
      /// Can be used to indicate that this component is restricted to specific types of models based on the provided base model type.
      /// </summary>
      /// <seealso cref="Archetype.IComponent.IIsRestrictedToCertainTypes"/>
      /// <seealso cref="Archetype.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
      /// <seealso cref="IModel.IComponent.IIsRestrictedToCertainTypes"/>
      /// <seealso cref="IModel.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
      public new interface IIsRestrictedTo<TModelBase>
        : XBam.IComponent.IIsRestrictedTo<TModelBase>,
          IIsRestrictedToCertainTypes,
          IComponent
        where TModelBase : IModel {

        /// <summary>
        /// Check if this is compatable with a model
        /// </summary>
        bool IIsRestrictedToCertainTypes.IsCompatableWith(IModel model)
          => model is TModelBase;
      }

      /// <summary>
      /// Can be used to indicate that this component is restricted to specific types of models based on the provided base model type.
      /// </summary>
      /// <seealso cref="Archetype.IComponent.IIsRestrictedToCertainTypes"/>
      /// <seealso cref="Archetype.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
      /// <seealso cref="IModel.IComponent.IIsRestrictedToCertainTypes"/>
      /// <seealso cref="IModel.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
      public new interface IIsRestrictedToCertainTypes
        : XBam.IComponent.IIsRestrictedToCertainTypes,
          IComponent {

        /// <summary>
        /// Check if this is compatable with a model
        /// </summary>
        bool IsCompatableWith(IModel model);
      }
    }
  }
}