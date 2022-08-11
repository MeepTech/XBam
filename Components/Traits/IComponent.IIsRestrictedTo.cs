namespace Meep.Tech.XBam {
  public partial interface IComponent {

    /// <summary>
    /// Can be used to indicate that this component is restricted to a specific branch of models or archetypes based on the provided base type.
    /// </summary>
    /// <seealso cref="Archetype.IComponent.IIsRestrictedToCertainTypes"/>
    /// <seealso cref="Archetype.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
    /// <seealso cref="IModel.IComponent.IIsRestrictedToCertainTypes"/>
    /// <seealso cref="IModel.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
    public interface IIsRestrictedToCertainTypes
      : IComponent {

      /// <summary>
      /// The base type this component is restricted to use with.
      /// </summary>
      public virtual System.Type RestrictedTo
        => null;
    }

    /// <summary>
    /// Can be used to indicate that this component is restricted to a specific branch of models or archetypes based on the provided base type.
    /// </summary>
    /// <seealso cref="Archetype.IComponent.IIsRestrictedToCertainTypes"/>
    /// <seealso cref="Archetype.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
    /// <seealso cref="IModel.IComponent.IIsRestrictedToCertainTypes"/>
    /// <seealso cref="IModel.IComponent.IIsRestrictedTo{TArchetypeBase}"/>
    public interface IIsRestrictedTo<TRestrictionBase>
      : IIsRestrictedToCertainTypes {

      /// <summary>
      /// The base type this component is restricted to use with.
      /// </summary>
      System.Type XBam.IComponent.IIsRestrictedToCertainTypes.RestrictedTo
       => typeof(TRestrictionBase);
    }
  }
}
