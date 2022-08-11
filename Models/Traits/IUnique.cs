using Meep.Tech.Noise;
using Meep.Tech.Reflection;
using System;
using System.Collections.Generic;

namespace Meep.Tech.XBam {

  /// <summary>
  /// An unique object with a unique id.
  /// </summary>
  public interface IUnique : IModel {

    /// <summary>
    /// The Unique Id of this Item
    /// </summary>
    public string Id {
      get;
      internal protected set;
    }

    /// <summary>
    /// If the id should be automatically set by the IModel.Builder
    /// </summary>
    bool AutoSetIdOnBuild
      => true;

    /// <summary>
    /// If the id should be allowed to be passed in as a param.
    /// </summary>
    bool AllowCustomUniqueIdAsParam
      => true;

    /// <summary>
    /// If AllowCustomUniqueIdAsParam is false, and an Id is provided anyway, should an exception be thrown.
    /// </summary>
    bool ThrowExceptionIfCustomIdIsNotAllowed
      => false;

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// </summary>
    public IUnique Copy(bool newUniqueId = true) {
      IUnique copy = (IUnique)(this as IModel).Copy();
      if (newUniqueId) {
        copy.InitializeId();
      }

      return copy;
    }

    /// <summary>
    /// Used to internally initialize the unique id.
    /// You can use this in a custom constructor to avoid using a builder to build the unique id.
    /// </summary>
    internal protected IUnique InitializeId(string? providedId = null) {
      var idMaker = _getIdGetter(this);
      if (idMaker != null) {
        Id = idMaker(this, providedId);
      }

      return this;
    }


    #region Id Setup

    private static readonly Dictionary<System.Type, IdGetter> _idGetters
      = new();

    internal delegate string IdGetter(IUnique model, string? value);

    internal static IdGetter _getIdGetter(IUnique model)
       => _idGetters.TryGetValue(model.GetType(), out IdGetter setter)
         ? setter
         : (_idGetters[model.GetType()] = _buildIdGetter(model));

    private static IdGetter _buildIdGetter(IUnique model) {
      if (model.AutoSetIdOnBuild) {
        if (model.AllowCustomUniqueIdAsParam) {
          return ((IUnique model, string? providedId) => {
            if (providedId is not null) {
              return providedId;
            }
            else if (model.Id is null) {
              return RNG.GenerateNextGuid();
            }
            else return null;
          });
        }
        else if (model.ThrowExceptionIfCustomIdIsNotAllowed) {
          return ((IUnique model, string? providedId) => {
            if (providedId is not null) {
              throw new ArgumentException($"Cannot pass in Id param for model of type: {model.GetType().ToFullHumanReadableNameString()}. Field: {nameof(IUnique)}.{nameof(IUnique.AllowCustomUniqueIdAsParam)} is false");
            }
            else if (model.Id is null) {
              return RNG.GenerateNextGuid();
            }
            else return null;
          });
        }
        else {
          return ((model, _) => {
            if (model.Id is null) {
              return RNG.GenerateNextGuid();
            }
            else return null;
          });
        }
      }
      else return null;
    }

    #endregion
  }

  /// <summary>
  /// Extensions and helpers for IUnique
  /// </summary>
  public static class IUniqueExtensions {

    /// <summary>
    /// Get the unique id
    /// </summary>
    public static string Id(this IUnique unique)
      => unique.Id;

    /// <summary>
    /// Get the unique id
    /// </summary>
    public static string GetUniqueId(this IUnique unique)
      => unique.Id;

    /// <summary>
    /// Copy a unique model, with a new unique id
    /// Override via IUnique.copy(bool)
    /// </summary>
    public static IUnique Copy(this IUnique original, bool newUniqueId = true) 
      => original.Copy(newUniqueId);
  }
}