using Meep.Tech.Collections.Generic;
using Meep.Tech.Reflection;
using Meep.Tech.XBam.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using static Meep.Tech.XBam.Archetype;

namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Loads archetypes.
  /// </summary>
  public sealed partial class Loader {
    internal Dictionary<Type, Dictionary<Type, IModel>> _testModels;
    Dictionary<Archetype, Type> _loadedTestParams;
    List<Type> _initializedTypes;
    List<Assembly> _assemblyLoadOrder;
    List<Failure> _failures;
    HashSet<Archetype> _successfullyTestedArchetypes;

    static readonly FieldInfo _archetypeUniverseBackingField
      = typeof(Archetype).GetField("<Universe>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>
    /// The assembly types that will be built in order
    /// </summary>
    OrderedDictionary<Assembly, AssemblyBuildableTypesCollection> _assemblyTypesToBuild;

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<Type, Exception> _failedArchetypes;

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<MemberInfo, Exception> _failedEnumerations;

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<Type, Exception> _failedModels;

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<Type, Exception> _failedComponents;

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    internal Dictionary<Type, Exception> _uninitializedArchetypes;

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    internal Dictionary<Type, Exception> _uninitializedModels;

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    internal Dictionary<Type, Exception> _uninitializedComponents;

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    internal Dictionary<PropertyInfo, Exception> _uninitializedEnums;

    /// <summary>
    /// The types that have been constructed and still need model data mapped to them.
    /// </summary>
    internal HashSet<Archetype> _initializedArchetypes;

    /// <summary>
    /// The types that have been finished.
    /// </summary>
    internal HashSet<Archetype> _finishedArchetypes;

    /// <summary>
    /// How many initalization attempts are remaining
    /// </summary>
    int _remainingInitializationAttempts;

    /// <summary>
    /// How many finalization attempts are remaining
    /// </summary>
    int _remainingFinalizationAttempts;

    /// <summary>
    /// Externally fetched assemblies for loading
    /// </summary>
    List<Assembly> _unorderedAssembliesToLoad;

    /// <summary>
    /// The assemblies from Options.PreOrderedAssemblyFiles along with order.json combined and ready to use
    /// </summary>
    Map<ushort, string> _orderedAssemblyFiles;

    /// <summary>
    /// The specified settings for this loader
    /// </summary>
    public Settings Options {
      get;
    }

    /// <summary>
    /// The universe this loader creates
    /// </summary>
    public Universe Universe {
      get;
      internal set;
    }

    /// <summary>
    /// If all archetypes have been initialized and the loader is finished.
    /// Once this is true, you cannot modify archetypes or their collections anymore.
    /// </summary>
    public bool IsFinished {
      get;
      private set;
    } = false;

    /// <summary>
    /// Types that failed to initialize and their exceptions.
    /// </summary>
    public IReadOnlyList<Failure> Failures 
      => _failures;

    /// <summary>
    /// Assembled mod load order.
    /// </summary>
    public IReadOnlyList<Assembly> AssemblyLoadOrder
      => _assemblyLoadOrder;

    /// <summary>
    /// Types that failed to initialize and their exceptions.
    /// </summary>
    public IEnumerable<Type> InitializedTypes
      => _initializedTypes;

    /// <summary>
    /// The assemblies included in the pre-included assemblies as well as the ones the main one is dependent on.
    /// These are not considered mods.
    /// </summary>
    public IEnumerable<Assembly> CoreAssemblies 
      { get; private set; }

    /// <summary>
    /// The total count of currently uninitialized types.
    /// </summary>
    public int UninitializedTypesCount
      => (_uninitializedArchetypes?.Count ?? 0)
        + (_uninitializedModels?.Count ?? 0)
        + (_uninitializedComponents?.Count ?? 0)
        + (_uninitializedEnums?.Count ?? 0);

    #region Initialization

    /// <summary>
    /// Make a new Archetype Loader.
    /// This can be made to make an Archetype Universe instance.
    /// </summary>
    public Loader(Settings? options = null) {
      Options = options ?? new Settings();
    }

    /// <summary>
    /// Try to load all archetypes, using the Settings
    /// </summary>
    public void Initialize(Universe? universe = null) {
      Universe?.ExtraContexts.OnLoaderInitializationStart(this);
      _initFields();

      Universe = universe ?? Universe ?? new Universe(this, Options.UniverseName);

      _initializeDefaultExtraContexts();

      Universe.ExtraContexts.OnLoaderInitializationComplete(Universe);

      _initalizeCompatableArchetypeData();

      Universe.ExtraContexts.OnLoaderInitialSystemTypesCollected();

      _initializeTypesByAssembly();

      Universe.ExtraContexts.OnLoaderTypesInitializationFirstRunComplete();

      while (_remainingInitializationAttempts-- > 0 && UninitializedTypesCount > 0) {
        int attemptNumber = Options.InitializationAttempts - _remainingInitializationAttempts + 1;
        Universe.ExtraContexts.OnLoaderFurtherAnizializationAttemptStart(attemptNumber);
        _tryToCompleteAllEnumsInitialization();
        _tryToCompleteAllModelsInitialization();
        _tryToCompleteAllArchetypesInitialization();
        _tryToCompleteAllComponentsInitialization();
        Universe.ExtraContexts.OnLoaderFurtherInizializationAttemptComplete(attemptNumber);
      }

      // try to register any final splayed archetypes.
      _tryToInitializeAllRemainingSplayedTypes(true);

      Universe.ExtraContexts.OnLoaderTypesInitializationAllRunsComplete();

      _testBuildModelsForAllInitializedTypes();

      _tryToLoadAllModifiers();

      while (_remainingFinalizationAttempts-- > 0 && _initializedArchetypes.Count > 0) {
        _tryToFinishAllInitalizedTypes();
      }

      _finalize();
    }

    /// <summary>
    /// Can be used to add an initialized archetype to this loader from extra contexts etc while loading is ongoing.
    /// </summary>
    public void AddInitializedArchetype(Archetype archetype) {
      if (!IsFinished && _successfullyTestedArchetypes is null) {
        _initializedArchetypes.Add(archetype);
      } else throw new InvalidOperationException($"Loader is sealed or has started Model Tests, newly initialized Archetypes cannot be added.");
    }

    /// <summary>
    /// Can be used to add info about an archetype that failed to be loaded.
    /// </summary>
    public void AddArchetypeFailure(Type archetypeType, Exception exception = null) {
      if (!IsFinished) {
        _failures.Add(new("Archetype", archetypeType, exception));
      } else throw new InvalidOperationException($"Loader is Sealed, failures shouldn't be being added at this point.");
    }

    /// <summary>
    /// Can be used to add info about an model that failed to be loaded.
    /// </summary>
    public void AddModelTypeFailure(Type modelType, Exception exception = null) {
      if (!IsFinished) {
        _failures.Add(new("Model", modelType, exception));
      }
      else throw new InvalidOperationException($"Loader is Sealed, failures shouldn't be being added at this point.");
    }

    /// <summary>
    /// Can be used to add info about an model that failed to be loaded.
    /// </summary>
    public void AddComponentTypeFailure(Type componentType, Exception exception = null) {
      if (!IsFinished) {
        _failures.Add(new("Component", componentType, exception));
      }
      else throw new InvalidOperationException($"Loader is Sealed, failures shouldn't be being added at this point.");
    }

    /// <summary>
    /// Can be used to add info about an enum that failed to be loaded.
    /// </summary>
    public void AddEnumValueFailure(PropertyInfo enumProperty, Exception exception = null) {
      if (!IsFinished) {
        _failures.Add(new("Enum", enumProperty.PropertyType, exception) {  Metadata = enumProperty });
      }
      else throw new InvalidOperationException($"Loader is Sealed, failures shouldn't be being added at this point.");
    }

    void _initFields() {
      _remainingInitializationAttempts = Options.InitializationAttempts;
      _remainingFinalizationAttempts = Options.FinalizationAttempts;

      _testModels = new();
      _loadedTestParams = new();
      _orderedAssemblyFiles = new();
      _assemblyLoadOrder = new();
      _unorderedAssembliesToLoad = new();

      _initializedTypes = new();
      _initializedArchetypes = new();
      _finishedArchetypes = new();

      _uninitializedComponents = new();
      _uninitializedModels = new();
      _uninitializedArchetypes = new();
      _uninitializedEnums = new();

      _failedEnumerations = new();
      _failedArchetypes = new();
      _failedComponents = new();
      _failedModels = new();

      _failures = new();
    }

    /// <summary>
    /// Set up initial settings.
    /// </summary>
    void _initalizeCompatableArchetypeData() {
      // pre-load
      Options.PreLoadAssemblies.Count();

      // order assemblies according to the config.json.
      _loadValidAssemblies();
      _loadModLoadOrderFromJson();
      _orderAssembliesByModLoadOrder();

      Universe.ExtraContexts.OnLoaderAssembliesCollected(_assemblyLoadOrder);

      _loadAllBuildableTypes();
    }

    /// <summary>
    /// Initialize the Universe.*Data types and Items in the Default Extra Contexts under the Options.
    /// </summary>
    void _initializeDefaultExtraContexts() {
      Universe.ExtraContexts.OnLoaderDefaultExtraContextsInitializationStart(Universe);
      Universe.Models.ComparisonConfig = Options.DefaultComparisonConfig;

      foreach (var (type, config) in Options.DefaultExtraContexts) {
        Universe.SetExtraContext(config.contextConstructor(config.optionsConstructor(Universe)));
      }

      Universe.ExtraContexts.OnLoaderDefaultExtraContextsInitializationComplete(Universe);
    }

    void _initializeTypesByAssembly() {
      foreach (AssemblyBuildableTypesCollection typesToBuild in _assemblyTypesToBuild.Values) {
        Universe.ExtraContexts.OnLoaderAssemblyLoadStart(typesToBuild);

        // enums first:
        foreach (PropertyInfo prop in typesToBuild.Enumerations) {
          if (!_tryToInitializeAndBuildEnumType(prop, out var e)) {
            if (e is CannotInitializeResourceException) {
              _failedEnumerations[prop] = e;
            }
            else
              _uninitializedEnums[prop] = e;
          }
        }

        // components next: 
        foreach (Type systemType in typesToBuild.Components) {
          if (!_tryToInitializeComponent(systemType, out var e)) {
            if (e is CannotInitializeResourceException) {
              _failedComponents[systemType] = e;
            }
            else
              _uninitializedComponents[systemType] = e;
          }
        }

        // then we run the static initializers for all simple models:
        foreach (Type systemType in typesToBuild.Models.Where(t => typeof(IModel<>).IsAssignableFrom(t))) {
          if (!_tryToPreInitializeSimpleModel(systemType, out var e)) {
            if (e is CannotInitializeResourceException) {
              _failedModels[systemType] = e;
            }
            else
              _uninitializedModels[systemType] = e;
          }
        }

        // then initialize archetypes:
        foreach (Type systemType in typesToBuild.Archetypes) {
          if (!_tryToInitializeArchetype(systemType, out var e)) {
            if (e is CannotInitializeResourceException) {
              _failedArchetypes[systemType] = e;
            }
            else
              _uninitializedArchetypes[systemType] = e;
          }
        }

        // then register models
        foreach (Type systemType in typesToBuild.Models.Except(_failedModels.Keys)) {
          if (!_tryToInitializeModel(systemType, out var e)) {
            if (e is CannotInitializeResourceException) {
              _failedModels[systemType] = e;
            }
            else
              _uninitializedModels[systemType] = e;
          }
        }

        // try to register any new splayed archetypes.
        _tryToInitializeAllRemainingSplayedTypes();

        Universe.ExtraContexts.OnLoaderAssemblyLoadComplete(typesToBuild);
      }
    }

    void _runStaticCtorsFromBaseClassUp(Type @class) {
      Type original = @class;
      List<Type> newAncestors = new() {
        @class
      };
      while ((@class = @class.BaseType) != null) {
        if (_staticallyInitializedTypes.Contains(@class)) {
          break;
        }
        else {
          _staticallyInitializedTypes.Add(@class);
          newAncestors.Add(@class);
        }
      }

      newAncestors.Reverse();
      foreach (Type type in newAncestors.Append(original)) {
        try {
          // invoke static ctor
          System.Runtime.CompilerServices
            .RuntimeHelpers
            .RunClassConstructor(type.TypeHandle);
        }
        catch (Exception e) {
          throw new Exception($"Failed to run static constructor for ancestor: {type?.FullName ?? "null"}, of type: {@class.FullName}.\n=Exception:{e}\n\n=Inner Exception\n{e.InnerException}");
        }
      }

      _staticallyInitializedTypes.Add(original);
    }

    #region Assembly Load Order Init

    /// <summary>
    /// Load all the mods from the mod folder
    /// </summary>
    void _loadModLoadOrderFromJson() {
      _orderedAssemblyFiles = Options.PreOrderedAssemblyFiles;
      string loadOrderFile = Path.Combine(Options.DataFolderParentFolderLocation, "order.json");
      if (File.Exists(loadOrderFile)) {
        foreach (LoadOrderItem loadOrderItem
          in JsonConvert.DeserializeObject<List<LoadOrderItem>>(
           File.ReadAllText(loadOrderFile))
        ) {
          _orderedAssemblyFiles
            .Add(loadOrderItem.Priority, loadOrderItem.AssemblyFileName);
        }
      }
    }

    void _orderAssembliesByModLoadOrder() {
      if (_orderedAssemblyFiles.Forward.Any()) {
        _assemblyLoadOrder
          = _unorderedAssembliesToLoad.OrderBy(
            assembly => _orderedAssemblyFiles.Reverse
              .TryGetValue(assembly.FullName.Split(',')[0], out ushort foundPriority)
                ? foundPriority
                : ushort.MaxValue
        ).ToList();
      } // Random order by default:
      else {
        _assemblyLoadOrder
          = _unorderedAssembliesToLoad;
      }
    }

    /// <summary>
    /// An item for setting up the load order; order.json file.
    /// Used to specify the order to load assemblies in
    /// </summary>
    struct LoadOrderItem {

      /// <summary>
      /// The order in the list/priority.
      /// Lower values go first
      /// </summary>
      public ushort Priority {
        get;
        set;
      }

      /// <summary>
      /// The local file name of the assembly.
      /// Add the folder path if it's in a sub folder too
      /// </summary>
      public string AssemblyFileName {
        get;
        set;
      }
    }

    #endregion

    #region Assembly and System Type Init

    /// <summary>
    /// Collect all assemblies that could have archetypes into _unorderedAssembliesToLoad
    /// </summary>
    void _loadValidAssemblies() {
      // load internal assemblies
      List<Assembly> defaultAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
      if (Options.PreLoadAllReferencedAssemblies) {
        var moreAssemblies = defaultAssemblies.SelectMany(
          a => a.GetReferencedAssemblies()
            .Where(assembly => _validateAssemblyByName(assembly))
            .Select((item) => {
              try {
                return Assembly.Load(item);
              }
              catch (Exception e) {
                throw new Exception($"Could not load assembly:{item.FullName}", e);
              }
            })).ToList();

        defaultAssemblies.AddRange(moreAssemblies);
      }

      CoreAssemblies = defaultAssemblies.ToList().AsEnumerable();

      // get ones added to the load order.
      List<Assembly> externalAssemblies = new();
      var externalAssemblyLocations = Options.PreOrderedAssemblyFiles.Forward.Values.Except(
          defaultAssemblies.Where(a => !a.IsDynamic).Select(a => a.Location)
      );
      if (externalAssemblyLocations.Any()) {
        foreach (var compatableAssemblyFileName in externalAssemblyLocations) {
          externalAssemblies
            .Add(Assembly.LoadFrom(compatableAssemblyFileName));
        }
      }

      // combine and filter them
      _unorderedAssembliesToLoad = defaultAssemblies.Concat(externalAssemblies)
        .Except(Options.IgnoredAssemblies)
        // ... that is not dynamic, and that matches any naming requirements
        .Where(assembly => !assembly.IsDynamic
          && (Options.PreLoadAssemblies.Contains(assembly)
            || _validateAssemblyByName(assembly.GetName()))
        ).Distinct().ToList();
    }

    bool _validateAssemblyByName(AssemblyName assembly)
      => assembly.FullName.StartsWith(Options.ArchetypeAssembliesPrefix)
        && !Options.AssemblyPrefixesToIgnore
          .Where(assemblyPrefix => assembly.FullName.StartsWith(assemblyPrefix))
          .Any();

    /// <summary>
    /// Get all types that this loader knows how to build from the loaded assemblies.
    /// Sets _assemblyTypesToBuild
    /// </summary>
    void _loadAllBuildableTypes() {
      _assemblyTypesToBuild =
        new OrderedDictionary<Assembly, AssemblyBuildableTypesCollection>();

      // TODO: allow the assemblies to somehow apply a load order.
      // Maybe provide their own weight, or an ini/json file with weights/settings
      // after that we should also get the Archetype.Modifier classes from each assembly if they exist.

      // For each loaded assembly
      foreach (Assembly assembly in AssemblyLoadOrder) {
        // For each type in these assemblies
        foreach (Type systemType in assembly.GetExportedTypes().Where(
          // ... abstract types can't be built
          systemType =>
            // ... if it doesn't have a disqualifying attribute
            (!Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildInInitialLoadAttribute))
              && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildThisOrChildrenInInitialLoadAttribute), true))
            || typeof(ISplayed).IsAssignableFrom(systemType)
        )) {
          if (!systemType.IsAbstract) {
            // ... if it extends Archetype<,> 
            if (systemType.IsAssignableToGeneric(typeof(Archetype<,>))) {
              _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
              _assemblyTypesToBuild[assembly]._archetypes.Add(systemType);
              IEnumerable<DependencyAttribute> dependencies = systemType.GetCustomAttributes<DependencyAttribute>();
              if (dependencies?.Any() ?? false) {
                Universe.Archetypes._dependencies[systemType] = dependencies.Select(d => d.DependentOnType);
              }
            } // ... or IModel<>
            else if (systemType.IsAssignableToGeneric(typeof(IModel<>))) {
              _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
              // ... if it's an IComponent<>
              if (systemType.IsAssignableToGeneric(typeof(IComponent<>))) {
                _assemblyTypesToBuild[assembly]._components.Add(systemType);
                IEnumerable<DependencyAttribute> dependencies = systemType.GetCustomAttributes<DependencyAttribute>();
                if (dependencies?.Any() ?? false) {
                  Universe.Components._dependencies[systemType] = dependencies.Select(d => d.DependentOnType);
                }
              }
              else {
                _assemblyTypesToBuild[assembly]._models.Add(systemType);
                IEnumerable<DependencyAttribute> dependencies = systemType.GetCustomAttributes<DependencyAttribute>();
                if (dependencies?.Any() ?? false) {
                  Universe.Models._dependencies[systemType] = dependencies.Select(d => d.DependentOnType);
                }
              }
            } // if it's a modifications class:
            else if (typeof(Modifications).IsAssignableFrom(systemType)) {
              _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
              _assemblyTypesToBuild[assembly].Modifications = systemType;
              //TOOD: modifications file can have dependencies on other modifications classes. This makes the whole assembly wait for the dependent one!.
            }
          }

          // if this type's got enums we want:
          if (systemType.GetCustomAttribute<BuildAllDeclaredEnumValuesOnInitialLoadAttribute>() != null
            || systemType.IsAssignableToGeneric(typeof(Enumeration<>))
            || (systemType.IsAssignableToGeneric(typeof(Archetype<,>))
              && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildInInitialLoadAttribute))
                && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildThisOrChildrenInInitialLoadAttribute), true))
          ) {
            _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
            foreach (PropertyInfo staticEnumProperty in systemType.GetProperties(BindingFlags.Static | BindingFlags.Public).Where(p => p.PropertyType.IsAssignableToGeneric(typeof(Enumeration<>)))) {
              _assemblyTypesToBuild[assembly]._enumerations.Add(staticEnumProperty);
            }
          }
          else
            continue;
        }
      }
    }

    /// <summary>
    /// Make sure the assembly collection exists for the given assmebly. Add a new one if it doesnt.
    /// </summary>
    static void _validateAssemblyCollectionExists(
      OrderedDictionary<Assembly, AssemblyBuildableTypesCollection> allCollections,
      Assembly assembly
    ) {
      if (!allCollections.ContainsKey(assembly)) {
        allCollections.Add(assembly, new AssemblyBuildableTypesCollection(assembly));
      }
    }

    /// <summary>
    /// Used to hold assembly types to load.
    /// </summary>
    public class AssemblyBuildableTypesCollection {
      internal List<Type> _archetypes
          = new();
      internal List<Type> _models
          = new();
      internal List<Type> _components
          = new();
      internal List<PropertyInfo> _enumerations
          = new();

      /// <summary>
      /// The archetype types
      /// </summary>
      public IReadOnlyList<Type> Archetypes
          => _archetypes;

      /// <summary>
      /// The model types
      /// </summary>
      public IReadOnlyList<Type> Models
          => _models;

      /// <summary>
      /// The component types
      /// </summary>
      public IReadOnlyList<Type> Components
          => _components;

      /// <summary>
      /// The enumeration properties
      /// </summary>
      public IReadOnlyList<PropertyInfo> Enumerations
          => _enumerations;

      /// <summary>
      /// The single modification class allowed per assembly.
      /// </summary>
      public Type Modifications {
        get;
        internal set;
      }

      /// <summary>
      /// The assembly these types are from
      /// </summary>
      public Assembly Assembly {
        get;
      }

      /// <summary>
      /// Used to make a new buildable type collection.
      /// </summary>
      internal AssemblyBuildableTypesCollection(Assembly assembly) {
        Assembly = assembly;
      }
    }

    #endregion

    #region Archetype Init

    bool _tryToInitializeArchetype(Type systemType, out Exception e) {
      Universe.ExtraContexts.OnLoaderArchetypeInitializationStart(systemType, false);
      // Check dependencies.
      // TODO: for a,m,&c, index items by their waiting dependency and when a type loads check if anything is waiting on it, and try to load it then.
      if (Universe.Archetypes.Dependencies.TryGetValue(systemType, out var dependencies)) {
        Type firstUnloadedDependency
          = dependencies.FirstOrDefault(t => !_initializedTypes.Contains(t));
        if (firstUnloadedDependency != null) {
          e = new MissingDependencyForArchetypeException(systemType, firstUnloadedDependency);
          Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(false, systemType, null, e, false);
          return false;
        }
      }

      bool isSplayed;
      Archetype archetype = null;
      try {
        // check if it's splayed
        isSplayed = typeof(ISplayed).IsAssignableFrom(systemType);

        // if not we need to construct a new one
        if (!isSplayed
          || (!Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildInInitialLoadAttribute))
            && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildThisOrChildrenInInitialLoadAttribute), true))
        ) {
          archetype = _constructArchetypeFromSystemType(systemType);
          _uninitializedArchetypes.Remove(systemType);
        }

        // init splayed
        if (isSplayed) {
          _initializeSplayedArchetype(systemType);
        }
      }
      catch (CannotInitializeArchetypeException ce) {
        if (Options.FatalOnCannotInitializeType) {
          Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(false, systemType, null, ce, false);
          throw;
        }

        e = ce;
        Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(false, systemType, null, e, false);
        return false;
      }
      catch (FailedToConfigureNewArchetypeException fe) {
        e = fe;
        Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(false, systemType, null, e, false);
        return false;
      }
      catch (MissingDependencyForArchetypeException fe) {
        e = fe;
        Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(false, systemType, null, e, false);
        return false;
      }

      // done initializing!
      _initializedTypes.Add(systemType);

      e = null;
      Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(true, systemType, archetype, e, false);
      return true;
    }

    void _initializeSplayedArchetype(Type systemType) {
      object dummy;
      List<GenericTestArgumentAttribute> attributes = new();
      Type[] genericTestTypeArguments = null;

      if (systemType.ContainsGenericParameters) {
        if ((attributes = systemType.GetCustomAttributes().Where(a => a is GenericTestArgumentAttribute).Cast<GenericTestArgumentAttribute>().ToList()).Any()) {
          genericTestTypeArguments = attributes.OrderBy(a => a.Order).Select(a => a.GenericArgumentType).ToArray();
          dummy = FormatterServices.GetUninitializedObject(systemType.MakeGenericType(genericTestTypeArguments));
        }
        else throw new InvalidDataException($"Splayed Archetype of type: {systemType.Name} is generic, and does not implement one of more of the GenericTestArgumentAttribute");
      }
      else {
        dummy = FormatterServices.GetUninitializedObject(systemType);
      }

      _archetypeUniverseBackingField.SetValue(dummy, Universe);

      foreach (var splayType in systemType.GetAllInheritedGenericTypes(typeof(ISplayed<,>))) {
        if (splayType.GetGenericArguments().Last() == systemType) {
          Type enumType = splayType.GetGenericArguments()[0];
          Type enumBaseType = enumType.GetFirstInheritedGenericTypeParameters(typeof(Enumeration<>)).First();

          ISplayed.Constructor getMethod;
          if ((getMethod = ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType.TryToGet(enumBaseType)?.TryToGet(enumType)?.TryToGet(splayType)) is null) {
            getMethod = _getSplayerArchetypeCtor(dummy, splayType, genericTestTypeArguments);
            _prepareSplayedArchetype(splayType, getMethod, enumType, enumBaseType);
          }

          _constructSplayedArchetypes(splayType, enumType, enumBaseType);
        }
      }
    }

    static ISplayed.Constructor _getSplayerArchetypeCtor(object dummy, Type splayType, Type[] genericTestTypes = null) {
      Type constructedSplayedType = splayType;

      if (genericTestTypes is not null) {
        var genericBaseArguments = splayType.GetGenericArguments();
        genericBaseArguments[1] = genericBaseArguments[1].MakeGenericType(genericTestTypes);
        constructedSplayedType = splayType.GetGenericTypeDefinition().MakeGenericType(genericBaseArguments);
      }

      MethodInfo getMethodInfo
        = constructedSplayedType.GetMethod("ConstructArchetypeFor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
      MethodInfo registerMethodInfo
        = constructedSplayedType.GetMethod("_registerSubArchetype", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

      ISplayed.Constructor ctor = new((@enum, universe) => {
        var archetype = (Archetype)getMethodInfo.Invoke(dummy, new object[] { @enum, universe });
        registerMethodInfo.Invoke(null, new object[] { archetype, @enum });
        return archetype;
      });

      return ctor;
    }

    void _tryToInitializeAllRemainingSplayedTypes(bool markErrorsAsFailuresAndContinue = false) {
      foreach (var splayedInterfaceWithGetMethod in ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType.SelectMany(e => e.Value.SelectMany(e => e.Value))) {
        _constructSplayedArchetypes(splayedInterfaceWithGetMethod.Key, getMethod: splayedInterfaceWithGetMethod.Value, markErrorsAsFailuresAndContinue: markErrorsAsFailuresAndContinue);
      }
    }

    void _constructSplayedArchetypes(Type splayInterfaceType, Type enumType = null, Type enumBaseType = null, ISplayed.Constructor getMethod = null, bool markErrorsAsFailuresAndContinue = false) {
      enumType ??= splayInterfaceType.GetGenericArguments()[0];
      enumBaseType ??= enumType.GetFirstInheritedGenericTypeParameters(typeof(Enumeration<>)).First();

      IEnumerable<Enumeration> enumValues;
      try {
        enumValues = Universe.Enumerations.GetAllByType(enumType);
      }
      catch (KeyNotFoundException ex) {
        var e = new MissingDependencyForArchetypeException($"Enums of type: {enumType.FullName}, have not been collected yet.", ex);
        if (markErrorsAsFailuresAndContinue) {
          _failures.Add(new Failure("Archetype", splayInterfaceType, e));
          return;
        }
        else throw e;
      }
      catch (Exception ex) {
        var e = new FailedToConfigureNewArchetypeException($"Could not build splayed Archetypes of type: {splayInterfaceType.GetGenericArguments()[1].FullName} for enums of type: {enumType.FullName} due to unknown inner exception.", ex);
        if (markErrorsAsFailuresAndContinue) {
          _failures.Add(new Failure("Archetype", splayInterfaceType, e));
          return;
        }
        else throw e;
      }

      getMethod ??= ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType[enumBaseType][enumType][splayInterfaceType];
      foreach (var @enum in enumValues.Except(ISplayed._completedEnumsByInterfaceBase.TryToGet(splayInterfaceType) ?? Enumerable.Empty<Enumeration>())) {
        Universe.ExtraContexts.OnLoaderArchetypeInitializationStart(splayInterfaceType.GetGenericArguments()[1], true);
        Archetype newType;
        try {
          newType = getMethod(@enum, Universe);
        }
        catch (CannotInitializeResourceException e) {
          Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(false, splayInterfaceType.GetGenericArguments()[1], null, e, true);
          if (markErrorsAsFailuresAndContinue) {
            _failures.Add(new Failure("Archetype", splayInterfaceType, e));
            continue;
          } else throw;
        }
        catch (Exception e) {
          Type archetypeType = splayInterfaceType.GetGenericArguments()[1];
          Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(false, archetypeType, null, e, true);
          var ex = new FailedToConfigureNewArchetypeException($"Failed to build Archetype Sub Type for splayed Archetype: {archetypeType.FullName}, using enum: {@enum}, of type: {enumType.FullName}", e);
          if (markErrorsAsFailuresAndContinue) {
            _failures.Add(new Failure("Archetype", splayInterfaceType, ex));
            continue;
          } else throw ex;
        }

        ISplayed._completedEnumsByInterfaceBase.AddToInnerHashSet(splayInterfaceType, @enum);
        _initializedArchetypes.Add(newType);
        Universe.ExtraContexts.OnLoaderArchetypeInitializationComplete(true, newType.GetType(), newType, null, true);
      }
    }

    void _prepareSplayedArchetype(Type splayInterfaceType, ISplayed.Constructor getMethod, Type enumType, Type enumBaseType) {
      if (Options.AllowRuntimeTypeRegistrations) {
        var allowLazyPropAttribute
          = splayInterfaceType.GetGenericArguments().Last()
          .GetCustomAttribute<AllowLazyArchetypeInitializationsOnNewLazyEnumerationInitializationsAttribute>();

        if (allowLazyPropAttribute is not null && allowLazyPropAttribute.SplayType == splayInterfaceType) {
          ISplayed._splayedInterfaceTypesThatAllowLazyInitializations.Add(getMethod);
        }
      }

      if (ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType.TryGetValue(enumBaseType, out var existingWaitingSplayedTypes)) {
        if (existingWaitingSplayedTypes.TryGetValue(enumType, out var existingWaitingLazyCtors)) {
          existingWaitingLazyCtors.Add(splayInterfaceType, getMethod);
        }
        else existingWaitingSplayedTypes.Add(enumType, new() { { splayInterfaceType, getMethod } });
      }
      else {
        ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType[enumBaseType] = new() {
          {enumType, new() { { splayInterfaceType, getMethod } }   }
        };
      }
    }

    /// <summary>
    /// Try to construct the archetype, which will register it with it's collections:
    /// TODO: change this so if we are missing a dependency archetype, then this tries to load that one by name, and then adds +1 to a depth parameter (default 0) on this function.
    /// Maybe this could be done more smoothly by pre-emptively registering all ids?
    /// </summary>
    Archetype _constructArchetypeFromSystemType(Type archetypeSystemType) {
      // see if we have a partially initialized archetype registered
      Archetype archetype = archetypeSystemType?.TryToGetAsArchetype();
      
      //// Try to construct it.
      //// The CTor should register it to it's main collection.
      try {
        if (archetype is null) {
          // Get ctor
          var archetypeConstructor = _getArchetypeConstructor(archetypeSystemType);
          archetype = archetypeConstructor?.Invoke(Universe);
        }

        // success:
        _initializedArchetypes.Add(archetype);
      } // attempt failure: 
      catch (FailedToConfigureResourceException e) {
        string failureMessage = $"Failed on attempt #{Options.InitializationAttempts - _remainingInitializationAttempts} to construct new Archetype of type: {archetypeSystemType.FullName} due to unknown internal error. \n ---------- \n Will retry \n ---------- \n.";
        throw new FailedToConfigureNewArchetypeException(failureMessage, e);
      } // fatal:
      catch (Exception e) {
        string fatalMessage = $"Cannot initialize archetype of type: {archetypeSystemType?.FullName ?? "NULLTYPE"} Due to unknown inner exception. \n ---------- \n Will Not Retry \n ---------- \n.";
        throw new CannotInitializeArchetypeException(fatalMessage, e);
      }

      return archetype;
    }

    Func<Universe, Archetype> _getArchetypeConstructor(Type archetypeSystemType) {
      // We first look for a private parameterless ctor, 
      var archetypeConstructor = archetypeSystemType.GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic,
        null,
        new Type[0],
        null
      );

      // then we look for for a protected ctor with one argument which inherits from ArchetypeId,
      //    or one that also has a second optional Universe argument.
      if (archetypeConstructor is null) {
        archetypeConstructor = archetypeSystemType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
          .Where(ctor => (ctor.GetParameters().Count() == 1 || ctor.GetParameters().Count() == 2) && typeof(Identity).IsAssignableFrom(ctor.GetParameters()[0].ParameterType)).FirstOrDefault();

        if (archetypeConstructor is null) {
          throw new CannotInitializeArchetypeException($"Cannot initialize type: {archetypeSystemType?.FullName ?? "ERRORNULLTYPE"},\n  it does not impliment either:\n\t\t a private or protected parameterless constructor that takes no arguments,\n\t\t or a protected/private ctor that takes one argument that inherits from ArchetypeId that accepts the default of Null for singleton initialization.");
        }

        if (archetypeConstructor.GetParameters().Count() == 2) {
          return u => (Archetype)archetypeConstructor.Invoke(new object[] { null, u });
        } // if we don't have the Universe argument passed in, we need to use a trick to set the universe before the ctor.
        else {
          return u => {
            var archetype = (Archetype)FormatterServices.GetUninitializedObject(archetypeSystemType);
            _archetypeUniverseBackingField.SetValue(archetype, u);
            archetypeConstructor.Invoke(archetype, new object[] { null });

            return archetype;
          };
        }
      } // if we don't have the Universe argument passed in, we need to use a trick to set the universe before the ctor.
      else {
        return u => {
          var archetype = (Archetype)FormatterServices.GetUninitializedObject(archetypeSystemType);
          _archetypeUniverseBackingField.SetValue(archetype, Universe);
          archetypeConstructor.Invoke(archetype, new object[0]);

          return archetype;
        };
      }
    }

    /// <summary>
    /// Try to build a test model for each archetype, and throw if it fails
    /// </summary>
    void _testBuildModelsForAllInitializedTypes() {
      Universe.ExtraContexts.OnLoaderBuildAllTestModelsStart(_initializedArchetypes.Count);

      _successfullyTestedArchetypes = new();
      int attemptsRemaining = Options.ModelTestBuildAttempts;
      while (_successfullyTestedArchetypes.Count() < _initializedArchetypes.Count() && attemptsRemaining-- > 0) {
        foreach (var archetype in _initializedArchetypes.Except(_successfullyTestedArchetypes).ToList()) {
          if (!_tryToBuildDefaultModelForArchetype(archetype.GetType(), archetype, out var failureException)) {
            if (attemptsRemaining == 0) {
              _initializedArchetypes.Remove(archetype);
              _uninitializedArchetypes.TryAdd(archetype.GetType(), failureException);
              _failedArchetypes.TryAdd(archetype.GetType(), failureException);
              archetype.TryToUnload();
            }
          }
          else {
            _successfullyTestedArchetypes.Add(archetype);
          }
        }
      }

      Universe.ExtraContexts.OnLoaderBuildAllTestModelsComplete();
    }

    bool _tryToBuildDefaultModelForArchetype(Type archetypeSystemType, Archetype archetype, out Exception exception) {
      Universe.ExtraContexts.OnLoaderTestModelBuildStart(archetype);
      exception = null;
      IModel defaultModel = null;
      try {
        // load branch attribute and set the new model ctor if there is one
        BranchAttribute branchAttribute;
        // (first one is newest inherited)
        if ((branchAttribute = archetypeSystemType.GetCustomAttributes<BranchAttribute>().FirstOrDefault()) != null) {
          branchAttribute.NewBaseModelType
            // Defaults to decalring type (surrounding type) if one wasn't specified.
            ??= _getFirstDeclaringParent(archetypeSystemType);

          // set it to the produced type for now
          archetype.ModelTypeProduced = branchAttribute.NewBaseModelType;

          Func<IBuilder, IModel> defaultModelConstructor
            = Universe.Models._getDefaultCtorFor(branchAttribute.NewBaseModelType);

          (archetype as IFactory)._modelConstructor
            = defaultModelConstructor;
        }

        // Try to make the default model, and register what that is:
        defaultModel
          = GetOrBuildTestModel(
              archetype,
              archetype.ModelTypeProduced
          );
      }
      catch (CannotInitializeResourceException e) {
        exception = e;
      }
      catch (MissingDependencyForModelException e) {
        exception = new FailedToConfigureNewModelException($"Could not configure default model. Will try again.", e);
      }
      catch (FailedToConfigureNewModelException e) {
        exception = new FailedToConfigureNewArchetypeException($"Could not configure default model. Will try again.", e);
      }
      catch (Exception e) {
        exception = new FailedToConfigureNewModelException($"Could not configure default model. Will try again.", e);
      }

      if (exception is not null) {
        Universe.ExtraContexts.OnLoaderTestModelBuildComplete(false, archetype, defaultModel, exception);
        return false;
      }

      Type modelType = defaultModel.GetType();

      archetype.ModelTypeProduced
        = Universe.Models._modelTypesProducedByArchetypes[archetype]
        = modelType;

      if (!Universe.Archetypes._rootArchetypeTypesByBaseModelType.ContainsKey(modelType.FullName)) {
        Universe.Archetypes._rootArchetypeTypesByBaseModelType[modelType.FullName] = archetype.GetType();
      }

      Universe.ExtraContexts.OnLoaderTestModelBuildComplete(true, archetype, defaultModel, null);

      return true;
    }

    #endregion

    #region Model Init

    /// <summary>
    /// Try to initialize a model type.
    /// </summary>
    bool _tryToPreInitializeSimpleModel(Type systemType, out Exception? e) {
      Universe.ExtraContexts.OnLoaderSimpleModelInitializationStart(systemType);
      // Check dependencies
      if (Universe.Models.Dependencies.TryGetValue(systemType, out var dependencies)) {
        Type firstUnloadedDependency
          = dependencies.FirstOrDefault(t => !_initializedTypes.Contains(t));
        if (firstUnloadedDependency != null) {
          e = new MissingDependencyForModelException(systemType, firstUnloadedDependency);
          Universe.ExtraContexts.OnLoaderSimpleModelInitializationComplete(false, systemType, e);

          return false;
        }
      }

      // invoke static ctor

      try {
        _runStaticCtorsFromBaseClassUp(systemType);
      }
      catch (CannotInitializeModelException ce) {
        if (Options.FatalOnCannotInitializeType) {
          throw;
        }

        e = ce;
        Universe.ExtraContexts.OnLoaderSimpleModelInitializationComplete(false, systemType, e);
        return false;
      }
      catch (Exception ex) {
        e = new FailedToConfigureNewModelException($"Could not initialize Model of type {systemType} due to Unknown Inner Exception.", ex);
        Universe.ExtraContexts.OnLoaderSimpleModelInitializationComplete(false, systemType, e);
        return false;
      }

      e = null;
      Universe.ExtraContexts.OnLoaderSimpleModelInitializationComplete(true, systemType, null);
      return true;
    }

    /// <summary>
    /// Try to initialize a model type.
    /// </summary>
    bool _tryToInitializeModel(Type systemType, out Exception? e) {
      Universe.ExtraContexts.OnLoaderModelFullInitializationStart(systemType);
      // Check dependencies
      if (Universe.Models.Dependencies.TryGetValue(systemType, out var dependencies)) {
        Type firstUnloadedDependency
          = dependencies.FirstOrDefault(t => !_initializedTypes.Contains(t));
        if (firstUnloadedDependency != null) {
          e = new MissingDependencyForModelException(systemType, firstUnloadedDependency);
          Universe.ExtraContexts.OnLoaderModelFullInitializationComplete(false, systemType, e);
          return false;
        }
      }

      // invoke static ctor

      try {
        _runStaticCtorsFromBaseClassUp(systemType);
        _registerModelType(systemType);
      }
      catch (CannotInitializeModelException ce) {
        if (Options.FatalOnCannotInitializeType) {
          Universe.ExtraContexts.OnLoaderModelFullInitializationComplete(false, systemType, ce);
          throw;
        }

        e = ce;
        Universe.ExtraContexts.OnLoaderModelFullInitializationComplete(false, systemType, e);
        return false;
      }
      catch (Exception ex) {
        e = new FailedToConfigureNewModelException($"Could not initialize Model of type {systemType} due to Unknown Inner Exception.", ex);
        Universe.ExtraContexts.OnLoaderModelFullInitializationComplete(false, systemType, e);
        return false;
      }

      _initializedTypes.Add(systemType);

      e = null;
      Universe.ExtraContexts.OnLoaderModelFullInitializationComplete(true, systemType, e);
      return true;
    }

    /// <summary>sd
    /// Register a new type of model.
    /// </summary>
    void _registerModelType(Type systemType) {
      Universe.ExtraContexts.OnLoaderModelFullRegistrationStart(systemType);

      try {
        if (_tryToGetLoaderInitializerDelegate(systemType, out var initializer)) {
          initializer(Universe);
        }

        // assign root archetype references
        // archetype based models
        if (systemType.IsAssignableToGeneric(typeof(IModel<,>))) {
          if (!Universe.Archetypes._rootArchetypeTypesByBaseModelType.ContainsKey(key: systemType.FullName)) {
            var types = systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<,>));
            Type rootArchetype = types.Last();

            Universe.Archetypes._rootArchetypeTypesByBaseModelType[systemType.FullName]
              = rootArchetype;
          }
        } // factory based models
        else {
          Type typeToTest = systemType;
          if (systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<>)).First().FullName is null) {
            IEnumerable<GenericTestArgumentAttribute> attributes;
            if ((attributes = systemType.GetCustomAttributes().Where(a => a is GenericTestArgumentAttribute).Cast<GenericTestArgumentAttribute>()).Any()) {
              typeToTest = systemType.MakeGenericType(
                attributes.OrderBy(a => a.Order)
                  .Select(a => a.GenericArgumentType)
                  .ToArray()
              );
            }
            else {
              var ex = new InvalidDataException($"Model of type: {systemType.Name} is generic, and does not implement one of more of the GenericTestArgumentAttribute");
              Universe.ExtraContexts.OnLoaderModelFullRegistrationComplete(false, systemType, ex);
              throw ex;
            }
          }

          _initializedArchetypes.Add(
            (Archetype)Universe.Models.GetFactory(typeToTest)
          );
        }
      }
      catch (FailedToConfigureResourceException e) {
        throw;
      }
      catch (CannotInitializeResourceException e) {
        throw;
      }
      catch (Exception e) {
        Universe.ExtraContexts.OnLoaderModelFullRegistrationComplete(false, systemType, e);
        throw new FailedToConfigureNewModelException($"Could not Register Model of type: {systemType.FullName} due to unknown inner exception.", e);
      }

      try {
        if (!Universe.Models._baseTypes.ContainsKey(systemType.FullName)) {
          Universe.Models._baseTypes.Add(
            systemType.FullName,
            systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<>)).FirstOrDefault()
              ?? systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<,>)).First()
          );
        }
      }
      catch (Exception e) {
        var ex = new CannotInitializeModelException($"Could not find IModel<> Base Type for {systemType}, does it inherit from IModel by mistake instead of Model<T>?", e);
        Universe.ExtraContexts.OnLoaderModelFullRegistrationComplete(false, systemType, ex);
        throw ex;
      }

      Universe.ExtraContexts.OnLoaderModelFullRegistrationComplete(true, systemType, null);
    }

    bool _tryToGetLoaderInitializerDelegate(Type systemType, out OnInitializingInLoaderAttribute.Initializer? initializer) {
      var potentialMethod = systemType.GetMethods(BindingFlags.Static | BindingFlags.DeclaredOnly)
        .FirstOrDefault(m => {
          if (m.HasAttribute<OnInitializingInLoaderAttribute>()) {
            if (!m.IsCompatibleWithDelegate<OnInitializingInLoaderAttribute.Initializer>()) {
              throw new CannotInitializeResourceException($"{nameof(OnInitializingInLoaderAttribute)} on Resource type: {systemType.ToFullHumanReadableNameString()} is on a Method that does not match the delegate type: {nameof(OnInitializingInLoaderAttribute)}.{nameof(OnInitializingInLoaderAttribute.Initializer)}");
            }

            return true;
          }

          return false;
        });

      if (potentialMethod is not null) {
        initializer = universe => potentialMethod.Invoke(null, new object[] { universe });
        return true;
      }

      var potentialProperty = systemType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly)
        .FirstOrDefault(p => {
          if (p.HasAttribute<OnInitializingInLoaderAttribute>()) {
            if (p.PropertyType != typeof(OnInitializingInLoaderAttribute.Initializer)) {
              if (p.PropertyType != typeof(Action<Universe>)) {
                throw new CannotInitializeResourceException($"{nameof(OnInitializingInLoaderAttribute)} on Resource type: {systemType.ToFullHumanReadableNameString()} is on a Method that does not match the delegate type: {nameof(OnInitializingInLoaderAttribute)}.{nameof(OnInitializingInLoaderAttribute.Initializer)}");
              }
            }

            return true;
          }

          return false;
        });

      if (potentialProperty is null) {
        initializer = null;
        return false;
      }

      initializer = potentialProperty != typeof(OnInitializingInLoaderAttribute.Initializer)
        ? new OnInitializingInLoaderAttribute.Initializer((Action<Universe>)potentialProperty.GetValue(null))
        : (OnInitializingInLoaderAttribute.Initializer)potentialProperty.GetValue(null);
      return true;
    }
      

    #endregion

    #region Component Init

    /// <summary>
    /// Try to initialize a component type.
    /// </summary>
    bool _tryToInitializeComponent(Type systemType, out Exception e) {
      Universe.ExtraContexts.OnLoaderComponentInitializationStart(systemType);
      /// Check dependencies
      if (Universe.Components.Dependencies.TryGetValue(systemType, out var dependencies)) {
        Type firstUnloadedDependency
          = dependencies.FirstOrDefault(t => !_initializedTypes.Contains(t));
        if (firstUnloadedDependency != null) {
          e = new MissingDependencyForComponentException(systemType, firstUnloadedDependency);
          Universe.ExtraContexts.OnLoaderComponentInitializationComplete(false, systemType, e);
          return false;
        }
      }

      try {
        _registerComponentType(systemType);
      }
      catch (CannotInitializeComponentException ce) {
        if (Options.FatalOnCannotInitializeType) {
          throw;
        }

        e = ce;
        Universe.ExtraContexts.OnLoaderComponentInitializationComplete(false, systemType, e);
        return false;
      }
      catch (Exception ex) {
        e = new FailedToConfigureNewComponentException($"Could not initialize Component of type {systemType} due to Unknown Inner Exception.", ex);
        Universe.ExtraContexts.OnLoaderComponentInitializationComplete(false, systemType, e);
        return false;
      }

      _initializedTypes.Add(systemType);
      e = null;
      Universe.ExtraContexts.OnLoaderComponentInitializationComplete(true, systemType, null);
      return true;
    }

    /// <summary>
    /// Register types of components
    /// </summary>
    void _registerComponentType(Type systemType) {

      // invoke static ctor
      _runStaticCtorsFromBaseClassUp(systemType);
      if (_tryToGetLoaderInitializerDelegate(systemType, out var initializer)) {
        initializer(Universe);
      }

      Universe.ExtraContexts.OnLoaderBuildTestComponentStart(systemType);
      IComponent testComponent = null;
      try {
        testComponent = _testBuildDefaultComponent(systemType);
      }
      catch (Exception e) {
        Universe.ExtraContexts.OnLoaderBuildTestComponentComplete(false, systemType, testComponent, e);
        throw;
      }
      Universe.ExtraContexts.OnLoaderBuildTestComponentComplete(true, systemType, testComponent, null);

      try {
        Universe.Components._baseTypes.Add(
          systemType.FullName,
          systemType.GetFirstInheritedGenericTypeParameters(typeof(IComponent<>)).First()
        );
      }
      catch (Exception e) {
        throw new NotImplementedException($"Could not find IComponent<> Base Type for {systemType}, does it inherit from IComponent instead of IComponent<T> by mistake?", e);
      }

      if (typeof(IComponent.IHaveContract).IsAssignableFrom(systemType)) {
        _cacheContractsForComponentType(systemType);
      }

      systemType.GetFirstInheritedGenericTypeParameters(typeof(Archetype.IComponent.IAmLinkedTo<>))
        .FirstOrDefault()?
        .ThenDo(t => Universe.Components._archetypeComponentsLinkedToModelComponents.Add(systemType, t));
    }

    void _cacheContractsForComponentType(Type systemType) {
      foreach (var contractTypeSets in systemType.GetAllInheritedGenericTypeParameters(typeof(IComponent<>.IHaveContractWith<>))) {
        (Type a, Type b, _) = contractTypeSets.ToList();
        MethodInfo contract = systemType.GetInterfaceMap(typeof(IComponent<>.IHaveContractWith<>).MakeGenericType(a, b)).TargetMethods.First(m => {
          if (m.Name.EndsWith("ExecuteContractWith"))
            if (m.GetParameters().Length == 1)
              if (m.GetParameters().First().ParameterType == b)
                if (m.ReturnType == typeof(ValueTuple<,>).MakeGenericType(a, b))
                  return true;
          return false;
        });

        if (IComponent.IHaveContract._contracts.TryGetValue(a, out var existingDic)) {
          existingDic.Add(b, _buildContractExecutor(contract));
        }
        else IComponent.IHaveContract._contracts[a] = new() {
          {b, _buildContractExecutor(contract)}
        };
      }
    }

    static Func<IComponent, IComponent, (IComponent a, IComponent b)> _buildContractExecutor(MethodInfo contract) => (a, b) => {
      var @return = (ITuple)contract.Invoke(a, new[] { b });
      return ((IComponent)@return[0], (IComponent)@return[1]);
    };

    HashSet<Type> _staticallyInitializedTypes
      = new();

    #endregion

    #region Enum Init

    bool _tryToInitializeAndBuildEnumType(PropertyInfo prop, out Exception ex) {
      Universe.ExtraContexts.OnLoaderEnumInitializationStart(prop);
      bool success = true;
      Enumeration value = null;
      ex = null;

      try {
        value = (Enumeration)prop.GetValue(null);
      }
      catch (Exception e) {
        ex = e;
        _failedEnumerations.Add(prop, e);
        success = false;
      }

      Universe.ExtraContexts.OnLoaderEnumInitializationComplete(success, prop, value, ex);
      return success;
    }

    #endregion

    #region Modifications Init

    void _tryToLoadAllModifiers() {
      IEnumerable<Type> modifierTypes = _assemblyTypesToBuild.Values
        .Select(a => a.Modifications)
        .Where(v => v is not null);

      Universe.ExtraContexts.OnLoaderAllModificationsStart(modifierTypes);
      _applyModificationsToAllTypesByAssemblyLoadOrder(modifierTypes);
      Universe.ExtraContexts.OnLoaderAllModificationsComplete();
    }

    /// <summary>
    /// Call all the the Archetype.Modifier.Initialize() functions in mod load order.
    /// </summary>
    void _applyModificationsToAllTypesByAssemblyLoadOrder(IEnumerable<Type> modifierTypes) {
      foreach (Type modifierType in modifierTypes) {
        Universe.ExtraContexts.OnLoaderModificationStart(modifierType);
        Modifications modifier = null;
        try {
          modifier
            = Activator.CreateInstance(
              modifierType,
              BindingFlags.NonPublic | BindingFlags.Instance,
              null,
              new object[] { Universe },
              null
            ) as Modifications;

          modifier.Initialize();
        }
        catch (Exception e) {
          Universe.ExtraContexts.OnLoaderModificationComplete(false, modifierType, modifier, e);
        }

        Universe.ExtraContexts.OnLoaderModificationComplete(true, modifierType, modifier, null);
      }
    }

    #endregion

    #endregion

    #region Test Build Models

    /// <summary>
    /// A function that can be used to test build models.
    /// </summary>
    public IModel GetOrBuildTestModel(Type modelBase, Universe universe) {
      if (universe.Loader._staticallyInitializedTypes.Contains(modelBase)) {
        var factory = universe.Archetypes.GetDefaultForModelOfType(modelBase);
        if (modelBase.IsAssignableFrom(factory.ModelTypeProduced)) {
          return GetOrBuildTestModel(factory, modelBase);
        }
        else throw new FailedToConfigureNewModelException($"Default archetype mismatch: {factory}. Cannot make a default test model of type:{modelBase.FullName}, with an unknown archetype if it's default archetype has not yet been set up. Usually this means the model's proper archetype has not yet been tested itself."); ;
      }
      else throw new FailedToConfigureNewModelException($"Cannot make a default test model of type:{modelBase.FullName}, with an unknown archetype if it's static constructors have not yet been called as a different archetype may have been set inthe static ctor.");
    }

    /// <summary>
    /// A function that can be used to test build models.
    /// </summary>
    public IModel GetOrBuildTestModel(Archetype factory, Type modelBase) {
      // check the cache (if there is one still)
      if ((factory.Universe.Loader._testModels?.ContainsKey(factory.GetType()) ?? false)
        && factory.Universe.Loader._testModels[factory.GetType()].ContainsKey(modelBase)
      ) {
        return factory.Universe.Loader._testModels[factory.GetType()][modelBase];
      }

      IBuilder testBuilder
        = _loadOrGetTestBuilder(
          factory,
          factory.Universe.Loader._loadOrGetTestParams(factory, modelBase),
          out (IModel model, bool hasValue)? potentiallyBuiltModel
        );

      IModel testModel;
      try {
        testModel = potentiallyBuiltModel?.model ?? factory.Make(testBuilder);
      }
      catch (Exception e) {
        Type accurateTargetType = _tryToCalculateAcurateBuilderType(modelBase, e);
        testModel = GetOrBuildTestModel(factory, accurateTargetType);
      }

      /// cache if this is the final form of le model.
      if (factory.Universe.Loader._testModels is not null && ((IFactory)factory)._modelConstructor is not null) {
        Type constructedModelType = testModel.GetType();
        if (factory.Universe.Loader._testModels.TryGetValue(factory.GetType(), out var existing)) {
          existing[constructedModelType] = testModel;
        }
        else {
          factory.Universe.Loader._testModels[factory.GetType()] = new() {
              {constructedModelType, testModel }
            };
        }
      }

      return testModel;
    }

    /// <summary>
    /// Used to try to re-targed the model type when the test build of a model type fails.
    /// </summary>
    /// <returns>If the handler handled the exception and found a new type or not</returns>
    public delegate bool TestModelBuilderTypeMismatchExceptionHandler(Type modelBaseType, Exception e, out Type? newTargetType);

    Type _tryToCalculateAcurateBuilderType(Type modelBase, Exception e) {
      Type accurateTargetType;

      (int score, TestModelBuilderTypeMismatchExceptionHandler handler)? bestHandler = null;
      foreach(var (exceptionType, handler) in Options.TestModelBuilderTypeMismatchExceptionHandlers) {
        var currentType = e.GetType();
        if (exceptionType == currentType) {
          bestHandler = (0, handler);
          break;
        } else if (exceptionType.IsAssignableFrom(currentType)) {
          int score = 0;
          while (currentType != exceptionType) {
            score++;
            currentType = currentType.BaseType;
          }

          if (!bestHandler.HasValue || bestHandler.Value.score > score) {
            bestHandler = (score, handler);
          }
        }
      }

      if (bestHandler.HasValue) {
        if (bestHandler.Value.handler(modelBase, e, out accurateTargetType!)) {
          return accurateTargetType!;
        }
      }

      // try to re-target via the called constructor.
      if (e is IBuilder.Param.IException) {
        StackTrace stackTrace = new(e);
        StackFrame lastFrame = stackTrace.GetFrame(1);
        Console.WriteLine(lastFrame);
        MethodBase method = lastFrame.GetMethod();
        if (method.Name.Contains("ctor")) {
          accurateTargetType = lastFrame.GetMethod().DeclaringType;
        }
        else {
          StackFrame[] frames = stackTrace.GetFrames();
          lastFrame = frames.First(f => {
            method = f.GetMethod();
            bool nameMatch = method.Name.Contains("ctor") && !method.Name.Contains("ModelConstructor");
            if (!nameMatch) {
              return false;
            }
            bool typeMatch = modelBase.IsAssignableFrom(method.DeclaringType);
            return typeMatch;
          });
          accurateTargetType = lastFrame.GetMethod().DeclaringType;
        }
      }
      else throw new FailedToConfigureResourceException($"Cannot Calulate Accurate Builder Type from Inner Exception", e);
      return accurateTargetType;
    }

    /// <summary>
    /// Test build a model of the given type using it's default archetype or builder.
    /// </summary>
    IModel _testBuildDefaultModel(Type systemType) {
      Archetype defaultFactory = _getDefaultFactoryBuilderForModel(systemType);

      if (defaultFactory == null) {
        throw new Exception($"Could not make a default model for model of type: {systemType.FullName}. Could not fine a default BuilderFactory or Archetype to build it with.");
      }

      IModel defaultModel;

      try {
        defaultModel
          = GetOrBuildTestModel(
              defaultFactory,
              systemType
          );

        Universe.Models._modelTypesProducedByArchetypes[defaultFactory] = defaultModel.GetType();
      }
      catch (Exception e) {
        throw new FailedToConfigureNewModelException($"Could not make a default model for model of type: {systemType.FullName}, using default archeytpe of type: {defaultFactory}.", e);
      }

      return defaultModel;
    }

    Archetype _getDefaultFactoryBuilderForModel(Type systemType)
      => systemType.IsAssignableToGeneric(typeof(IModel<,>))
        ? Universe.Archetypes.GetDefaultForModelOfType(systemType)
        : (Archetype)Universe.Models.GetFactory(systemType);

    /// <summary>
    /// Test build a model of the given type using it's default archetype or builder.
    /// </summary>
    IComponent _testBuildDefaultComponent(Type systemType) {
      if (!(Universe.Components.GetFactory(systemType) is Archetype defaultFactory)) {
        throw new Exception($"Could not make a default component model of type: {systemType.FullName}. Could not fine a default BuilderFactory to build it with.");
      }

      IModel defaultComponent;
      try {
        defaultComponent
          = GetOrBuildTestModel(
             defaultFactory,
             systemType
          );

        /// Register component key
        Universe.Components.
          _byKey[(defaultFactory as IComponent.IFactory).Key]
            = systemType;
      }
      catch (Exception e) {
        throw new Exception($"Could not make a default component model of type: {systemType.FullName}, using default archeytpe of type: {defaultFactory}. Make sure you have propper default test parameters set for the archetype", e);
      }

      return (IComponent)defaultComponent;
    }

    static IBuilder _loadOrGetTestBuilder(Archetype factory, Dictionary<string, object> @params, out (IModel model, bool hasValue)? potentiallyBuiltModel) {
      IFactory iFactory = factory;
      if (iFactory._modelConstructor is null) {
        Func<IBuilder, IModel> defaultCtor = factory.Universe.Models
          ._getDefaultCtorFor(factory.ModelTypeProduced);

        iFactory._modelConstructor
          = builder => defaultCtor.Invoke(builder);
      }

      // TODO: is this cache check needed?
      potentiallyBuiltModel = factory.Universe.Loader._testModels is not null
        && factory.Universe.Loader._testModels.ContainsKey(factory.GetType())
        && factory.Universe.Loader._testModels[factory.GetType()].ContainsKey(factory.ModelTypeProduced)
          ? (factory.Universe.Loader._testModels[factory.GetType()][factory.ModelTypeProduced], true)
          : null;

      if (potentiallyBuiltModel is null) {
        Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor
            = factory.GetGenericBuilderConstructor();

        IBuilder builder = builderCtor.Invoke(
          factory,
          @params
        );

        TestParentFactoryAttribute testParentData;
        if (typeof(IComponent).IsAssignableFrom(factory.ModelTypeProduced)) {
          if ((testParentData = factory.ModelTypeProduced.GetCustomAttribute<TestParentFactoryAttribute>()) != null) {
            (builder as IComponent.IBuilder).Parent = new DummyParent() { 
              Factory = testParentData.TestArchetypeType.AsArchetype()
            };
          }
        }

        return builder;
      }

      return null;
    }

    [Settings.DoNotBuildInInitialLoad]
    internal class DummyParent : IModel, IReadableComponentStorage, IWriteableComponentStorage {
      internal IFactory Factory { get; init; }

      IFactory IModel.Factory => Factory;

      Universe IModel.Universe { get; set; }

      /*internal IEnumerable<IModel.IComponent> Components { init 
        => value.ForEach(this.AddComponent); 
      }*/

      Dictionary<string, IComponent> IReadableComponentStorage.ComponentsByBuilderKey { get; } = new();

      Dictionary<Type, ICollection<IComponent>> IReadableComponentStorage.ComponentsWithWaitingContracts { get; } = new();
    }

    Dictionary<string, object> _loadOrGetTestParams(Archetype factoryType, Type modelType) {
      Type currentModelType = null;
      if (!factoryType.Universe.Loader.IsFinished) {
        if (_loadedTestParams?.TryGetValue(factoryType, out currentModelType) ?? false) {
          if (currentModelType != modelType) {
            return _generateTestParamsHelper(factoryType, modelType);
          }
        }
        else {
          return _generateTestParamsHelper(factoryType, modelType);
        }
      }

      return factoryType.DefaultTestParams;
    }

    Dictionary<string, object> _generateTestParamsHelper(Archetype factoryType, Type modelType) {
      Dictionary<string, object> @params = TestValueAttribute._generateTestParameters(factoryType, modelType);

      _loadedTestParams?.Set(factoryType, modelType);

      return @params.Any()
        ? (factoryType._defaultTestParams = @params)
        : (factoryType._defaultTestParams = null);
    }

    #endregion

    #region Try To Complete Init Loops

    /// <summary>
    /// Try to initialize any archetypes that failed:
    /// </summary>
    void _tryToCompleteAllEnumsInitialization() {
      _uninitializedEnums.Keys.ToList().ForEach(enumProp => {
        if (_tryToInitializeAndBuildEnumType(enumProp, out var e)) {
          _uninitializedEnums.Remove(enumProp);
        }
        else {
          _uninitializedEnums[enumProp] = e;
        }
      });
    }

    /// <summary>
    /// Try to initialize any archetypes that failed:
    /// </summary>
    void _tryToCompleteAllComponentsInitialization() {
      _uninitializedComponents.Keys.ToList().ForEach(componentSystemType => {
        if (_tryToInitializeComponent(componentSystemType, out var e)) {
          _uninitializedComponents.Remove(componentSystemType);
        }
        else {
          _uninitializedComponents[componentSystemType] = e;
        }
      });
    }

    /// <summary>
    /// Try to initialize any archetypes that failed:
    /// </summary>
    void _tryToCompleteAllModelsInitialization() {
      _uninitializedModels.Keys.ToList().ForEach(modelSystemType => {
        if (_tryToInitializeModel(modelSystemType, out var e)) {
          _uninitializedModels.Remove(modelSystemType);
        }
        else {
          _uninitializedModels[modelSystemType] = e;
        }
      });
    }

    /// <summary>
    /// Try to initialize any archetypes that failed:
    /// </summary>
    void _tryToCompleteAllArchetypesInitialization() {
      _uninitializedArchetypes.Keys.ToList().ForEach(archetypeSystemType => {
        if (_tryToInitializeArchetype(archetypeSystemType, out var e)) {
          _uninitializedArchetypes.Remove(archetypeSystemType);
        }
        else {
          _uninitializedArchetypes[archetypeSystemType] = e;
        }
      });
    }

    #endregion

    #region Finishing and Finalization

    /// <summary>
    /// Try to finish all remaining initialized archetypes:
    /// </summary>
    void _tryToFinishAllInitalizedTypes() {
      Universe.ExtraContexts.OnLoaderFinishTypesStart();
      var values = _initializedArchetypes.ToList();
      values.RemoveAll(archetype => {
        try {
          archetype.Finish();
          _finishedArchetypes.Add(archetype);

          return true;
        } // attempt failure: 
        catch (FailedToConfigureNewArchetypeException) {

          return false;
        } // attempt fatal: 
        catch (CannotInitializeArchetypeException) {
          if (Options.FatalOnCannotInitializeType) {
            throw;
          }

          return true;
        } // attempt fatal: 
        catch (Exception) {
          if (Options.FatalOnCannotInitializeType) {
            throw;
          }

          return true;
        }
      });
      _initializedArchetypes = values.ToHashSet();
      Universe.ExtraContexts.OnLoaderFinishTypesComplete();
    }

    /// <summary>
    /// Finish initialization
    /// </summary>
    void _finalize() {
      Universe.ExtraContexts.OnLoaderFinalizeStart();
      _reportOnFailedTypeInitializations();
      Universe.ExtraContexts.OnLoaderFinalizeComplete();

      _clearUnnesisaryFields();
      _clearNonLazySplayedTypesFromMemory();
      _clearCache();

      IsFinished = true;
      Universe.ExtraContexts.OnLoaderIsFinished();
    }

    void _clearNonLazySplayedTypesFromMemory() {
      /// remove all non lazy loadable types.
      ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType.Keys.ForEach(enumBaseType =>
        ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType[enumBaseType].Keys.ForEach(enumType =>
          ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType[enumBaseType][enumType].ToList().ForEach(getMethod => {
            if (!ISplayed._splayedInterfaceTypesThatAllowLazyInitializations.Contains(getMethod.Value)) {
              ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType[enumBaseType][enumType].Remove(getMethod.Key);
              if (!ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType[enumBaseType][enumType].Any()) {
                ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType[enumBaseType].Remove(enumType);
                if (!ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType[enumBaseType].Any()) {
                  ISplayed._splayedArchetypeCtorsByEnumBaseTypeAndEnumTypeAndSplayType.Remove(enumBaseType);
                }
              }
            }
          })
        )
      );

      ISplayed._splayedInterfaceTypesThatAllowLazyInitializations = null;
      ISplayed._completedEnumsByInterfaceBase = null;
    }

    void _clearUnnesisaryFields() {
      _orderedAssemblyFiles = null;
      _testModels = null;
      _loadedTestParams = null;
      _unorderedAssembliesToLoad = null;
      _assemblyTypesToBuild = null;

      _uninitializedArchetypes = null;
      _uninitializedEnums = null;
      _uninitializedComponents = null;
      _uninitializedModels = null;

      _failedComponents = null;
      _failedModels = null;
      _failedArchetypes = null;
      _failedEnumerations = null;
      _finishedArchetypes = null;

      _successfullyTestedArchetypes = null;
      _initializedArchetypes = null;
    }

    void _clearCache() {
      ICached.ClearAll();
    }

    void _reportOnFailedTypeInitializations() {
      List<Failure> failures = new();
      foreach ((Type componentType, Exception ex) in _uninitializedComponents.AddAndSetVauesFrom(_failedComponents)) {
        failures.Add(new("Component", componentType, ex));
      }
      foreach ((Type modelType, Exception ex) in _uninitializedModels.AddAndSetVauesFrom(_failedModels)) {
        failures.Add(new("Model", modelType, ex));
      }
      foreach ((Type archetypeType, Exception ex) in _uninitializedArchetypes.AddAndSetVauesFrom(_failedArchetypes)) {
        failures.Add(new("Archetype", archetypeType, ex));
      }
      foreach ((MemberInfo enumProp, Exception ex) in _failedEnumerations) {
        failures.Add(new("Enumeration", (enumProp as PropertyInfo).PropertyType, ex) {  Metadata = enumProp});
      }

      _failures.AddRange(failures);
      if ((Options.FatalOnCannotInitializeType || Options.FatalDuringFinalizationOnCouldNotInitializeTypes) && Failures.Any()) {
        throw new InvalidOperationException("Failed to initialize several types in the ECSBAM Loader:\n"
          + string.Join('\n', Failures));
      }
    }

    /// <summary>
    /// Represents a failed type that wasn't loaded during XBAM initialization
    /// </summary>
    public struct Failure {

      /// <summary>
      /// The XBam Type of the failure.
      /// Buit in options are: Archetype, Model, Enumeration and Component.
      /// </summary>
      public string XbamType { get; }

      /// <summary>
      /// The system type of the failed type
      /// </summary>
      public Type SystemType { get; }

      /// <summary>
      /// The exception that was thrown.
      /// </summary>
      public Exception Exception { get; }

      /// <summary>
      /// Other extra metadata about the failure.
      /// </summary>
      public object Metadata { get; internal init; }

      /// <summary>
      /// Make a new failure for reporting.
      /// </summary>
      public Failure(string xbamType, Type systemType, Exception exception) {
        XbamType = xbamType;
        SystemType = systemType;
        Exception = exception;
        Metadata = null;
      }

      ///<summary><inheritdoc/></summary>
      public override string ToString() {
        StringBuilder builder = new();
        builder.Append($"\n====:{XbamType}::{SystemType.ToFullHumanReadableNameString()}:====");
        builder.Append($"\n\t==Exception:== ");
        builder.Append(Exception.Message?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\n\t\t\t\t"));
        builder.Append($"\n\t\t====Stack Trace:== ({Exception.GetType().ToFullHumanReadableNameString()})\n\t\t\t\t");
        builder.Append(Exception.StackTrace?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\n\t\t\t\t"));
        builder.Append($"\n\t\t======");

        var ie = Exception.InnerException;
        while (ie is not null) {
          builder.Append($"\n\n\t\t====Inner Exception:== ");
          builder.Append(ie.Message?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\n\t\t\t\t"));
          builder.Append($"\n\t\t\t======Stack Trace:== ({ie.GetType().ToFullHumanReadableNameString()})\n\t\t\t\t");
          builder.Append(ie.StackTrace?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\n\t\t\t\t"));
          builder.Append($"\n\t\t\t========");
          builder.Append($"\n\t\t======");
          ie = ie.InnerException;
        }

        builder.Append($"\n\t====");
        builder.Append($"========");

        return builder.ToString();
      }
    }

    #endregion

    /// <summary>
    /// Go up the tree and find a declaring type that these types inherit from.
    /// </summary>
    static Type _getFirstDeclaringParent(Type archetypeSystemType) {
      if (archetypeSystemType.DeclaringType == null) {
        if (archetypeSystemType.BaseType != null) {
          return _getFirstDeclaringParent(archetypeSystemType.BaseType);
        }
        else
          return null;
      }
      else
        return archetypeSystemType.DeclaringType;
    }
  }
}