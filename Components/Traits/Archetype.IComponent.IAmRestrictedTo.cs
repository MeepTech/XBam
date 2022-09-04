namespace Meep.Tech.XBam {

  public abstract partial class Archetype {

    public partial interface IComponent {

      /// <summary>
      /// Can be used to indicate that this component is restricted to a specific branch of archetypes based on the provided base archetype.
      /// </summary>
      /// <seealso cref="Archetype.IComponent.IAmRestrictedToCertainTypes"/>
      /// <seealso cref="Archetype.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
      /// <seealso cref="IModel.IComponent.IAmRestrictedToCertainTypes"/>
      /// <seealso cref="IModel.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
      public new interface IAmRestrictedTo<TArchetypeBase>
        : XBam.IComponent.IAmRestrictedTo<TArchetypeBase>,
          Archetype.IComponent.IAmRestrictedToCertainTypes,
          IComponent
        where TArchetypeBase : Archetype {

        /// <summary>
        /// Check if this is compatable with an archetype
        /// </summary>
        bool Archetype.IComponent.IAmRestrictedToCertainTypes.IsCompatableWith(Archetype archetype)
          => archetype is TArchetypeBase;
      }

      /// <summary>
      /// Can be used to indicate that this component is restricted to a specific branch of archetypes based on the provided base archetype.
      /// Base functionality. Extend the generic version instead.
      /// Base Generic Type.
      /// </summary>
      /// <seealso cref="Archetype.IComponent.IAmRestrictedToCertainTypes"/>
      /// <seealso cref="Archetype.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
      /// <seealso cref="IModel.IComponent.IAmRestrictedToCertainTypes"/>
      /// <seealso cref="IModel.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
      public new interface IAmRestrictedToCertainTypes
        : XBam.IComponent.IAmRestrictedToCertainTypes,
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
