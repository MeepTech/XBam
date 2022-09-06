using Meep.Tech.Collections.Generic;
using Meep.Tech.XBam.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  /// <summary>
  /// A global collection of arechetypes.
  /// This is what the loader builds.
  /// </summary>
  public sealed partial class Universe : IUniverse {

    /// <summary>
    /// All Universes
    /// </summary>
    public static IReadOnlyDictionary<string, Universe> All
      => _all;
    static OrderedDictionary<string, Universe> _all
      = new();

    /// <summary>
    /// The default archetype universe
    /// </summary>
    public static Universe Default
      => XBam.Archetypes.DefaultUniverse;

    /// <summary>
    /// The unique key of this universe.
    /// </summary>
    public string Key {
      get;
    } = "";

    /// <summary>
    /// The loader used to build this universe
    /// </summary>
    public Loader Loader {
      get;
      internal set;
    }

    /// <summary>
    /// Archetypes data
    /// </summary>
    public ArchetypesData Archetypes {
      get;
    }

    /// <summary>
    /// Models data
    /// </summary>
    public ModelsData Models {
      get;
    }

    /// <summary>
    /// Components data
    /// </summary>
    public ComponentsData Components {
      get;
    }

    /// <summary>
    /// Enumerations Data
    /// </summary>
    public EnumerationData Enumerations {
      get;
    }

    /// <summary>
    /// The extra contexts
    /// </summary>
    public ExtraContextsData ExtraContexts {
      get;
    }

    /// <summary>
    /// Make a new universe of Archetypes
    /// </summary>
    public Universe(Loader loader, string? nameKey = null) {
      Key = nameKey ?? Key;
      Loader = loader;
      Loader.Universe = this;
      Archetypes = new(this);
      Models = new(this);
      Components = new(this);
      Enumerations = new(this);
      ExtraContexts = new ExtraContextsData(this);

      // set this as the default universe if there isn't one yet
      XBam.Archetypes.DefaultUniverse ??= this;
      _all.Add(Key, this);
    }

    /// <summary>
    /// Set an extra context item to this universe.
    /// It also adds the item to any empty indexes in the inheritance chain for the extra context type's parents.
    /// </summary>
    public void SetExtraContext<TExtraContext>(TExtraContext extraContext)
      where TExtraContext : ExtraContext {
      if (Loader.IsFinished) {
        throw new Exception($"Must add extra context before the loader for the universe has finished.");
      }

      extraContext.Universe = this;
      ExtraContexts._addAllOverrideDelegates(extraContext);

      // add the context to all types in it's inheritance chain that aren't already taken.
      var currentContextType = typeof(TExtraContext);
      if (currentContextType == typeof(ExtraContext)) {
        currentContextType = extraContext.GetType();
      }

      if (currentContextType == typeof(ExtraContext)) {
        throw new InvalidOperationException($"Cannot Set an Extra Context of Base Type: {nameof(ExtraContext)}");
      }

      var contextBaseType = currentContextType;
      while (currentContextType is not null && currentContextType != typeof(ExtraContext)) {
        if (!ExtraContexts._extraContexts.TryAdd(currentContextType, extraContext)) {
          break;
        }

        currentContextType = currentContextType.BaseType;
      }

      // check for interfaces that implement it too
      foreach(
        Type extraTypeInterface 
          in contextBaseType
            .GetInterfaces()
            .Where(i => typeof(IExtraUniverseContextType).IsAssignableFrom(i) 
              && i != typeof(IExtraUniverseContextType))
      ) {
        ExtraContexts._extraContexts.TryAdd(extraTypeInterface, extraContext);
      }
    }

    /// <summary>
    /// Set a new extracontext of this type to this universe.
    /// </summary>
    public void SetExtraContext<TExtraContext>()
      where TExtraContext : ExtraContext, new()
        => SetExtraContext<TExtraContext>(new());

    /// <summary>
    /// Get an extra context item that was assigned to this universe.
    /// </summary>
    public bool HasExtraContext<TExtraContext>()
      where TExtraContext : IExtraUniverseContextType
        => ExtraContexts._extraContexts.ContainsKey(typeof(TExtraContext));

    /// <summary>
    /// Get an extra context item that was assigned to this universe.
    /// </summary>
    public bool TryToGetExtraContext<TExtraContext>(out TExtraContext? extraContext)
      where TExtraContext : IExtraUniverseContextType {
      if (ExtraContexts._extraContexts.TryGetValue(typeof(TExtraContext), out var found)) {
        extraContext = (TExtraContext)(IExtraUniverseContextType)found;
        return true;
      }

      extraContext = default;
      return false;
    }

    /// <summary>
    /// Get an extra context item that was assigned to this universe.
    /// </summary>
    public TExtraContext? TryToGetExtraContext<TExtraContext>()
      where TExtraContext : IExtraUniverseContextType {
      if (ExtraContexts._extraContexts.TryGetValue(typeof(TExtraContext), out var found)) {
        return (TExtraContext)(IExtraUniverseContextType)found;
      }

      return default;
    }

    /// <summary>
    /// Get an extra context item that was assigned to this universe.
    /// </summary>
    public TExtraContext GetExtraContext<TExtraContext>()
      where TExtraContext : IExtraUniverseContextType {
      try {
        return (TExtraContext)(ExtraContexts._extraContexts[typeof(TExtraContext)] as IExtraUniverseContextType);
      }
      catch (KeyNotFoundException e) {
        throw new KeyNotFoundException($"No extra context of the type {typeof(TExtraContext).FullName} added to this universe. Further ECSBAM configuration may be required.", e);
      }
    }
  }
}
