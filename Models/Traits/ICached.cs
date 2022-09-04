using Meep.Tech.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using Meep.Tech.Reflection;

namespace Meep.Tech.XBam {

  /// <summary>
  /// Base functionality for cacheable models
  /// TODO: store this data in an extra-context in the universe.
  /// </summary>
  public interface ICached : IUnique {
    /// TODO: store this data in an extra-context in the universe.
    internal static Dictionary<System.Type, Dictionary<string, IUnique>> _cacheByBaseType
      = new();

    internal System.Type _cachedBaseType 
      { get;}

    /// <summary>
    /// Set an item to the cache
    /// </summary>
    public static void Set(ICached thingToCache) {
      if (_cacheByBaseType.TryGetValue(thingToCache._cachedBaseType, out var existingDic)) {
        existingDic[thingToCache.Id()] = thingToCache;
      } else {
        _cacheByBaseType[thingToCache._cachedBaseType]
          = new() {
            {thingToCache.Id(), thingToCache}
          };
      }
    }

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static IUnique TryToGet(string modelId) {
      IUnique @return = null;
      _cacheByBaseType.Values
        .ForEach(e => 
          !e.TryGetValue(modelId, out @return));

      return @return;
    }

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static IUnique TryToGet(System.Type modelType, string modelId) {
      Dictionary<string, IUnique> containingDic
        = _cacheByBaseType.TryToGet(modelType)
          ?? _cacheByBaseType.TryToGet(modelType.GetModelBaseType())
          ?? _cacheByBaseType.TryToGet(modelType.GetFirstInheritedGenericTypeParameters(typeof(ICached<>)).First());

      return containingDic?.TryToGet(modelId);
    }

    /// <summary>
    /// Try to load an item from the cache by id.
    /// </summary>
    public static IUnique TryToGet<TModel>(string modelId)
      where TModel : ICached 
        => TryToGet(typeof(TModel), modelId);

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static bool TryToGet(string modelId, out IUnique model)
      => (model = TryToGet(modelId)) != null;

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static bool TryToGet(System.Type modelType, string modelId, out IUnique model)
      => (model = TryToGet(modelType, modelId)) != null;

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static bool TryToGet<TModel>(string modelId, out TModel model)
      where TModel : ICached
        => TryToGet(typeof(TModel), modelId, out var foundModel) 
          ? (model = (TModel)foundModel) != null 
          : (model = default) != null;

    /// <summary>
    ///  load an item from the cache by id.
    /// </summary>
    public static IUnique Get(string modelId) 
      => TryToGet(modelId) ?? throw new KeyNotFoundException($"No model cached for key: {modelId}");

    /// <summary>
    ///  load an item from the cache by id.
    /// </summary>
    public static IUnique Get(System.Type modelType, string modelId)
      => TryToGet(modelType, modelId) ?? throw new KeyNotFoundException($"No model of type: {modelType.ToFullHumanReadableNameString()} cached for key: {modelId}");

    /// <summary>
    ///  load an item from the cache by id.
    /// </summary>
    public static TModel Get<TModel>(string modelId)
      where TModel : ICached
        => (TModel)TryToGet(typeof(TModel), modelId) ?? throw new KeyNotFoundException($"No model cached for key: {modelId}");

    /// <summary>
    /// Clear the cached models by id
    /// </summary>
    public static bool Clear(string modelId) {
      var anyRemoved = false;
      foreach (var e in _cacheByBaseType.Values) {
        anyRemoved = e.Remove(modelId);
      }

      return anyRemoved;
    }

    /// <summary>
    /// Clear the cached models by id
    /// </summary>
    public static bool Clear<TModel>(TModel model)
      where TModel : ICached
    {
      if (_cacheByBaseType.TryGetValue(model._cachedBaseType, out var existingDic)) {
        return existingDic.Remove(model.Id()); 
      }

      return false;
    }

    /// <summary>
    /// Clear the cached model by id
    /// </summary>
    public static bool Clear(System.Type modelType, string modelId) {
      Dictionary<string, IUnique> containingDic
        = _cacheByBaseType.TryToGet(modelType)
          ?? _cacheByBaseType.TryToGet(modelType.GetModelBaseType())
          ?? _cacheByBaseType.TryToGet(modelType.GetFirstInheritedGenericTypeParameters(typeof(ICached<>)).First());

      return containingDic?.Remove(modelId) ?? false;
    }

