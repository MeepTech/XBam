namespace Meep.Tech.XBam {

  public abstract partial class Archetype {

    public partial interface IComponent {

      /// <summary>
      /// Can be used to indicate that this component is restricted to a specific branch of archetypes based on the provided base archetype.
      /// </summary>
      /// <seealso cref="Archetype.IComponent.IIsRestrictedToCertainTypes"/>
      /// <seealso cref="Archetype.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
      /// <seealso cref="IModel.IComponent.IIsRestrictedToCertainTypes"/>
      /// <seealso cref="IModel.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
      public new interface IIsRestrictedTo<TArchetypeBase>
        : XBam.IComponent.IIsRestrictedTo<TArchetypeBase>,
          Archetype.IComponent.IIsRestrictedToCertainTypes,
          IComponent
        where TArchetypeBase : Archetype {

        /// <summary>
        /// Check if this is compatable with an archetype
        /// </summary>
        bool Archetype.IComponent.IIsRestrictedToCertainTypes.IsCompatableWith(Archetype archetype)
          => archetype is TArchetypeBase;
      }

      /// <summary>
      /// Can be used to indicate that this component is restricted to a specific branch of archetypes based on the provided base archetype.
      /// Base functionality. Extend the generic version instead.
      /// Base Generic Type.
      /// </summary>
      /// <seealso cref="Archetype.IComponent.IIsRestrictedToCertainTypes"/>
      /// <seealso cref="Archetype.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
      /// <seealso cref="IModel.IComponent.IIsRestrictedToCertainTypes"/>
      /// <seealso cref="IModel.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
      public new interface IIsRestrictedToCertainTypes
        : XBam.IComponent.IIsRestrictedToCertainTypes,
          IComponent {

        /// <summary>
        /// Check if this is compatable with an archetype
        /// </summary>
        public virtual bool IsCompatableWith(Archetype archetype)
          => false;
      }
    }
  }
}
