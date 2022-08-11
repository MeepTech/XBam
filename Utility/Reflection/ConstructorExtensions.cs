using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Meep.Tech.Reflection {
  /// <summary>
  /// Used for constructing types.
  /// </summary>
  public static class ConstructorExtensions {
    static Dictionary<ConstructorInfo, Delegate> _activatorCache
      = new();

    /// <summary>
    /// A faster alternative to Activator.CreateInstance.
    /// </summary>
    public delegate T ObjectActivator<out T>(params object[] args);

    /// <summary>
    /// Get an activator that works quicker than Activator.CreateInstance and is cached.
    /// </summary>
    public static ObjectActivator<T> GetActivator<T>(this ConstructorInfo ctor) {
      if (_activatorCache.TryGetValue(ctor, out var activator)) {
        return activator is ObjectActivator<T> atvtr
          ? atvtr
          : throw new InvalidCastException($"Cannot cast an existing ObjectActivator of type {activator.GetType().FullName} for type {ctor.DeclaringType.FullName}. You have likely passed the wrong constructor for the type you are requesting to GetActivator.");
      }

      ParameterInfo[] paramsInfo = ctor.GetParameters();

      //create a single param of type object[]
      ParameterExpression param 
        = Expression.Parameter(typeof(object[]), "args");
      Expression[] argsExp
        = new Expression[paramsInfo.Length];

      //pick each argument from the params array and create a typed expression
      for (int i = 0; i < paramsInfo.Length; i++) {
        Expression index = Expression.Constant(i);
        Type paramType = paramsInfo[i].ParameterType;

        Expression paramAccessorExp
          = Expression.ArrayIndex(param, index);

        Expression paramCastExp 
          = Expression.Convert(paramAccessorExp, paramType);

        argsExp[i] = paramCastExp;
      }

      //make an that calls the ctor with the created arguments
      NewExpression newExp = Expression.New(ctor, argsExp);
      LambdaExpression lambda =
          Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

      //compile and cache it
      var compiled = (ObjectActivator<T>)lambda.Compile();
      _activatorCache[ctor] = compiled;
      return compiled;
    }
  }
}
