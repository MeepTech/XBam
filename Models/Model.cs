using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Meep.Tech.XBam {

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system.
  /// This is the non-generic base class for Utility
  /// </summary>
  public abstract partial class Model
    : IModel 
  {

    /// <summary>
    /// Deserialize a model from json as a Model
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public static Model FromJson(
       JObject jObject,
       Type deserializeToTypeOverride = null,
       Universe universeOverride = null,
       params (string key, object value)[] withConfigurationParameters
     ) => (Model)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// Deserialize a model from json as a Model
    /// </summary>
    /// <typeparam name="TModel">The type to cast the produced model to. Not the same as deserializeToTypeOverride</typeparam>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public static TModel FromJsonAs<TModel>(
      JObject jObject,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      params (string key, object value)[] withConfigurationParameters
     ) where TModel : Model
      => (TModel)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

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

    ///<summary><inheritdoc/></summary>
    public override bool Equals(object obj) {
      // must be this type or a child
      if(obj is not null && !GetType().IsAssignableFrom(obj.GetType())) {
        return false;
      }

      // unique are easy
      if(obj is IUnique other && this is IUnique current)
        return other.Id == current.Id;
      else {
        CompareLogic compareLogic = Universe.Models.GetCompareLogic(GetType());
        ComparisonResult result = compareLogic.Compare(this, obj as IModel);

        return result.AreEqual;
      }
    }
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

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModelBase FromJson(
      JObject jObject,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      params (string key, object value)[] withConfigurationParameters
     ) => (TModelBase)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <typeparam name="TModel">The type to cast the produced model to. Not the same as deserializeToTypeOverride</typeparam>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModel FromJsonAs<TModel>(
      JObject jObject,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      params (string key, object value)[] withConfigurationParameters
     ) where TModel : TModelBase
      => (TModel)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// Can be used to initialize a model after the ctor call in xbam
    /// </summary>
    protected virtual Model<TModelBase> OnInitialized(IBuilder<TModelBase> builder)
      => this;

    IModel IModel.OnInitialized(Archetype archetype, Universe universe, IBuilder builder) {
      Factory = (IModel.IFactory)(archetype ?? builder.Archetype);
      Universe = (universe ?? builder?.Archetype.Id.Universe ?? Universe.Default);

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
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModelBase FromJson(
       JObject jObject,
       Type deserializeToTypeOverride = null,
       Universe universeOverride = null,
       params (string key, object value)[] withConfigurationParameters
     ) => (TModelBase)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <typeparam name="TModel">The type to cast the produced model to. Not the same as deserializeToTypeOverride</typeparam>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModel FromJsonAs<TModel>(
       JObject jObject,
       Type deserializeToTypeOverride = null,
       Universe universeOverride = null,
       params (string key, object value)[] withConfigurationParameters
     ) where TModel : TModelBase
      => (TModel)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

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
    }

    /// <summary>
    /// Can be used to initialize a model after the ctor call in xbam
    /// </summary>
    protected virtual Model<TModelBase, TArchetypeBase> OnInitialized(IBuilder<TModelBase> builder)
      => this;

    IModel IModel.OnInitialized(Archetype archetype, Universe universe, IBuilder builder) {
      Archetype = (TArchetypeBase)(archetype ?? builder?.Archetype);
      Universe = (universe ?? builder?.Archetype.Id.Universe ?? Universe.Default);

      return OnInitialized((IBuilder<TModelBase>)builder);
    }

    /// <summary>
    /// Make shortcut.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel>(TArchetypeBase type, Action<IModel.IBuilder> builderConfiguration = null)
      where TDesiredModel : TModelBase
        => type.Make<TDesiredModel>(builder => { 
          builderConfiguration(builder); 
          return builder;
        });

    /// <summary>
    /// Make shortcut.
    /// </summary>
    public static TModelBase Make(TArchetypeBase type, Action<IModel.IBuilder> builderConfiguration = null)
        => type.Make<TModelBase>(builder => { 
          builderConfiguration(builder); 
          return builder;
        });
  }
}