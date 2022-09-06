using KellermanSoftware.CompareNetObjects;
using Meep.Tech.Collections.Generic;
using Meep.Tech.XBam.Cloning;
using Meep.Tech.XBam.Cloning.Configuration;
using Meep.Tech.XBam.Json.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Meep.Tech.XBam.Configuration {

  public partial class Loader {

    /// <summary>
    /// Settings for the loader
    /// </summary>
    public partial class Settings {

      /// <summary>
      /// Assemblies that should be included in the loading that are built in.
      /// This helps prevent assemblies from not being loaded yet on initial searches.
      /// Assemblies here are included even if they don't match naming validations.
      /// </summary>
      [JsonIgnore]
      public List<Assembly> PreLoadAssemblies {
        get;
        set;
      } = new List<Assembly>();

      /// <summary>
      /// Assemblies that should be ignored in the loading 
      /// </summary>
      [JsonIgnore]
      public List<Assembly> IgnoredAssemblies {
        get;
        set;
      } = new List<Assembly>();

      /// <summary>
      /// If a single archetype not being initialized should throw a fatal.
      /// </summary>
      public bool FatalOnCannotInitializeType {
        get;
        set;
      } = false;

      /// <summary>
      /// If any archetypes not being initialized by the time we get to finalize should throw a fatal.
      /// </summary>
      public bool FatalDuringFinalizationOnCouldNotInitializeTypes {
        get;
        set;
      } = false;

      /// <summary>
      /// The prefix to limit assemblies to for loading archetypes
      /// </summary>
      public string ArchetypeAssembliesPrefix {
        get;
        set;
      } = "";

      /// <summary>
      /// The assembly name prefixes to ignore when loading types from assemblies
      /// </summary>
      public List<string> AssemblyPrefixesToIgnore {
        get;
        set;
      } = new List<string> {
        "System.",
        "Microsoft.",
        "WinRT.Runtime",
        "Newtonsoft.",
        "netstandard",
        "NuGet.",
        "KellermanSoftware.",
        "Meep.Tech.Data"
      };

      /// <summary>
      /// How many times to re-run initialization to account for types that require others
      /// </summary>
      public short InitializationAttempts {
        get;
        set;
      } = 10;

      /// <summary>
      /// How many times to re-run models that failed to be test built to account for types that require others
      /// </summary>
      public short ModelTestBuildAttempts {
        get;
        set;
      } = 10;

      /// <summary>
      /// How many times to attempt to run finalization on remaining initializing types
      /// </summary>
      public short FinalizationAttempts {
        get;
        set;
      } = 1;

      /// <summary>
      /// Overrideable bool to allow runtime registrations of types that set AllowSubtypeRuntimeRegistrations to true.
      /// </summary>
      public bool AllowRuntimeTypeRegistrations {
        get;
        set;
      } = false;

      /// <summary>
      /// If true (is true by default), this tells the loader to pre-load all assemblies referenced by the one currently running the loader.
      /// </summary>
      public bool PreLoadAllReferencedAssemblies {
        get;
        set;
      } = true;

      /// <summary>
      /// The folder containing the /data/ folder.
      /// </summary>
      public string DataFolderParentFolderLocation {
        get;
        set;
      } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

      /// <summary>
      /// The name to configure for the current universe.
      /// This will be used as it's unique key in the db
      /// </summary>
      public string? UniverseName {
        get;
        set;
      }

      /// <summary>
      /// If this universe registers lazily initialized enums.
      /// </summary>
      public bool AllowLazyEnumerationRegistration {
        get;
        init;
      } = true;

      /// <summary>
      /// A pre-settable setting for specifying how to order certain mods for loading.
      /// This will throw if there's a conflict with order.json
      /// </summary>
      [JsonIgnore]
      public Map<ushort, string> PreOrderedAssemblyFiles {
        get;
        set;
      } = new Map<ushort, string>();

      /// <summary>
      /// The default model serializer options
      /// </summary>
      [JsonIgnore]
      public Dictionary<
        System.Type,
        (
          Func<Universe.ExtraContext.Settings?, Universe.ExtraContext> contextConstructor,
          Func<Universe, Universe.ExtraContext.Settings?> optionsConstructor
        )
      > DefaultExtraContexts {
        get;
      } = new() {
        {
          typeof(ModelJsonSerializerContext),
          (
            new(o => new ModelJsonSerializerContext((ModelJsonSerializerContext.Settings?)o)),
            u => new ModelJsonSerializerContext.Settings()
          )
        },{
          typeof(ModelCopyContext),
          (
            new(o => new ModelCopyContext((ModelCopyContext.Settings?)o)),
            u => new ModelCopyContext.Settings()
          )
        },{
          typeof(AutoBuilderContext),
          (
            new(o => new AutoBuilderContext()),
            u => null
          )
        }
      };


      /// <summary>
      /// The default config used to compare model objects
      /// </summary>
      public ComparisonConfig DefaultComparisonConfig {
        get;
        set;
      } = new ComparisonConfig {
        AttributesToIgnore = new List<Type> {
            typeof(ModelComponentsProperty)
          },
        IgnoreObjectTypes = true
#if DEBUG
        ,
        DifferenceCallback = x => {
          if (System.Diagnostics.Debugger.IsAttached) {
            System.Diagnostics.Debugger.Break();
          }
        }
#endif
      };

      /// <summary>
      /// Delegates used to try to re-targed the model type when the test build of a model type fails inside of Loader._tryToCalculateAcurateBuilderType.
      /// Indexed by the exception type.
      /// </summary>
      public Dictionary<System.Type, TestModelBuilderTypeMismatchExceptionHandler> TestModelBuilderTypeMismatchExceptionHandlers {
        get;
      } = new();
    }
  }
}