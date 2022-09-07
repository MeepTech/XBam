using Meep.Tech.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Meep.Tech.XBam.Json.Configuration;

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
    /// The base type of this enumeration
    /// </summary>
    Type EnumBaseType { get; }
  }

  /// <summary>
  /// Base for a simple Enumerable value
  /// </summary>
  public abstract partial class Enumeration 
    : IEnumeration, IEquatable<Enumeration?>
  {
    [JsonProperty(Enumerations.JsonObjectConverter.EnumTypePropertyName)]
    string _typeKey 
      => EnumBaseType.FullName;

    /// <summary>
    /// The current number of enums. Used for internal indexing.
    /// </summary>
    static int _currentMaxInternalEnumId 
      = 0;

    /// <summary>
    /// The perminant and unique external id
    /// </summary>
    [JsonProperty(Enumerations.JsonObjectConverter.EnumKeyPropertyName)]
    public object ExternalId {
      get => _externalId
        ?? throw new InvalidOperationException($"Attempted to access uninitialized Enum of type {GetType()}");
      private set {
        _registerNew(value);
      }
    } object _externalId = null!;

    /// <summary>
    /// The assigned internal id of this archetype. This is only consistend within the current runtime and execution.
    /// </summary>
    [JsonIgnore]
    public int InternalId {
      get;
      private set;
    } = -1;

    /// <summary>
    /// The base type of this enumeration
    /// </summary>
    [JsonIgnore]
    public abstract Type EnumBaseType {
      get;
    }

    /// <summary>
    /// Make a new enumeration.
    /// </summary>
    protected Enumeration(object uniqueIdentifier)
      : this(uniqueIdentifier, true) { }

    internal Enumeration(object uniqueIdentifier, bool registerAsNew) {
      InternalId = Interlocked.Increment(ref _currentMaxInternalEnumId) - 1;
      if (registerAsNew) {
        _registerNew(uniqueIdentifier);
      }
    }

    void _registerNew(object uniqueIdentifier) {
      _externalId = UniqueIdCreationLogic(uniqueIdentifier);

      if (Universe.All.Count > 1) {
        Universe.All.Values.ForEach(u => {
          if (!u.Loader.IsFinished || u.Loader.Options.AllowLazyEnumerationRegistration) {
            u.Enumerations._register(this);
          }
        });
      } else {
        if (Universe.Default.Loader.IsFinished && !Universe.Default.Loader.Options.AllowLazyEnumerationRegistration) {
          throw new Configuration.Loader.CannotInitializeResourceException($"The Only Present Universe Does Not Allow Lazy Enumeration Registration");
        } else {
          Universe.Default.Enumerations._register(this);
        }
      }
    }

    internal void _deRegister(Universe? universe = null) {
      if (universe is null) {
        Universe.All.Values.ForEach(u => {
          u.Enumerations._deRegister(this);
        });
        InternalId = -1;
      } else {
        if (Universe.All.Count == 1) {
          _deRegister();
        } else {
          universe.Enumerations._deRegister(this);
        }
      }
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

    ///<summary><inheritdoc/></summary>
    public override bool Equals(object obj)
      => Equals(obj as Enumeration);

    ///<summary><inheritdoc/></summary>
    public bool Equals(Enumeration? other) 
      => this == other;

    ///<summary><inheritdoc/></summary>
    public static bool operator ==(Enumeration? a, Enumeration? b)
      => (a is null && b is null) || (a?.Equals(b) ?? false);

    ///<summary><inheritdoc/></summary>
    public static bool operator !=(Enumeration? a, Enumeration? b)
      => !(a == b);

    ///<summary><inheritdoc/></summary>
    public override int GetHashCode() {
      return HashCode.Combine(_typeKey, ExternalId);
    }

    ///<summary><inheritdoc/></summary>
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
    protected Enumeration(object uniqueIdentifier) 
      : base(uniqueIdentifier) { }

    internal Enumeration(object uniqueIdentifier, bool registerAsNew)
      : base(uniqueIdentifier, registerAsNew) { }

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
