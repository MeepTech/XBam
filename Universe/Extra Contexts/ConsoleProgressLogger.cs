using Meep.Tech.Reflection;
using Meep.Tech.XBam.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Meep.Tech.XBam.Archetype;

namespace Meep.Tech.XBam.Logging.Configuration {

  /// <summary>
  /// Used to set up debugging and progress loading bars for xbam.
  /// </summary>
  public class ConsoleProgressLogger : Universe.ExtraContext {
    int? _overallStepsRemaining;
    int? _subProcessStepsRemaining;
    int? _totalCurrentSteps;
    int? _totalCurrentSubProcessSteps;

    const string DebuggerOutputPrefix = "XBAM:";

    const int NumberOfGenericLoaderMajorSteps
      = 2;

    const int LoaderModelTestingStepCountSize
      = 3;

    const int LoaderMajorStepCountSize
      = 1;

    const int LoaderModifierStepCountSize
      = 1;

    const int LoaderAssemblyStepCountSize
      = 1;

    const int LoaderCollectTypesStepCountSize
      = 1;

    /// <summary>
    /// Tells this to print full errors inline
    /// </summary>
    public bool VerboseModeForErrors { get; }

    /// <summary>
    /// Tells this to print non error messages more verbosely
    /// </summary>
    public bool VerboseModeForNonErrors { get; }

    /// <summary>
    /// The overall completeness of the whole loader program
    /// </summary>
    public float? OverallPercentComplete
      => _totalCurrentSteps.HasValue && _overallStepsRemaining.HasValue
        ? Math.Min(1f, Math.Max(0, (float)((float)((_totalCurrentSteps - _overallStepsRemaining)) / _totalCurrentSteps)))
        : null;

    /// <summary>
    /// The current sub process of the loader
    /// </summary>
    public string? CurrentSubProcessName {
      get;
      private set;
    } = "Initialization";

    /// <summary>
    /// The current completeness of the current sub process
    /// </summary>
    public float? CurrentSubProcessPercentComplete
      => _totalCurrentSubProcessSteps.HasValue && _subProcessStepsRemaining.HasValue
        ? Math.Min(1f, Math.Max(0, (float)(((float)(_totalCurrentSubProcessSteps - _subProcessStepsRemaining)) / _totalCurrentSubProcessSteps)))
        : null;

    ///<summary><inheritdoc/></summary>
    public ConsoleProgressLogger()
      : this(true, false) { }

    /// <summary>
    /// Make a special logger for the loader.
    /// </summary>
    public ConsoleProgressLogger(bool verboseModeForErrors, bool verboseModeForNonErrors = false) {
      VerboseModeForErrors = verboseModeForErrors;
      VerboseModeForNonErrors = verboseModeForNonErrors;
    }

