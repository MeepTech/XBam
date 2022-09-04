using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Meep.Tech.Reflection {

  /// <summary>
  /// Shortcuts and caching for casting types for xbam
  /// </summary>
  public static class TypeExtensions {

    static readonly HashSet<Type> _numericTypes = new(){
      typeof(int), 
      typeof(double),
      typeof(decimal),
      typeof(long), 
      typeof(short),
      typeof(sbyte),
      typeof(byte),
      typeof(ulong),
      typeof(ushort),
      typeof(uint),
      typeof(nint),
      typeof(float),
      typeof(nuint)
    };

    /// <summary>
    /// Check if a type is a numeric value type.
    /// </summary>
    public static bool IsNumeric(this Type type) 
      => _numericTypes.Contains(type) ||
        _numericTypes.Contains(Nullable.GetUnderlyingType(type));

    static readonly HashSet<Type> _stringTypes = new(){
      typeof(string),
      typeof(char[]),
      typeof(char)
    };

    /// <summary>
    /// Check if a type is a basic text type.
    /// string, char, or char[]
    /// </summary>
    public static bool IsText(this Type type) 
      => _stringTypes.Contains(type) ||
        _stringTypes.Contains(Nullable.GetUnderlyingType(type));

    #region Inheritance Testing

    /// <summary>
    /// Check if a given type is assignable to a generic type
    /// </summary>
    /// <param name="givenType"></param>
    /// <param name="genericType"></param>
    /// <returns></returns>
    public static bool IsAssignableToGeneric(this Type givenType, Type genericType) {
      var interfaceTypes = givenType.GetInterfaces();

      foreach(var it in interfaceTypes) {
        if(it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
          return true;
      }

      if(givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        return true;

      Type baseType = givenType.BaseType;
      if(baseType == null)
        return false;

      return baseType.IsAssignableToGeneric(genericType);
    }

    /// <summary>
    /// Get the generic arguments from a type this inherits from
    /// </summary>
    public static IEnumerable<Type> GetFirstInheritedGenericTypeParameters(this Type type, Type genericParentType) {
      foreach(Type intType in type.GetParentTypes()) {
        if(intType.IsGenericType && intType.GetGenericTypeDefinition() == genericParentType) {
          return intType.GetGenericArguments();
        }
      }

      return Enumerable.Empty<Type>();
    }

    /// <summary>
    /// Get the generic arguments from a type this inherits from
    /// </summary>
    public static IEnumerable<IEnumerable<Type>> GetAllInheritedGenericTypeParameters(this Type type, Type genericParentType) {
      foreach(Type intType in type.GetParentTypes()) {
        if(intType.IsGenericType && intType.GetGenericTypeDefinition() == genericParentType) {
          yield return intType.GetGenericArguments();
        }
      }
    }

    /// <summary>
    /// Get the generic arguments from a type this inherits from
    /// </summary>
    public static IEnumerable<Type> GetAllInheritedGenericTypes(this Type type, Type genericParentType) {
      foreach (Type intType in type.GetParentTypes()) {
        if (intType.IsGenericType && intType.GetGenericTypeDefinition() == genericParentType) {
          yield return intType;
        }
      }
    }

    /// <summary>
    /// Get all parent types and interfaces 
    /// </summary>
    public static IEnumerable<Type> GetParentTypes(this Type type) {
      // is there any base type?
      if(type == null) {
        yield break;
      }

      // return all implemented or inherited interfaces
      foreach(var i in type.GetInterfaces()) {
        yield return i;
      }

      // return all inherited types
      var currentBaseType = type.BaseType;
      while(currentBaseType != null) {
        yield return currentBaseType;
        currentBaseType = currentBaseType.BaseType;
      }
    }

    /// <summary>
    /// Get the depth of inheritance of a type
    /// </summary>
    public static int GetDepthOfInheritance(this Type type) {
      int index = 0;
      while (type.BaseType != null) {
        index++;
        type = type.BaseType;
      }

      return index;
    }

    #endregion

    #region Casting

    static readonly Dictionary<Tuple<Type, Type>, Func<object, object>> _castCache
      = new();

    /// <summary>
    /// Tries to cast an object to a given type. First time is expensive
    /// </summary>
    public static TTarget CastTo<TTarget>(this object @object)
      => (TTarget)@object.CastTo(typeof(TTarget));

    /// <summary>
    /// Tries to cast an object to a given type. First time is expensive
    /// </summary>
    public static object CastTo(this object @object, Type type) {
      Func<object, object> castDelegate = _getCastDelegate(@object.GetType(), type);
      try {
        return castDelegate.Invoke(@object);
      }
      catch (InvalidCastException e) {
        var key = new Tuple<Type, Type>(@object.GetType(), type);
        _castCache[key] = null;

        throw e;
      }
    }

    /// <summary>
    /// Tries to cast an object to a given type. First time is expensive
    /// </summary>
    public static bool TryToCastTo<TTarget>(this object @object, out TTarget result) {
      if (@object.TryToCastTo(typeof(TTarget), out var resultObject)) {
        result = (TTarget)resultObject;
        return true;
      }

      result = default;
      return false;
    }

    /// <summary>
    /// Tries to cast an object to a given type. First time is expensive
    /// </summary>
    public static bool TryToCastTo(this object @object, Type type, out object result) {
      if(_tryToGetCastDelegate(@object.GetType(), type, out var @delegate)) {
        try {
          result = @delegate.Invoke(@object);
          return true;
        } catch(InvalidCastException) {
          var key = new Tuple<Type, Type>(@object.GetType(), type);
          _castCache[key] = null;
        }
      }

      result = default;
      return false;
    }

    static Func<object, object> _buildCastDelegate(Type originalType, Type resultingType) {
      var inputObject = Expression.Parameter(typeof(object));
      return Expression.Lambda<Func<object, object>>(
          Expression.Convert(
            Expression.ConvertChecked(
              Expression.Convert(inputObject, originalType),
              resultingType
            ),
            typeof(object)
          ),
          inputObject
        ).Compile();
    }

    static Func<object, object> _getCastDelegate(Type from, Type to) {
      lock (_castCache) {
        var key = new Tuple<Type, Type>(from, to);
        if (!_castCache.TryGetValue(key, out Func<object, object> cast_delegate)) {
          try {
            cast_delegate = _buildCastDelegate(from, to);
          }
          catch (Exception e) {
            throw new NotImplementedException($"No cast found from:\n\t{from.FullName},\nto:\n\t{to.FullName}.\n\n{e}", e);
          }
          _castCache.Add(key, cast_delegate);
        }

        return cast_delegate ?? throw new NotImplementedException($"No cast found from:\n\t{from.FullName},\nto:\n\t{to.FullName}.");
      }
    }

    static bool _tryToGetCastDelegate(Type from, Type to, out Func<object, object>? cast_delegate) {
      lock (_castCache) {
        var key = new Tuple<Type, Type>(from, to);
        if (!_castCache.TryGetValue(key, out cast_delegate)) {
          try {
            cast_delegate = _buildCastDelegate(from, to);
          }
          catch (Exception e) {
            cast_delegate = null;
          }
          _castCache.Add(key, cast_delegate);
        }

        return cast_delegate is not null;
      }
    }

    #endregion

    /// <summary>
    /// Can be used to get any type by it's full name. Searches all assemblies and returns first match.
    /// </summary>
    public static Type GetTypeByFullName(string typeName) {
      Type type = Type.GetType(typeName);
      if (type != null) {
        return type;
      }

      foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        type = assembly.GetType(typeName);
        if (type != null) {
          return type;
        }
      }

      return null;
    }

    /// <summary>
    /// Try to get an attribute
    /// </summary>
    public static bool TryToGetAttribute<TAttribute>(this System.Type type, out TAttribute attribute)
      where TAttribute : Attribute
        => (attribute = type.GetCustomAttribute<TAttribute>()) != null;

    /// <summary>
    /// Check if a type has this attribute
    /// </summary>
    public static bool HasAttribute<TAttribute>(this System.Type type)
      where TAttribute : Attribute
        => Attribute.IsDefined(type, typeof(TAttribute));

    /// <summary>
    /// Build a no opp function for a delegate type
    /// </summary>
    public static Delegate BuildNoOpDelegate(this System.Type type) {
      if (!typeof(Delegate).IsAssignableFrom(type)) {
        throw new ArgumentException($"Cannot build a no opperation delegate for a non delegate type");
      }

      var invoke = type.GetMethod(nameof(Action.Invoke));

      var paramTypes = invoke.GetParameters().Select(c => c.ParameterType);

      // return default(TReturn) or default(Void)
      var body = Expression.Default(invoke.ReturnType);

      MethodInfo[] allMethods = typeof(Expression).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      IEnumerable<MethodInfo> _potentialMethods = allMethods.Where(m => m.Name.Contains(nameof(System.Linq.Expressions.Expression.Lambda)));
      _potentialMethods = _potentialMethods.Where(m => m.Name.StartsWith(nameof(System.Linq.Expressions.Expression.Lambda)));
      _potentialMethods = _potentialMethods.Where(m => m.ContainsGenericParameters);
      _potentialMethods = _potentialMethods.Where(m => m.GetParameters().Count() == 2);
      _potentialMethods = _potentialMethods.Where(m => m.GetParameters().First().ParameterType == typeof(System.Linq.Expressions.Expression));
      _potentialMethods = _potentialMethods.Where(m => m.GetParameters().Last().ParameterType == typeof(IEnumerable<ParameterExpression>));

      LambdaExpression lambda = _potentialMethods.First().MakeGenericMethod(type).Invoke(
          null,
          new object[] {
            body,
            paramTypes.Select(Expression.Parameter)
          }
      ) as LambdaExpression;

      return lambda.Compile();
    }

    /// <summary>
    /// Get a clean, easier to read type name that's still fully qualified.
    /// </summary>
    public static string ToFullHumanReadableNameString(this Type type, bool withNamespace = true, bool includeGenerics = true, IEnumerable<Type> genericTypeOverrides = null) {
      if (type.IsGenericParameter) {
        return type.Name;
      }

      if (!type.IsGenericType) {
        return withNamespace 
          ? type.FullName 
          : type.FullName[(type.Namespace.Length + 1)..];
      }

      System.Text.StringBuilder builder
        = new();

      if (withNamespace) {
        builder.Append(type.Namespace);
        builder.Append(".");
      }

      if (type.DeclaringType != null) {
        builder.Append(ToFullHumanReadableNameString(type.DeclaringType, false, includeGenerics, type.GetGenericArguments()));
        builder.Append(".");
      }

      if (!type.Name.Contains("`")) {
        builder.Append(type.Name);
        return builder.ToString();
      } else
        builder.Append(type.Name[..type.Name.IndexOf("`")]);

      if (includeGenerics) {
        builder.Append('<');
        bool first = true;
        foreach (Type genericTypeArgument in genericTypeOverrides ?? type.GetGenericArguments()) {
          if (!first) {
            builder.Append(',');
          }
          builder.Append(genericTypeArgument.ToFullHumanReadableNameString(withNamespace));
          first = false;
        }
        builder.Append('>');
      }

      return builder.ToString();
    }
  }
}
