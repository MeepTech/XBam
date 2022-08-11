using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Meep.Tech.XBam {

  /// <summary>
  /// Interface for a single item in an Enumerable collection of values
  /// </summary>
  public interface IEnumeration {

    /// <summary>
    /// The assigned internal id of this archetype. This is only consistend within the current runtime and execution.
    /// </summary>
    int InternalId { get; }

    /// <summary>
    /// The perminant and unique external id
    /// </summary>
    object ExternalId { get; }

    /// <summary>
    /// The universe this enum is a part of
    /// </summary>
    Universe Universe { get; }

    /// <summary>
    /// The base type of this enumeration
    /// </summary>
    Type EnumBaseType { get; }
  }

  /// <summary>
  /// Base for a simple Enumerable value
  /// </summary>
  public abstract partial class Enumeration 
    : IEnumeration, IEquatable<Enumeration>, IResource
  {

    /// <summary>
    /// The current number of enums. Used for internal indexing.
    /// </summary>
    static int _currentMaxInternalEnumId 
      = 0;

    /// <summary>
    /// The assigned internal id of this archetype. This is only consistend within the current runtime and execution.
    /// </summary>
    [JsonIgnore]
    public int InternalId {
      get;
      private set;
    } = -1;

    /// <summary>
    /// The perminant and unique external id
    /// </summary>
    public object ExternalId {
      get => _externalId
        ?? throw new InvalidOperationException($"Attempted to access uninitialized Enum of type {GetType()}");
      private set {
        _registerNew(value);
      }
    } object _externalId;

    /// <summary>
    /// The universe this enum is a part of
    /// </summary>
    [JsonIgnore]
    public Universe Universe {
      get;
      private set;
    }

    /// <summary>
    /// The base type of this enumeration
    /// </summary>
    public abstract Type EnumBaseType {
      get;
    }

    /// <summary>
    /// Make a new enumeration.
    /// </summary>
    protected Enumeration(object uniqueIdentifier, Universe universe = null)
      : this(uniqueIdentifier, universe, true) { }

    internal Enumeration(object uniqueIdentifier, Universe universe, bool registerAsNew) {
      Universe = universe ?? Archetypes.DefaultUniverse;
      if (Universe is null) {
        throw new System.ArgumentNullException(nameof(Universe));
      }
      if (Universe.Enumerations is null) {
        throw new System.ArgumentNullException("Universe.Enumerations");
      }

      InternalId = Interlocked.Increment(ref _currentMaxInternalEnumId) - 1;

      if (registerAsNew) {
        _registerNew(uniqueIdentifier);
      }
    }

    void _registerNew(object uniqueIdentifier) {
      _externalId = UniqueIdCreationLogic(uniqueIdentifier);
      Universe.Enumerations._register(this);
    }

    internal void _deRegisterFromCurrentUniverse() {
      Universe.Enumerations._deRegister(this);
      InternalId = -1;
      Universe = null;
    }

    /// <summary>
    /// Used to make a unique id for an enum from the provided unique value.
    /// </summary>
    protected internal abstract object UniqueIdCreationLogic(object uniqueIdentifier);

    /// <summary>
    /// Just removes any spaces.
    /// </summary>
    public static object DefaltUniqueIdCreationLogic(Type baseType, object uniqueIdentifier) =>
      // Remove any spaces:
      Regex.Replace($"{baseType.Name}.{uniqueIdentifier}", @"\s+", "");

    #region Equality, Comparison, and Conversion

    /// <summary>
    /// ==
    /// </summary>
    /// 
    public override bool Equals(object obj)
      => Equals(obj as Enumeration);

    public bool Equals(Enumeration other) 
      => !(other is null) && other.ExternalId == ExternalId;

    public static bool operator ==(Enumeration a, Enumeration b)
      => (a is null && b is null) || (a?.Equals(b) ?? false);
     
    public static bool operator !=(Enumeration a, Enumeration b)
      => !(a == b);

    /// <summary>
    /// #
    /// </summary>
    public override int GetHashCode() {
      // TODO: test using internal id here instead for more speeeed in indexing:
      return ExternalId.GetHashCode();
    }

    public override string ToString() {
      return ExternalId.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Base for a general Enum
  /// </summary>
  /// <typeparam name="TEnumBase"></typeparam>
  public abstract class Enumeration<TEnumBase>
    : Enumeration
    where TEnumBase : Enumeration<TEnumBase>
  {

    /// <summary>
    /// Readonly list of all items from the default universe
    /// </summary>
    public static IEnumerable<TEnumBase> All
      => Archetypes.DefaultUniverse.Enumerations.GetAllByType<TEnumBase>().Cast<TEnumBase>();

    /// <summary>
    /// Readonly list of all items from the default or given universe
    /// </summary>
    public static IEnumerable<TEnumBase> GetAll(Universe universe = null)
      => (universe ?? Archetypes.DefaultUniverse).Enumerations.GetAllByType<TEnumBase>().Cast<TEnumBase>();

    /// <summary>
    /// Get the enum of this type with the given id from the default or provided universe
    /// </summary>
    public static TEnumBase Get(object externalId, Universe universe = null)
      => (universe ?? Archetypes.DefaultUniverse).Enumerations.Get<TEnumBase>(externalId);

    /// <summary>
    /// The base type of this enumeration
    /// </summary>
    public override Type EnumBaseType
      => typeof(TEnumBase);

    /// <summary>
    /// Make a new type of enumeration.
    /// </summary>
    protected Enumeration(object uniqueIdentifier, Universe universe = null) 
      : base(uniqueIdentifier, universe) { }

    internal Enumeration(object uniqueIdentifier, Universe universe, bool registerAsNew)
      : base(uniqueIdentifier, universe, registerAsNew) { }

    /// <summary>
    /// Used to make a unique id for an enum from the provided unique value.
    /// </summary>
    protected internal override object UniqueIdCreationLogic(object uniqueIdentifier)
      => DefaltUniqueIdCreationLogic(typeof(TEnumBase), uniqueIdentifier);

    /// <summary>
    /// Used to make a unique id for an enum from the provided unique value.
    /// </summary>
    public static object GetUniqueIdFromBaseObjectKey(object uniqueIdentifier)
      => All.First().UniqueIdCreationLogic(uniqueIdentifier);
  }
}
