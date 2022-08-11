using Meep.Tech.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  /// <summary>
  /// Base functionality for cacheable models
  /// </summary>
  public interface ICached : IUnique {
    internal static Dictionary<string, IUnique> _cache
      = new();

    /// <summary>
    /// Set an item to the cache
    /// </summary>
    public static void Set(ICached thingToCache) 
      => _cache.Add(thingToCache.Id, thingToCache);

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static IUnique TryToGetFromCache(string modelId) 
      => _cache.TryGetValue(modelId, out IUnique fetchedModel)
        ? fetchedModel
        : null;

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static bool TryToGetFromCache(string modelId, out IUnique model)
      => _cache.TryGetValue(modelId, out model);

    /// <summary>
    ///  load an item from the cache by id.
    /// </summary>
    public static IUnique GetFromCache(string modelId)
      => _cache[modelId];

    /// <summary>
    /// Clear the cached model
    /// </summary>
    public static void Clear(string modelId) 
      => _cache.Remove(modelId);

    /// <summary>
    /// Clear all caches fully
    /// </summary>
    public static void ClearAll() 
      => _cache = new();
  }

  /// <summary>
  /// A Model that can be cached
  /// </summary>
  public interface ICached<T> : ICached
    where T : class, ICached<T>
  {

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static new T TryToGetFromCache(string modelId) {
      IUnique fetched = null;
      try { 
        return (fetched = ICached.TryToGetFromCache(modelId)) as T; 
      } catch (InvalidCastException e) {
        throw new InvalidCastException($"Fetched Model From Cache with ID {modelId} is likely not of type {typeof(T).FullName}. Actual type: {fetched?.GetType().FullName ?? "NULL"}", e);
      };
    }

    /// <summary>
    /// Load an item from the cache by id.
    /// </summary>
    public new static T GetFromCache(string modelId) 
      => (T)ICached.GetFromCache(modelId);

    /// <summary>
    /// Try to load an item from the cache by id
    /// </summary>
    public static bool TryToGetFromCache(string modelId, out T found)
      => (found = TryToGetFromCache(modelId)) != null;

    /// <summary>
    /// Cache an item of the given type.
    /// </summary>
    public static void Set(T thingToCache) 
      => _cache[thingToCache.Id] = thingToCache;

    /// <summary>
    /// Clear the cached model of this type
    /// </summary>
    public new static void Clear(string modelId) {
      if (_cache.Remove(modelId, out IUnique value)) {
        if (value is not T) {
          _cache.Add(modelId, value);
          throw new InvalidCastException();
        }
      }
    }

    /// <summary>
    /// Clear the cache fully of items of this type
    /// </summary>
    public new static void ClearAll() 
      => _cache.Values.Where(m => m is T)
        .ForEach(m => _cache.Remove(m.Id));

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
  }
}