    /// <summary>
    /// Clear the cached models by id
    /// </summary>
    public static bool Clear<TModel>(string modelId)
      where TModel : class, ICached
        => Clear(typeof(TModel), modelId);

    /// <summary>
    /// Clear all caches fully
    /// </summary>
    public static void ClearAll() 
      => _cacheByBaseType = new();

    /// <summary>
    /// Clear the given cache fully
    /// </summary>
    public static void ClearAll(System.Type modelType) {
      System.Type containingType = null;
      if (_cacheByBaseType.ContainsKey(modelType)) {
        containingType = modelType;
      } else {
        var key = modelType.GetModelBaseType();
        if (_cacheByBaseType.ContainsKey(key)) {
          containingType = key;
        } else {
          var typeKey = modelType.GetFirstInheritedGenericTypeParameters(typeof(ICached<>)).First();
          if (_cacheByBaseType.ContainsKey(typeKey)) {
            containingType = typeKey;
          }
        }
      }

      if (containingType is not null) {
        _cacheByBaseType.Remove(containingType);
      }
    }

    /// <summary>
    /// Clear the containing cache of the given type fully
    /// </summary>
    public static void ClearAll<TModel>()
      where TModel : class, ICached<TModel>
        => _cacheByBaseType.Remove(typeof(TModel));
  }

  /// <summary>
  /// A Model that can be cached
  /// </summary>
  public interface ICached<T> : ICached
    where T : class, ICached<T>
  {

    System.Type ICached._cachedBaseType
      => _cachedBaseType;

    internal static new System.Type _cachedBaseType
      => typeof(T);

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static new T TryToGet(string modelId)
      => (T)_cacheByBaseType.TryToGet(_cachedBaseType)?.TryToGet(modelId);

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static bool TryToGet(string modelId, out T model) {
      if (_cacheByBaseType.TryGetValue(_cachedBaseType, out var values)) {
        if (values.TryGetValue(modelId, out var found)) {
          model = (T)found;
          return true;
        }
      }

      model = default;
      return false;
    }

    /// <summary>
    /// Load an item from the cache by id.
    /// </summary>
    public new static T Get(string modelId)
      => (T)_cacheByBaseType[_cachedBaseType][modelId];

    /// <summary>
    /// Cache an item of the given type.
    /// </summary>
    public static void Set(T thingToCache) {
      if (_cacheByBaseType.TryGetValue(_cachedBaseType, out var existingDic)) {
        existingDic[thingToCache.Id()] = thingToCache;
      } else {
        _cacheByBaseType[_cachedBaseType]
          = new() {
            {thingToCache.Id(), thingToCache }
          };
      }
    }

    /// <summary>
    /// Clear the cached model of this type
    /// </summary>
    public new static bool Clear(string modelId) {
      if (_cacheByBaseType.TryGetValue(_cachedBaseType, out var existingDic)) {
        return existingDic.Remove(modelId);
      }

      return false;
    }

    /// <summary>
    /// Clear the cache fully of items of this type
    /// </summary>
    public new static void ClearAll() {
      _cacheByBaseType.Remove(_cachedBaseType);
    }

    /// <summary>
    /// Can be used to override IModel.Onfinalized.
    /// </summary>
    internal protected new IModel OnFinalized(XBam.IBuilder builder)
      => this;

    IModel IModel.OnFinalized(XBam.IBuilder builder) {
      Set((T)this);
      return OnFinalized(builder);  
    }
  }

  public static class ICachedExtensions {

    /// <summary>
    /// Cache the current item by it's id.
    /// </summary>
    /// <returns>The cached model for chaining.</returns>
    public static TModel Cache<TModel>(this TModel model)
      where TModel : class, ICached<TModel> {
        ICached<TModel>.Set(model);
      return model;
    }

    /// <summary>
    /// clear the cache for the current item by it's id.
    /// </summary>
    /// <returns>The cached model for chaining.</returns>
    public static TModel ClearCache<TModel>(this TModel model)
      where TModel : class, ICached<TModel> {
        ICached<TModel>.Clear(model.Id());
      return model;
    }
  }
}