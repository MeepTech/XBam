using Meep.Tech.XBam.Configuration;
using Meep.Tech.Noise;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Meep.Tech.XBam {

  public partial class Universe {

    /// <summary>
    /// A Type that can be added as context to a universe.
    /// </summary>
    public class ExtraContext : IExtraUniverseContextType {
      Universe _universe = null!;

      /// <summary>
      /// Settings base class for an extra context
      /// </summary>
      public abstract class Settings {

        /// <summary>
        /// The Universe these settings belong to.
        /// </summary>
        public Universe Universe {
          get;
          internal set;
        } = null!;
      }

      ///<summary><inheritdoc/></summary>
      public string Id {
        get;
      } = RNG.GenerateNextGuid();

      ///<summary><inheritdoc/></summary>
      public Universe Universe {
        get => _universe;
        internal set {
          _universe = value;
          if (Options is not null) {
            Options.Universe = value;
          }
        }
      }

      /// <summary>
      /// The options for this extra context
      /// </summary>
      public Settings? Options {
        get;
        protected set;
      }

      /// <summary>
      /// Used to make new extra contexts.
      /// </summary>
      public ExtraContext(Settings? options = null) {
        Options = options;
      }

      #region Overrideable Loader Step Callbacks

      /// <summary>
      /// Code that's executed before initialization of the loader
      /// </summary>
      internal protected virtual Action<Configuration.Loader>? OnLoaderInitializationStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed after initialization of the loader, before any types are loaded
      /// </summary>
      internal protected virtual Action<Universe>? OnLoaderInitializationComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed before initialization of the model serializer
      /// </summary>
      internal protected virtual Action<Universe>? OnLoaderDefaultExtraContextsInitializationStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed after  initialization of the model serializer
      /// </summary>
      internal protected virtual Action<Universe>? OnLoaderDefaultExtraContextsInitializationComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed after the loader collects the initial assemblies to work on
      /// </summary>
      internal protected virtual Action<IEnumerable<Assembly>>? OnLoaderAssembliesCollected { get; protected set; } = null;

      /// <summary>
      /// Code that's executed after the loader collects the initial system types to work on
      /// </summary>
      internal protected virtual Action? OnLoaderInitialSystemTypesCollected { get; protected set; } = null;

      /// <summary>
      /// Code that's executed before an aseembly's types are loaded
      /// </summary>
      internal protected virtual Action<Configuration.Loader.AssemblyBuildableTypesCollection>? OnLoaderAssemblyLoadStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed after an aseembly's types are loaded
      /// </summary>
      internal protected virtual Action<Configuration.Loader.AssemblyBuildableTypesCollection>? OnLoaderAssemblyLoadComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an enumeration is about to be initialized.
      /// </summary>
      internal protected virtual Action<PropertyInfo>? OnLoaderEnumInitializationStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an enumeration is done being initialized.
      /// the first bool is if it was successful or not.
      /// </summary>
      internal protected virtual Action<bool, PropertyInfo, Enumeration?, Exception?>? OnLoaderEnumInitializationComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an component is about to be initialized.
      /// </summary>
      internal protected virtual Action<System.Type>? OnLoaderComponentInitializationStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an component is about to be test built.
      /// </summary>
      internal protected virtual Action<System.Type>? OnLoaderBuildTestComponentStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an component is done being test built.
      /// </summary>
      internal protected virtual Action<bool, System.Type, XBam.IComponent?, Exception?>? OnLoaderBuildTestComponentComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an component is done being initialized.
      /// the first bool is if it was successful or not.
      /// </summary>
      internal protected virtual Action<bool, System.Type, Exception?>? OnLoaderComponentInitializationComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an simple model is about to be initialized.
      /// </summary>
      internal protected virtual Action<System.Type>? OnLoaderSimpleModelInitializationStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an simple model is about to be initialized.
      /// </summary>
      internal protected virtual Action<bool, System.Type, Exception?>? OnLoaderSimpleModelInitializationComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an archetype is about to be initialized.
      /// the bool indicates if it's a splayed sub-type.
      /// </summary>
      internal protected virtual Action<System.Type, bool>? OnLoaderArchetypeInitializationStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when an archetype is about to be initialized.
      /// the first bool indicates success. The second bool indicates if it's a splayed or sub-type that shares a class with a parent.
      /// </summary>
      internal protected virtual Action<bool, System.Type, Archetype?, Exception?, bool>? OnLoaderArchetypeInitializationComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when a model is about to be initialized.
      /// </summary>
      internal protected virtual Action<System.Type>? OnLoaderModelFullInitializationStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed before final initialization but after registration of a new type of model.
      /// Default model is null if the model type is generic and can't be tested.
      /// </summary>
      internal protected virtual Action<Type>? OnLoaderModelFullRegistrationStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed before final initialization but after registration of a new type of model.
      /// Default model is null if the model type is generic and can't be tested.
      /// </summary>
      internal protected virtual Action<bool, Type, Exception?>? OnLoaderModelFullRegistrationComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when a model is about to be initialized.
      /// </summary>
      internal protected virtual Action<bool, System.Type, Exception?>? OnLoaderModelFullInitializationComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed after all models, enums, components and archetypes are initialized the first time.
      /// </summary>
      internal protected virtual Action? OnLoaderTypesInitializationFirstRunComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed if there's further initialization attempt loops needed because there are uninitialized types.
      /// This happens if types are missing dependencies the first time their assembly is loaded.
      /// The passed in value is the attempt number, starting at 2, as it's the second attempt.
      /// See OnLoaderAssemblyLoadComplete and OnLoaderTypesInitializationFirstRunComplete for the first load attempt
      /// </summary>
      internal protected virtual Action<int>? OnLoaderFurtherAnizializationAttemptStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed if there's further initialization attempt loops needed because there are uninitialized types.
      /// This happens if types are missing dependencies the first time their assembly is loaded.
      /// The passed in value is the attempt number, starting at 2, as it's the second attempt.
      /// See OnLoaderAssemblyLoadComplete and OnLoaderTypesInitializationFirstRunComplete for the first load attempt
      /// </summary>
      internal protected virtual Action<int>? OnLoaderFurtherInizializationAttemptComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed after all models, enums, components and archetypes are initialized for all looping run attempts.
      /// </summary>
      internal protected virtual Action? OnLoaderTypesInitializationAllRunsComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed before test models start being built
      /// </summary>
      internal protected virtual Action<int>? OnLoaderBuildAllTestModelsStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed before a test model is built for an archetype.
      /// </summary>
      internal protected virtual Action<Archetype>? OnLoaderTestModelBuildStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when a test model is done being built for an archetype.
      /// </summary>
      internal protected virtual Action<bool, Archetype, IModel?, Exception?>? OnLoaderTestModelBuildComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed after all test models have been built
      /// </summary>
      internal protected virtual Action? OnLoaderBuildAllTestModelsComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when modifications are about to be loaded in assembly order
      /// </summary>
      internal protected virtual Action<IEnumerable<System.Type>>? OnLoaderAllModificationsStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when a modification is about to be loaded
      /// </summary>
      internal protected virtual Action<System.Type>? OnLoaderModificationStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when a modification finishes loading or fails to load
      /// </summary>
      internal protected virtual Action<bool, System.Type, Modifications?, Exception?>? OnLoaderModificationComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed when modifications are done loading, before all types are finalized.
      /// </summary>
      internal protected virtual Action? OnLoaderAllModificationsComplete { get; protected set; } = null;

      /// <summary>
      /// Executed each time the ctor is set on an archetype.
      /// May be executed more than once for the same archetype.
      /// </summary>
      internal protected virtual Action<Archetype>? OnLoaderArchetypeModelConstructorSetStart { get; protected set; } = null;

      /// <summary>
      /// Executed each time the ctor is set on an archetype.
      /// May be executed more than once for the same archetype.
      /// </summary>
      internal protected virtual Action<Archetype>? OnLoaderArchetypeModelConstructorSetComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed on finalization of the loader, after all types are already finalized and before the loader is sealed.
      /// </summary>
      internal protected virtual Action? OnLoaderFinishTypesStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed on finalization of the loader, after all types are already finalized and before the loader is sealed.
      /// </summary>
      internal protected virtual Action? OnLoaderFinishTypesComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed on finalization of the loader, after all types are already finalized and before the loader is sealed.
      /// </summary>
      internal protected virtual Action? OnLoaderFinalizeStart { get; protected set; } = null;

      /// <summary>
      /// Code that's executed on finalization of the loader, after all types are already finalized and before the loader is sealed.
      /// </summary>
      internal protected virtual Action? OnLoaderFinalizeComplete { get; protected set; } = null;

      /// <summary>
      /// Code that's executed lastly after the loader is finished and cleared.
      /// </summary>
      internal protected virtual Action? OnLoaderIsFinished { get; protected set; } = null;

      /// <summary>
      /// Occurs when an archetype is un-loaded.
      /// </summary>
      internal protected virtual Action<Archetype>? OnUnloadArchetype { get; protected set; } = null;

      #endregion

      #region Helpers

      /// <summary>
      /// Helper function.
      /// Get any builder.
      /// </summary>
      protected IBuilder GetBuilder(Archetype archetype)
        => (archetype as IBuilderSource).Build();

      /// <summary>
      /// Add the given components to the given archetypes
      /// </summary>
      protected void AddComponentsToArchetypes(IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components)
        => Modifications._addComponentsToArchetypes(Universe, archetypes, components);

      /// <summary>
      /// Add the given initial model components to the given archetypes
      /// </summary>
      protected void AddInitialModelComponentsToArchetypes(IEnumerable<Archetype> archetypes, Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> componentBuilders)
        => Modifications._addInitialModelComponentsToArchetypes(Universe, archetypes, componentBuilders);

      /// <summary>
      /// Remove the given components from the given archetypes
      /// </summary>
      protected void RemoveComponentsFromArchetypes(IEnumerable<Archetype> archetypes, params string[] componentKeys)
        => Modifications._removeComponentsFromArchetypes(Universe, archetypes, componentKeys);

      /// <summary>
      /// Remove the given components from the given archetypes
      /// </summary>
      protected void RemoveInitialModelComponentsFromArchetypes(IEnumerable<Archetype> archetypes, params string[] componentKeys)
        => Modifications._removeInitialModelComponentsFromArchetypes(Universe, archetypes, componentKeys);

      /// <summary>
      /// Update the existing component from the given archetypes.
      /// </summary>
      protected void UpdateComponentsForArchetypes(Dictionary<string, Archetype.IComponent> updateComponentsByType, params Archetype[] archetypes)
        => Modifications._updateComponentsForArchetypes(
          Universe,
          updateComponentsByType
            .ToDictionary(e => e.Key, e => new Func<Archetype.IComponent, Archetype.IComponent>(_ => e.Value)),
          archetypes
        );

      /// <summary>
      /// Update the existing component from the given archetypes.
      /// </summary>
      protected void UpdateComponentsForArchetypes(Dictionary<string, Func<Archetype.IComponent, Archetype.IComponent>> updateComponentsByType, params Archetype[] archetypes)
        => Modifications._updateComponentsForArchetypes(Universe, updateComponentsByType, archetypes);

      /// <summary>
      /// Update or add the given component from the given archetypes.
      /// </summary>
      protected void UpdateOrAddComponentsForArchetypes(IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components)
        => Modifications._updateOrAddComponentsForArchetypes(Universe, archetypes, components);

      /// <summary>
      /// Update or add the given component from the given archetypes.
      /// </summary>
      protected void UpdateOrAddComponentsForArchetypes(Dictionary<string, Func<Archetype.IComponent?, Archetype.IComponent>> updateComponentsByKey, params Archetype[] archetypes)
        => Modifications._updateOrAddComponentsForArchetypes(Universe, updateComponentsByKey: updateComponentsByKey, archetypes);

      /// <summary>
      /// Update the existing component from the given archetypes.
      /// </summary>
      protected void UpdateInitialModelComponentsForArchetypes(Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> updateComponentsByType, params Archetype[] archetypes)
        => Modifications._updateInitialModelComponentsForArchetypes(
          Universe,
          updateComponentsByType,
          archetypes
        );

      /// <summary>
      /// Update or add the given component from the given archetypes.
      /// </summary>
      protected void UpdateOrAddInitialModelComponentsForArchetypes(IEnumerable<Archetype> archetypes, Dictionary<string, Func<IComponent.IBuilder, IModel.IComponent>> newComponentInitializers)
        => Modifications._updateOrAddInitialModelComponentsForArchetypes(Universe, newComponentInitializers, archetypes.ToArray());

      /// <summary>
      /// Update the existing component from the given archetypes.
      /// </summary>
      protected void UpdateInitialModelComponentsForArchetypes(Dictionary<string, Func<Func<IComponent.IBuilder, IModel.IComponent>, IComponent.IBuilder, IModel.IComponent>> updateComponentsByType, params Archetype[] archetypes)
        => Modifications._updateInitialModelComponentsForArchetypes(
          Universe,
          updateComponentsByType,
          archetypes
        );

      /// <summary>
      /// Update or add the given component from the given archetypes.
      /// </summary>
      protected void UpdateOrAddInitialModelComponentsForArchetypes(IEnumerable<Archetype> archetypes, Dictionary<string, Func<Func<IComponent.IBuilder, IModel.IComponent>, IComponent.IBuilder, IModel.IComponent>> newComponentInitializers)
        => Modifications._updateOrAddInitialModelComponentsForArchetypes(Universe, newComponentInitializers, archetypes.ToArray());

      #endregion

      ///<summary><inheritdoc/></summary>
      public override bool Equals(object? obj)
        => obj is ExtraContext context
          && context.GetType() == GetType()
          && (context.Id == Id);

      ///<summary><inheritdoc/></summary>
      public override int GetHashCode()
        => Id.GetHashCode();
    }
  }
}
