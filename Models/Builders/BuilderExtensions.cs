using Meep.Tech.Reflection;
using System;

namespace Meep.Tech.XBam {
  public static class BuilderExtensions {

    #region Access Via Param

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static T Get<T>(this IBuilder builder, IModel.IBuilder.Param toFetch, T defaultValue = default) {
      if (toFetch.ValueType != null && !toFetch.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.IBuilder.Param.MissmatchException($"Param {toFetch.Key}, is clamped to the type: {toFetch.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if (builder.TryToGet(toFetch.Key, out object value)) {
        // if the value is of the requested type, return it
        if (value is T typedValue) {
          return typedValue;
        }

        // if the provided value is null, and this is nullable, return the default value
        bool canBeNull = !toFetch.ValueType.IsValueType || (Nullable.GetUnderlyingType(toFetch.ValueType) != null);
        if (canBeNull && value == null) {
          return toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          return (T)value.CastTo(toFetch.ValueType);
        }
        catch (Exception e) {
          throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {toFetch.ValueType.FullName}", e);
        }
      }

      return toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
    }

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static bool TryToGet<T>(this IBuilder builder, IModel.IBuilder.Param toFetch, out T result, T defaultValue = default) {
      if (toFetch.ValueType != null && !toFetch.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.IBuilder.Param.MissmatchException($"Param {toFetch.Key}, is clamped to the type: {toFetch.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if (builder.TryToGet(toFetch.Key, out object value)) {
        // if the value is of the requested type, return it
        if (value is T typedValue) {
          result = typedValue;

          return true;
        }

        // if the provided value is null, and this is nullable, return the default value
        bool canBeNull = !toFetch.ValueType.IsValueType || (Nullable.GetUnderlyingType(toFetch.ValueType) != null);
        if (canBeNull && value == null) {
          result = toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
          return false;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          result = (T)value.CastTo(toFetch.ValueType);
          return true;
        }
        catch (Exception e) {
          throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {toFetch.ValueType.FullName}.", e);
        }
      }

      result = toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
      return false;
    }

    /// <summary>
    /// Fetch a param from a collection. The param cannot be left out, and no defaults will be replaced.
    /// </summary>
    public static T GetRequired<T>(this IBuilder builder, IModel.IBuilder.Param toFetch) {
      if (toFetch.ValueType != null && !toFetch.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}, but the provided Param object expects a value of Type: {toFetch.ValueType.FullName}.");
      }
      if (builder.TryToGet(toFetch.Key, out object value)) {
        // if the value is of the requested type, return it
        if (value is T typedValue) {
          return typedValue;
        }

        // if the provided value is null, and this is nullable, return the provided null
        bool canBeNull = !toFetch.ValueType.IsValueType || (Nullable.GetUnderlyingType(toFetch.ValueType) != null);
        if (canBeNull && value == null) {
          return default;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          return (T)value.CastTo(toFetch.ValueType);
        }
        catch (Exception e) {
          throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param: {toFetch.ExternalId}, as Type: {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {toFetch.ValueType}.", e);
        }
      }

      throw new IModel.IBuilder.Param.MissingException($"Tried to construct a model without the required param: {toFetch} of type {typeof(T).FullName} being provided. If this is a test, try adding a default value for the empty model for this required param to the Archetype's DefaultTestParams field.");
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static IBuilder Append<T>(this IBuilder builder, IModel.IBuilder.Param toSet, T value = default) {
      if (toSet.ValueType != null && !toSet.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.IBuilder.Param.MissmatchException($"Param {toSet.Key}, is clamped to the type: {toSet.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }

      return builder.Add(toSet.Key, value);
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void AppendDefault<T>(this IBuilder builder, IModel.IBuilder.Param toSet, T defaultValue) {
      if (toSet.ValueType != null && !toSet.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.IBuilder.Param.MissmatchException($"Param {toSet.Key}, is clamped to the type: {toSet.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if (!builder.TryToGet(toSet.Key, out _)) {
        builder.Add(toSet.Key, defaultValue);
      }
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void AppendDefault<T>(this IBuilder builder, IModel.IBuilder.Param toSet) {
      if (toSet.ValueType != null && !toSet.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.IBuilder.Param.MissmatchException($"Param {toSet.Key}, is clamped to the type: {toSet.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if (!builder.TryToGet(toSet.Key, out _)) {
        builder.Add(toSet.Key, toSet.HasDefaultValue
          ? toSet.DefaultValue
          : throw new IModel.IBuilder.Param.MissingException($"Param {toSet.Key} tried to set using a default value, but it does not have one set up."));
      }
    }

    #endregion

    #region Access Via String

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static T Get<T>(this IBuilder builder, string paramKey, T defaultValue = default) {
      if (builder is not null) {
        Type valueType = typeof(T);
        if (builder.TryToGet(paramKey, out object value)) {
          // if the value is of the requested type, return it
          if (value is T typedValue) {
            return typedValue;
          }

          // if the provided value is null, and this is nullable, return the default value
          bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
          if (canBeNull && value == null) {
            return defaultValue;
          }

          // See if this can be cast to the valuetype of the param, and return it if it can.
          try {
            return (T)value.CastTo(valueType);
          } catch (Exception e) {
            throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param: {paramKey}, as Type: {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {valueType.FullName}.", e);
          }
        }
      }

      return defaultValue;
    }

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static bool TryToGet<T>(this IBuilder builder, string paramKey, out T result, T defaultValue = default) {
      if (builder is not null) {
        Type valueType = typeof(T);
        if (builder.TryToGet(paramKey, out object value)) {
          // if the value is of the requested type, return it
          if (value is T typedValue) {
            result = typedValue;
            return true;
          }

          // if the provided value is null, and this is nullable, return the default value
          bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
          if (canBeNull && value == null) {
            result = defaultValue;
            return false;
          }

          // See if this can be cast to the valuetype of the param, and return it if it can.
          try {
            result = (T)value.CastTo(valueType);
            return true;
          } catch (Exception e) {
            throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param: {paramKey}, as type {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {valueType.FullName}", e);
          }
        }
      }

      result = defaultValue;
      return false;
    }

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static T TryToGet<T>(this IBuilder builder, string paramKey, T defaultValue = default) {
      if (builder is not null) {
        Type valueType = typeof(T);
        if (builder.TryToGet(paramKey, out object value)) {
          // if the value is of the requested type, return it
          if (value is T typedValue) {
            return typedValue ?? defaultValue;
          }

          // if the provided value is null, and this is nullable, return the default value
          bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
          if (canBeNull && value == null) {
            return defaultValue;
          }

          // See if this can be cast to the valuetype of the param, and return it if it can.
          try {
            return (T)value.CastTo(valueType);
          }
          catch (Exception e) {
            throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param: {paramKey}, as type {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {valueType.FullName}", e);
          }
        }
      }

      return defaultValue;
    }

    /// <summary>
    /// Check if this has the given param
    /// </summary>
    public static bool Has(this IBuilder builder, string paramName, out System.Type paramType) { 
      if (builder.TryToGet(paramName, out var parameter)) {
        paramType = parameter.GetType();
        return true;
      }

      paramType = null;
      return false;
    }

    /// <summary>
    /// Fetch a param from a collection. The param cannot be left out, and no defaults will be replaced.
    /// </summary>
    public static T GetRequired<T>(this IBuilder builder, string paramKey) {
      if (builder is not null) {
        Type valueType = typeof(T);
        if (builder.TryToGet(paramKey, out object value)) {
          // if the value is of the requested type, return it
          if (value is T typedValue) {
            return typedValue;
          }

          // if the provided value is null, and this is nullable, return the provided null
          bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
          if (canBeNull && value == null) {
            return default;
          }

          // See if this can be cast to the valuetype of the param, and return it if it can.
          try {
            return (T)value.CastTo(valueType);
          } catch (Exception e) {
            throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param: {paramKey}, as type {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {valueType.FullName}.", e);
          }
        }
      }

      throw new IModel.IBuilder.Param.MissingException($"Tried to construct a model without the required param: {paramKey} of type {typeof(T).FullName} being provided. If this is a test, try adding a default value for the empty model for this required param to the Archetype's {nameof(Archetype.DefaultTestParams)} field.");
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void SetDefault<T>(this IBuilder builder, string parameter, T defaultValue = default) {
      if (builder is not null) {
        if (!builder.TryToGet(parameter, out _)) {
          builder.Add(parameter, defaultValue);
        }
      }
    }

    #endregion
  }
}