    /// <summary>
    /// Used to write a standardize console message
    /// </summary>
    public void WriteMessage(string message, string? prefix = null, bool isError = false, Exception? exception = null, string? verboseNonErrorText = null) {
      string toWrite = (isError ? "!" : "") + DebuggerOutputPrefix;
      if (prefix != null) {
        toWrite += prefix + ":";
      }
      if (_overallStepsRemaining != null) {
        toWrite += Math.Round(OverallPercentComplete!.Value * 100).ToString() + "%:";
      } else {
        toWrite += "\t";
      }

      toWrite += "\t";

      if (_subProcessStepsRemaining != null) {
        toWrite += (CurrentSubProcessName is not null ? CurrentSubProcessName + " - " : "") + Math.Round(CurrentSubProcessPercentComplete!.Value * 100).ToString() + "%:";
        toWrite += "\t";
      }

      if (isError && VerboseModeForErrors) {
        toWrite += (" " + (isError ? "ERROR:" : "") + message + (exception is not null ? $"\n ERROR:\n{exception.ToString().Replace("\n\r", "\n").Replace("\n", "\n\t\t")}\n\n" : ""));
      } else {
        toWrite += message;
      }

      if (!string.IsNullOrWhiteSpace(verboseNonErrorText) && VerboseModeForNonErrors) {
        toWrite += ($"\n" + verboseNonErrorText).Replace("\n\r", "\n").Replace("\n", "\n\t\t");
      }

      Console.WriteLine(toWrite);
    }

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Loader> OnLoaderInitializationStart
      => loader => {
        WriteMessage($"Initializing a New XBam Loader.", "Loader", verboseNonErrorText: $"With settings: \n{JsonConvert.SerializeObject(Universe.Loader.Options, Formatting.Indented)}");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Universe> OnLoaderInitializationComplete
      => universe => {
        WriteMessage($"Initialized XBam Loader for Universe: {universe.Key}", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Universe>? OnLoaderDefaultExtraContextsInitializationComplete
      => universe => {
        if (universe.TryToGetExtraContext<Json.Configuration.IModelJsonSerializer>(out var autoBuilder)) {
          autoBuilder!.Options.OnLoaderModelJsonPropertyCreationComplete += (member, jsonProp) => {
            if (VerboseModeForNonErrors) {
              WriteMessage($"Added Json Property For Serialization: {jsonProp.PropertyName}, to Model: {member.DeclaringType.Name}.", "Serializer");
            }
          };
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<IEnumerable<Assembly>> OnLoaderAssembliesCollected
      => assemblies => {
        _overallStepsRemaining
          = _totalCurrentSteps
          = (assemblies.Count() * LoaderAssemblyStepCountSize)
            + (NumberOfGenericLoaderMajorSteps * LoaderMajorStepCountSize) + LoaderCollectTypesStepCountSize + LoaderModelTestingStepCountSize;

        WriteMessage($"Collected and Ordered Assemblies to Load Types from.", verboseNonErrorText: $"\tCount: {assemblies.Count()}\n\t - {string.Join("\n\t - ", assemblies.Select(a => a.FullName))}\n\n", prefix: "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderInitialSystemTypesCollected
      => () => {
        _overallStepsRemaining -= LoaderCollectTypesStepCountSize;
        WriteMessage($"Collected Initial Types to Load from the Ordered Assemblies", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Loader.AssemblyBuildableTypesCollection> OnLoaderAssemblyLoadStart
      => (assemblyTypes) => {
        CurrentSubProcessName = $"Load Types From Assembly - {assemblyTypes.Assembly.GetName().Name}";
        _subProcessStepsRemaining
          = _totalCurrentSubProcessSteps
          = assemblyTypes.Components.Count * 2
            + assemblyTypes.Enumerations.Count
            + assemblyTypes.Archetypes.Count
            + assemblyTypes.Models.Count * 2
            + ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType
              .Sum(x => x.Value.Sum(
                e => e.Value.Count));

        if (assemblyTypes.Modifications is not null) {
          _totalCurrentSteps += LoaderModifierStepCountSize;
        }

        WriteMessage($"Started Loading Assembly with {_subProcessStepsRemaining} Types.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<PropertyInfo> OnLoaderEnumInitializationStart
      => prop => {
        WriteMessage($"Started Loading Enum from Property: {prop.Name}, on class: {prop.DeclaringType.ToFullHumanReadableNameString()}, of type {prop.PropertyType.ToFullHumanReadableNameString()}.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, PropertyInfo, Enumeration?, Exception?> OnLoaderEnumInitializationComplete
      => (success, prop, @enum, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          WriteMessage($"Finished Loading Enum: {@enum}, from Property: {prop.Name}, on class: {prop.DeclaringType.ToFullHumanReadableNameString()}, of type {prop.PropertyType.ToFullHumanReadableNameString()}.", "Loader");
        }
        else {
          WriteMessage($"Failed to Load Enum from Property: {prop.Name}, on class: {prop.DeclaringType.ToFullHumanReadableNameString()}, of type {prop.PropertyType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Type> OnLoaderComponentInitializationStart
      => componentSystemType => {
        WriteMessage($"Started Loading Component from class: {componentSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Type> OnLoaderBuildTestComponentStart
      => componentSystemType => {
        WriteMessage($"Started Test-Building Component from class: {componentSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, Type, IComponent?, Exception?> OnLoaderBuildTestComponentComplete
      => (success, componentSystemType, component, exception) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          WriteMessage($"Successfully Test-Built Component with Key:{component!.Key}, from class: {componentSystemType.ToFullHumanReadableNameString()}", "Loader");
        } else {
          WriteMessage($"Failed to Test-Build Component from class: {componentSystemType.ToFullHumanReadableNameString()}", "Loader", true, exception);
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, Type, Exception?> OnLoaderComponentInitializationComplete
      => (success, componentSystemType, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          WriteMessage($"Finished Loading Component from class: {componentSystemType.ToFullHumanReadableNameString()}", "Loader");
        }
        else {
          WriteMessage($"Failed to Load Component from class: {componentSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Type> OnLoaderSimpleModelInitializationStart
      => modelSystemType => {
        WriteMessage($"Started Initializing Simple Model from class: {modelSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, Type, Exception?> OnLoaderSimpleModelInitializationComplete
      => (success, modelSystemType, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          WriteMessage($"Finished Initializing Simple Model from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader");
        }
        else {
          WriteMessage($"Failed to Initialize Simple Model from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Type, bool> OnLoaderArchetypeInitializationStart
      => (archetypeSystemType, isSplayedSubType) => {
        WriteMessage($"Started Initializing {(isSplayedSubType ? "A Splayed " : "")}Archetype from class: {archetypeSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, Type, Archetype?, Exception?, bool> OnLoaderArchetypeInitializationComplete
      => (success, archetypeSystemType, archetype, error, isSplayedSubType) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          if (archetype is not null) {
            WriteMessage($"Finished Initializing {(isSplayedSubType ? "Splayed " : "")}Archetype: {archetype.Id.Key}, from class: {archetypeSystemType.ToFullHumanReadableNameString()}.", "Loader");
          }
          else {
            WriteMessage($"Finished Initializing All Sub-Archetypes from class: {archetypeSystemType.ToFullHumanReadableNameString()}.", "Loader");
          }
        }
        else {
          WriteMessage($"Failed to Initialize {(isSplayedSubType ? "A Splayed " : "")}Archetype from class: {archetypeSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Type> OnLoaderModelFullInitializationStart
      => modelSystemType => {
        WriteMessage($"Started Initializing Model Fully from class: {modelSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Type> OnLoaderModelFullRegistrationStart
      => modelSystemType => {
        WriteMessage($"Started Registering Model from class: {modelSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, Type, Exception?> OnLoaderModelFullRegistrationComplete
      => (success, modelSystemType, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          WriteMessage($"Finished Registering Model from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader");
        }
        else {
          WriteMessage($"Failed to Registering Model from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, Type, Exception?> OnLoaderModelFullInitializationComplete
      => (success, modelSystemType, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          WriteMessage($"Finished Initializing Model Fully from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader");
        }
        else {
          WriteMessage($"Failed to Initialize Model Fully from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Loader.AssemblyBuildableTypesCollection> OnLoaderAssemblyLoadComplete
      => (assemblyTypes) => {
        CurrentSubProcessName = null;
        _subProcessStepsRemaining = null;
        _overallStepsRemaining -= LoaderAssemblyStepCountSize;

        WriteMessage($"Finished Loading Assembly!", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderTypesInitializationFirstRunComplete
      => () => WriteMessage($"First Initialization Attempt Run on all Assemblies Complete!", "Loader");

    ///<summary><inheritdoc/></summary>
    protected internal override Action<int> OnLoaderFurtherAnizializationAttemptStart
      => runNumber => {
        _subProcessStepsRemaining = 0;
        CurrentSubProcessName = $"Further Initialization Attempt #: {runNumber - 1}";
        _subProcessStepsRemaining += Universe.Loader._uninitializedEnums.Any() ? Universe.Loader._uninitializedEnums.Count : 0;
        _subProcessStepsRemaining += Universe.Loader._uninitializedArchetypes.Any() ? Universe.Loader._initializedArchetypes.Count : 0;
        _subProcessStepsRemaining += Universe.Loader._uninitializedComponents.Any() ? Universe.Loader._uninitializedComponents.Count : 0;
        _subProcessStepsRemaining += Universe.Loader._uninitializedModels.Any() ? Universe.Loader._uninitializedModels.Count : 0;
        WriteMessage($"Running Initialization Attempt #{runNumber} for {Universe.Loader.UninitializedTypesCount} remaining Uninintalized Types.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<int> OnLoaderFurtherInizializationAttemptComplete
      => runNumber => {
        _subProcessStepsRemaining = 0;
        CurrentSubProcessName = null;
        WriteMessage($"Finished Initialization Attempt #{runNumber} with {Universe.Loader.UninitializedTypesCount} remaining Uninintalized Types.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderTypesInitializationAllRunsComplete
      => () => {
        _overallStepsRemaining -= LoaderMajorStepCountSize;
        WriteMessage($"Finished all Loader Initialization Attempts with {Universe.Loader.UninitializedTypesCount} remaining Uninintalized Types.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<int> OnLoaderBuildAllTestModelsStart
      => archetypesToTestCount => {
        _subProcessStepsRemaining = archetypesToTestCount;
        CurrentSubProcessName = $"Building Test Models";
        WriteMessage($"Building Test Models for {archetypesToTestCount} Initialized Archetypes.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Archetype> OnLoaderTestModelBuildStart
      => archetypeToBuildTestModelFor => WriteMessage($"Building Test Model for Archetype: {archetypeToBuildTestModelFor}.", "Loader");

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, Archetype, IModel?, Exception?> OnLoaderTestModelBuildComplete
      => (success, archetypeToTestBuildModelFor, testBuiltModel, exception) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          WriteMessage($"Finished Building Test Model of type {testBuiltModel!.GetType()} for Archetype: {archetypeToTestBuildModelFor}: {testBuiltModel}", "Loader");
        }
        else {
          WriteMessage($"Failed to Build Test Model for Archetype: {archetypeToTestBuildModelFor}.", "Loader", true, exception);
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderBuildAllTestModelsComplete
      => () => {
        _subProcessStepsRemaining = null;
        CurrentSubProcessName = null;
        _overallStepsRemaining -= LoaderModelTestingStepCountSize;
        WriteMessage($"Finished Building Test Models for Initialized Archetypes.", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<IEnumerable<Type>> OnLoaderAllModificationsStart
      => modifierTypes => {
        _subProcessStepsRemaining = modifierTypes.Count();
        CurrentSubProcessName = $"Loading Modifiers";
        WriteMessage(message: $"Loading and Running Modifier Types in Assembly Load Order.", verboseNonErrorText: $"{string.Join("\n\t - ", modifierTypes.Select(m => m.Name))}\n\n", prefix: "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Type> OnLoaderModificationStart
      => modType => WriteMessage($"Starting Initialization for Modifier of Type: {modType.Name} from Assembly: {modType.Assembly.GetName().Name}", "Loader");

    ///<summary><inheritdoc/></summary>
    protected internal override Action<bool, Type, Modifications, Exception?> OnLoaderModificationComplete
      => (success, modType, mod, error) => {
        _subProcessStepsRemaining -= 1;
        _overallStepsRemaining -= LoaderModifierStepCountSize;

        if (success) {
          WriteMessage($"Finished Initializing and Running Modifier of Type: {modType.Name}, from Assembly: {modType.Assembly.GetName().Name}.", "Loader");
        } else {
          WriteMessage($"Failed to Initialize and Run Modifier of Type: {modType.Name}, from Assembly: {modType.Assembly.GetName().Name}.", "Loader", true, error);
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderAllModificationsComplete
      => () => {
        _subProcessStepsRemaining = null;
        CurrentSubProcessName = null;
        WriteMessage($"Finished Initializing and Loading Modifier Classes", "Loader");
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderFinishTypesStart
      => () => WriteMessage($"Starting the Process of Finishing {Universe.Loader._initializedArchetypes.Count()} Archetypes", "Loader");

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderFinishTypesComplete
      => () => {
        _overallStepsRemaining -= LoaderMajorStepCountSize;
        WriteMessage($"Finished Loading {Universe.Loader._finishedArchetypes.Count()} Archetypes with: {Universe.Loader._initializedArchetypes.Count()} Types Unfinished.", "Loader", verboseNonErrorText: string.Join("\n\t - ", Universe.Loader._initializedArchetypes));
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderFinalizeStart
      => () => WriteMessage($"Starting the Process of Finalizing the XBam Loader for Universe: {Universe.Loader.Universe.Key}.", "Loader");

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderFinalizeComplete
      => () => {
        _overallStepsRemaining = 0;

        WriteMessage($"Finalized the XBam Loader for Universe: {Universe.Loader.Universe.Key}.", "Loader");
        if (VerboseModeForErrors && Universe.Loader.Failures.Any()) {
          WriteMessage($"Failed To Initialize The Following {Universe.Loader.Failures.Count()} Types:", isError: true, prefix: "Loader");
          foreach(Loader.Failure failure in Universe.Loader.Failures) {
            if (failure.XbamType == "Enumeration") {
              Console.Error.WriteLine($"Could not initialize Enumeration of Type: {failure.SystemType.ToFullHumanReadableNameString()} on Property with Name:{((PropertyInfo)failure.Metadata).Name}, on Type: {((PropertyInfo)failure.Metadata).DeclaringType.ToFullHumanReadableNameString()}, due to Exception:\n\n{failure.Exception}");
            }
            else {
              Console.Error.WriteLine($"Could not initialize {failure.XbamType} Type: {failure.SystemType.ToFullHumanReadableNameString()}, due to Exception:\n\n{failure.Exception}");
            }
          }
        }
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action OnLoaderIsFinished
      => () => {
        _overallStepsRemaining = null;

        WriteMessage($"XBam Loader for Universe: {Universe.Loader.Universe.Key} Finished and is now Sealed.", "Loader", verboseNonErrorText:
          $"Enumerations Loaded:\n\t - "
            + string.Join($"\n\t - ", Universe.Loader.Universe.Enumerations.ByType.SelectMany(bt => bt.Value))
          + $"\nArchetypes Loaded:\n\t - "
            + string.Join($"\n\t - ", Universe.Loader.Universe.Archetypes.All.All)
          + $"\nModel Types Loaded:\n\t - "
            + string.Join($"\n\t - ", Universe.Loader.Universe.Models.All.Select(t => t.FullName))
          + $"\nComponent Types Loaded:\n\t - "
            + string.Join($"\n\t - ", Universe.Loader.Universe.Components.All.Select(t => t.FullName))
        );
      };

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Archetype> OnUnloadArchetype
      => archetype => {
        WriteMessage($"Unloaded Archetype of Type:{archetype.GetType().FullName}.", "UnLoader");
      };
  }
}
