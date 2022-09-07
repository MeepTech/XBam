using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.XBam {

  /// <summary>
  /// An attribute that adds a value to the DefaultTestParams field of an archetype by default.
  /// </summary>
  public class TestValueBaseAttribute : Attribute {
    internal object _value;

    /// <summary>
    /// The settable test id.
    /// TODO: implement multiple tests by id.
    /// </summary>
    public string TestId {
      get;
      set;
    } = "_default";

    internal static Dictionary<string, object> _generateTestParameters(Archetype factoryType, Type modelType) {
      Dictionary<string, object> @params = factoryType.DefaultTestParams ?? new();
      foreach ((PropertyInfo property, TestValueBaseAttribute attribute) in modelType
        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Select(p => (p, a: p.GetCustomAttribute<TestValueBaseAttribute>(true)))
        .Where(e => e.a is not null)
      ) {
        if (attribute._value is null) {
          if (attribute is TestValueIsNull) {
            attribute._value = null;
          }
          else if (attribute is TestValueAttribute testValueAttrubute) {
            attribute._value ??= testValueAttrubute.Value;
          }
          else if (attribute is TestValueIsNewAttribute) {
            attribute._value ??= Activator.CreateInstance(property.PropertyType);
          }
          else if (attribute is TestValueIsEmptyEnumerableAttribute) {
            attribute._value ??= typeof(Enumerable).GetMethod(nameof(Enumerable.Empty), BindingFlags.Static | BindingFlags.Public)
              .MakeGenericMethod(property.PropertyType.GetGenericArguments().First()).Invoke(null, new object[0]);
          }
          else if (attribute is TestValueIsTestModel) {
            try {
              attribute._value ??= factoryType.Universe.Loader.GetOrBuildTestModel(property.PropertyType, factoryType.Universe);
            }
            catch (Exception ex) {
              throw new Configuration.Loader.MissingDependencyForModelException(
                $"Could not build test model of type: {modelType}, without the test model for the type {property.PropertyType}",
                ex
              );
            }
          }
          else if (attribute is GetTestValueFromMemberAttribute memberAttribute) {
            try {
              System.Type currentModelType = modelType;
              MemberInfo[] members = new MemberInfo[0];
              MemberInfo member;
              while (!members.Any() && typeof(IModel).IsAssignableFrom(currentModelType)) {
                members = currentModelType.GetMember(memberAttribute.MethodName, BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static);
                currentModelType = currentModelType.BaseType;
              }

              member = members.First();
              if (member is MethodInfo method) {
                var methodParams = method.GetParameters();
                if (methodParams.Any()) {
                  if (methodParams.Length > 1 || !typeof(Archetype).IsAssignableFrom(methodParams.First().ParameterType)) {
                    throw new ArgumentException($"GetTestValueFromMemberAttribute requires a static property, field, or method (with 0 or 1 parameter(s)). If 1 parameter is provided for a method it must be of type Archetype.");
                  }
                  attribute._value = method.Invoke(null, new object[] {
                    factoryType
                  });
                }
                else {
                  attribute._value = method.Invoke(null, new object[0]);
                }

              }
              else if (member is PropertyInfo prop) {
                attribute._value = prop.GetValue(null);
              }
              else if (member is FieldInfo field) {
                attribute._value = field.GetValue(null);
              }
            }
            catch (Exception e) {
              throw new MissingMemberException($"Member {memberAttribute.MethodName} not found, or {memberAttribute.MethodName} is not a static property, field, or method (with 0 or 1 parameter(s)). If 1 parameter is provided for a method it must be of type Archetype.", e);
            }
          }
        }

        AutoBuildAttribute autoBuildData;
        string fieldName = property.Name;
        if ((autoBuildData = property.GetCustomAttribute<AutoBuildAttribute>(true)) != null) {
          fieldName = autoBuildData.ParameterName ?? fieldName;
        }

        @params[fieldName] = attribute._value;
      }

      return @params;
    }
  }

    /// <summary>
    /// An attribute that adds a value to the DefaultTestParams field of an archetype by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueAttribute : TestValueBaseAttribute {

    /// <summary>
    /// The value of the attribute
    /// </summary>
    public virtual object Value {
      get => _value;
      init => _value = value;
    }

    /// <summary>
    /// Set the DefaultTestParam value
    /// </summary>
    /// <param name="value"></param>
    public TestValueAttribute(object value = null) {
      Value = value;
    }
  }

  /// <summary>
  /// An attribute that uses a null as the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueIsNull : TestValueBaseAttribute {

    /// <summary>
    /// Set the test value of this field to 'new()';
    /// </summary>
    public TestValueIsNull() : base() { }
  }

  /// <summary>
  /// An attribute that activates a default object of the type as a the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueIsNewAttribute : TestValueBaseAttribute {

    /// <summary>
    /// Set the test value of this field to 'new()';
    /// </summary>
    public TestValueIsNewAttribute() : base() { }
  }

  /// <summary>
  /// An attribute that tells this field to use the already generated test model of another model type as the test value for this field
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueIsTestModel : TestValueBaseAttribute {

    /// <summary>
    /// Can be used to specify a test model type for generic base classes.
    /// TODO: implement
    /// </summary>
    public System.Type TestModelType {
      get;
      init;
    }

    /// <summary>
    /// Set the test value of this field to 'new()';
    /// </summary>
    public TestValueIsTestModel() : base() { }
  }

  /// <summary>
  /// An attribute that gets a value from a local member as a the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class GetTestValueFromMemberAttribute : TestValueBaseAttribute {

    /// <summary>
    /// The name of the method to use to get the value.
    /// </summary>
    public string MethodName { 
      get;
      init;
    }

    /// <summary>
    /// Set the test value of this field to 'new()';
    /// </summary>
    public GetTestValueFromMemberAttribute(string methodName) : base() {
      MethodName = methodName;
    }
  }

  /// <summary>
  /// An attribute that activates an empty enum of the seired type as a the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueIsEmptyEnumerableAttribute : TestValueBaseAttribute {

    /// <summary>
    /// Set the test value of this field to 'Enumerable.Empty[T]()';
    /// </summary>
    public TestValueIsEmptyEnumerableAttribute() : base() { }
  }

  /// <summary>
  /// An attribute that tells the testing system in xbam to use an archetype with a dummy parent.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true)]
  public class TestParentFactoryAttribute : Attribute {

    /// <summary>
    /// The parent type to use in tests.
    /// </summary>
    public Type TestArchetypeType { get; }

    /// <summary>
    /// An attribute that tells the testing system in xbam to use a dummy archetype component with a dummy parent.
    /// </summary>
    public TestParentFactoryAttribute(System.Type TestArchetypeComponentType) : base() {
      this.TestArchetypeType = TestArchetypeComponentType;
    }
  }
}
