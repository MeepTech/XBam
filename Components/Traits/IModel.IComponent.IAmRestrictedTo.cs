namespace Meep.Tech.XBam {

  public partial interface IModel {

    public partial interface IComponent {

      /// <summary>
      /// Can be used to indicate that this component is restricted to specific types of models based on the provided base model type.
      /// </summary>
      /// <seealso cref="Archetype.IComponent.IAmRestrictedToCertainTypes"/>
      /// <seealso cref="Archetype.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
      /// <seealso cref="IModel.IComponent.IAmRestrictedToCertainTypes"/>
      /// <seealso cref="IModel.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
      public new interface IAmRestrictedTo<TModelBase>
        : XBam.IComponent.IAmRestrictedTo<TModelBase>,
          IAmRestrictedToCertainTypes,
          IComponent
        where TModelBase : IModel {

        /// <summary>
        /// Check if this is compatable with a model
        /// </summary>
        bool IAmRestrictedToCertainTypes.IsCompatableWith(IModel model)
          => model is TModelBase;
      }

      /// <summary>
      /// Can be used to indicate that this component is restricted to specific types of models based on the provided base model type.
      /// </summary>
      /// <seealso cref="Archetype.IComponent.IAmRestrictedToCertainTypes"/>
      /// <seealso cref="Archetype.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
      /// <seealso cref="IModel.IComponent.IAmRestrictedToCertainTypes"/>
      /// <seealso cref="IModel.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
      public new interface IAmRestrictedToCertainTypes
        : XBam.IComponent.IAmRestrictedToCertainTypes,
          IComponent {

        /// <summary>
        /// Check if this is compatable with a model
        /// </summary>
        bool IsCompatableWith(IModel model);
      }
    }
  }
}