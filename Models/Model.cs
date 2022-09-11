using KellermanSoftware.CompareNetObjects;
using Meep.Tech.XBam.Json;
using Meep.Tech.XBam.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Meep.Tech.XBam {

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system.
  /// This is the non-generic base class for Utility
  //c# get the type of a null object/ </summary>
  public abstract partial class Model
    : IModel 
  {

    /// <summary>
    /// The universe this model was made inside of
    /// </summary>
    [JsonIgnore]
    public Universe Universe {
      get;
      internal set;
    } Universe IModel.Universe { 
      get => Universe;
      set => Universe = value; 
    }

  IFactory IModel.Factory 
      => throw new NotImplementedException();

    #region Equality

    ///<summary><inheritdoc/></summary>
    public override bool Equals(object obj)
      => Equals(obj, out _);

    ///<summary><inheritdoc/></summary>
    public virtual bool Equals(object obj, out ComparisonResult? result) {
      // must be this type or a child
      if (obj is not null && !GetType().IsAssignableFrom(obj.GetType())) {
        result = null;

        return false;
      }

      // unique are easy
      if (obj is IUnique other && this is IUnique current) {
        result = null;

        return other.Id() == current.Id();
      } else {
        CompareLogic compareLogic = Universe.Models.GetCompareLogic(GetType());
        result = compareLogic.Compare(this, obj as IModel);

        return result.AreEqual;
      }
    }

    #endregion

    #region Serialization and Deserialization

    string _original;
    bool _isSerializing;
    ModelLog _logger;
    [OnDeserializing]
    void _onDeserializing(StreamingContext context) {
      if (_logger is not null || this.CanLog(out _logger)) {
        _original = this.ToJson().ToString();
      }
    }

    [OnSerializing]
    void _onSerializing(StreamingContext context) {
      if (!_isSerializing) {
        _isSerializing = true;
        if (_logger is not null || this.CanLog(out _logger)) {
          _original = this.ToJson().ToString();
        }
      }
    }

    [OnDeserialized]
    void _onDeserialized(StreamingContext context) {
      if (_logger is not null) {
        ((IReadableComponentStorage)this).Log(
          ModelLog.Entry.ActionType.Deserialized,
          _original,
          metadata: new Dictionary<string, object> {
            { ModelLog.Entry.MetadataField.FromJson.Key, "json".Equals(context.Context) }
          }
        );
        _original = null;
      }
    }

    [OnSerialized]
    void _onSerialized(StreamingContext context) {
      if (!_isSerializing && _logger is not null) {
        ((IReadableComponentStorage)this).Log(
          ModelLog.Entry.ActionType.Serialized,
          _original,
          metadata: new Dictionary<string, object> {
            { ModelLog.Entry.MetadataField.FromJson.Key, "json".Equals(context.Context) }
          }
        );
        _original = null;
        _isSerializing = false;
      }
    }

    #endregion
  }

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system, and uses a built in default Builder as it's base archetype.
  /// </summary>
  public abstract partial class Model<TModelBase>
    : Model, IModel<TModelBase>
    where TModelBase : Model<TModelBase> 
  {

    /// <summary>
    /// The factory that was used to make this object
    /// </summary>
    [ArchetypeProperty]
    public IModel.IFactory Factory {
      get;
      private set;
    } IFactory IModel.Factory 
      => Factory;

    #region Initialization

    /// <summary>
    /// Can be used to initialize a model after the ctor call in xbam
    /// </summary>
    protected virtual Model<TModelBase> OnInitialized(IBuilder<TModelBase> builder)
      => this;

    IModel IModel.OnInitialized(Archetype archetype, Universe universe, IBuilder builder) {
      Factory = (IModel.IFactory)(archetype ?? builder.Archetype);
      Universe = (universe ?? builder?.Archetype.Universe ?? Universe.Default);

      return OnInitialized((IBuilder<TModelBase>)builder);
    }

    /// <summary>
    /// Can be used to finalize a model after the components and everything else is set up.
    /// </summary>
    protected virtual Model<TModelBase> OnFinalized(IBuilder<TModelBase> builder)
      => this;

    IModel IModel.OnFinalized(IBuilder builder) {
      return OnFinalized((IBuilder<TModelBase>)builder);
    }

    #endregion
  }

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system.
  /// </summary>
  public abstract partial class Model<TModelBase, TArchetypeBase>
    : Model, IModel<TModelBase, TArchetypeBase>, IResource
    where TModelBase : IModel<TModelBase, TArchetypeBase> 
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {

    /// <summary>
    /// Default collection of archetypes for this model type based on the Default Univese
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.Collection Types
      => (Archetype<TModelBase, TArchetypeBase>.Collection)
        Archetypes.DefaultUniverse.Archetypes.GetCollection(typeof(TArchetypeBase));

    /// <summary>
    /// The model's archetype:
    /// </summary>
    [ArchetypeProperty]
    public TArchetypeBase Archetype {
      get;
      private set;
    } TArchetypeBase IModel<TModelBase, TArchetypeBase>.Archetype { 
      get => Archetype; 
      set => Archetype = value;
    } IFactory IModel.Factory
      => Archetype;

    #region Initialization

    /// <summary>
    /// Can be used to initialize a model after the ctor call in xbam
    /// </summary>
    protected virtual Model<TModelBase, TArchetypeBase> OnInitialized(IBuilder<TModelBase> builder)
      => this;

    IModel IModel.OnInitialized(Archetype archetype, Universe universe, IBuilder builder) {
      Archetype = (TArchetypeBase)(archetype ?? builder?.Archetype);
      Universe = (universe ?? builder?.Archetype.Universe ?? Universe.Default);

      return OnInitialized((IBuilder<TModelBase>)builder);
    }

    #endregion
  }
}