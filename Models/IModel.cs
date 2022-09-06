using Meep.Tech.XBam.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.XBam {

  /// <summary>
  /// The base interface for all XBam Models.
  /// A Model is a mutable grouping of data fields that can be produced by an Archetype.
  /// This is the non generic for Utility only, don't inherit from this direclty; Extend IModel[], Model[], or Model[,], or  Model[,].IFromInterface instead.
  /// </summary>
  /// <see cref="IModel{TModelBase}"/>
  /// <see cref="Model{TModelBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}.IFromInterface"/>
  public partial interface IModel: IResource {
    Universe IResource.Universe => Universe;

    /// <summary>
    /// The factory or archetype used to build this model
    /// </summary>
    public XBam.IFactory Factory { get; }

    /// <summary>
    /// The universe this model was made in
    /// </summary>
    public new Universe Universe {
      get;
      internal set;
    }

    /// <summary>
    /// Initializes the universe, archetype, factory and other built in model links.
    /// To set the archetype of a model in a custom public constructor, call this method.
    /// </summary>
    protected internal IModel OnInitialized([NotNull] Archetype factoryArchetype, Universe? universe = null, XBam.IBuilder? builder = null)
      => this;

    /// <summary>
    /// Can be overriden to finalize the model on build.
    /// </summary>
    protected internal IModel OnFinalized(XBam.IBuilder? builder)
      => this;
  }

  /// <summary>
  /// The base interface for all 'Simple' XBam Models.
  /// A Model is a mutable grouping of data fields that can be produced by an Archetype.
  /// Simple Models have a single non-branching Archetype called a BuilderFactory.
  /// </summary>
  /// <see cref="Model{TModelBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}.IFromInterface"/>
  public partial interface IModel<TModelBase>
    : IModel
    where TModelBase : IModel<TModelBase>
  {

    /// <summary>
    /// For the base configure calls
    /// </summary>
    IModel IModel.OnInitialized(Archetype archetype, Universe? universe, XBam.IBuilder? builder) {
      Universe = universe ?? builder?.Archetype.Universe ?? Universe.Default;

      return this;
    }
  }

  /// <summary>
  /// The base interface for all Branchable XBam Models.
  /// A Model is a mutable grouping of data fields that can be produced by an Archetype.
  /// Brnachable XBam models can have branching inheritance trees for both the Archetypes that produce the Models, and the Models produced.
  /// This is the nbase interface for utility only, don't inherit from this directly; Extend IModel[], Model[], or Model[,], or  Model[,].IFromInterface instead.
  /// </summary>
  /// <see cref="IModel{TModelBase}"/>
  /// <see cref="Model{TModelBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}"/>
  /// <see cref="Model{TModelBase, TArchetypeBase}.IFromInterface"/>
  public interface IModel<TModelBase, TArchetypeBase>
    : IModel<TModelBase>
    where TModelBase : IModel<TModelBase, TArchetypeBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// Default collection of archetypes for this model type based on the Default Univese
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.Collection Types
      => (Archetype<TModelBase, TArchetypeBase>.Collection)
        Archetypes.DefaultUniverse.Archetypes.GetCollection(typeof(TArchetypeBase));

    /// <summary>
    /// The archetype for this model
    /// </summary>
    public TArchetypeBase Archetype {
      get;
      internal set;
    }

    XBam.IFactory IModel.Factory
      => Archetype;

    /// <summary>
    /// For the base configure calls
    /// </summary>
    IModel IModel.OnInitialized([NotNull] Archetype archetype, Universe? universe, XBam.IBuilder? builder) {
      Archetype = (TArchetypeBase)(archetype ?? throw new ArgumentNullException(nameof(Archetype)));
      Universe = universe ?? Archetype.Universe ?? Universe.Default;

      return this;
    }
  }
}
