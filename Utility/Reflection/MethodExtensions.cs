using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.Reflection {

  /// <summary>
  /// Extensions for methodinfo
  /// </summary>
  public static class MethodExtensions {

    /// <summary>
    /// Check if a type has this attribute
    /// </summary>
    public static bool HasAttribute<TAttribute>(this MethodInfo method)
      where TAttribute : Attribute
        => Attribute.IsDefined(method, typeof(TAttribute));

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
      IEnumerable<MethodInfo> potentialMethods = allMethods.Where(m => m.Name.Contains(nameof(System.Linq.Expressions.Expression.Lambda)));
      potentialMethods = potentialMethods.Where(m => m.Name.StartsWith(nameof(System.Linq.Expressions.Expression.Lambda)));
      potentialMethods = potentialMethods.Where(m => m.ContainsGenericParameters);
      potentialMethods = potentialMethods.Where(m => m.GetParameters().Count() == 2);
      potentialMethods = potentialMethods.Where(m => m.GetParameters().First().ParameterType == typeof(System.Linq.Expressions.Expression));
      potentialMethods = potentialMethods.Where(m => m.GetParameters().Last().ParameterType == typeof(IEnumerable<ParameterExpression>));

      LambdaExpression lambda = potentialMethods.First().MakeGenericMethod(type).Invoke(
          null,
          new object[] {
            body,
            paramTypes.Select(Expression.Parameter)
          }
      ) as LambdaExpression;

      return lambda.Compile();
    }
  }
}
