namespace Meep.Tech.XBam {
  public partial interface IComponent {

    /// <summary>
    /// Can be used to indicate that this component is restricted to a specific branch of models or archetypes based on the provided base type.
    /// </summary>
    /// <seealso cref="Archetype.IComponent.IAmRestrictedToCertainTypes"/>
    /// <seealso cref="Archetype.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
    /// <seealso cref="IModel.IComponent.IAmRestrictedToCertainTypes"/>
    /// <seealso cref="IModel.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
    public interface IAmRestrictedToCertainTypes
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
    /// <seealso cref="Archetype.IComponent.IAmRestrictedToCertainTypes"/>
    /// <seealso cref="Archetype.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
    /// <seealso cref="IModel.IComponent.IAmRestrictedToCertainTypes"/>
    /// <seealso cref="IModel.IComponent.IAmRestrictedTo{TArchetypeBase}"/>
    public interface IAmRestrictedTo<TRestrictionBase>
      : IAmRestrictedToCertainTypes {

      /// <summary>
      /// The base type this component is restricted to use with.
      /// </summary>
      System.Type XBam.IComponent.IAmRestrictedToCertainTypes.RestrictedTo
       => typeof(TRestrictionBase);
    }
  }
}
