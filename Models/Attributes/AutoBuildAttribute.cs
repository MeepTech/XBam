using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Meep.Tech.Reflection;

namespace Meep.Tech.XBam {

  /// <summary>
  /// An attribute signifying that this field should be auto incluided in the builder constructor for this model.
  /// <para>Works with DefaultAttribute, RequiredAttribute, and NotNullAttribute</para>
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class AutoBuildAttribute : Attribute {

    /// <summary>
    /// Gets a default value for an auto-built field on a model given the model being built and the builder.
    /// </summary>
    /// <returns>The default value if the parameter isn't supplied by the builder.</returns>
    public delegate object DefaultValueGetter(IBuilder builder, IModel model);

    /// <summary>
    /// Validates the final value.
    /// </summary>
    /// <param name="validationError">(optional returnable exception)</param>
    public delegate bool ValueValidator(object value, IBuilder builder, IModel model, out string message, out System.Exception validationError);

    /// <summary>
    /// A value getter.
    /// </summary>
    /// <returns>the gotten value</returns>
    public delegate object ValueGetter(IModel model, IBuilder builder, PropertyInfo propertyInfo, AutoBuildAttribute attribute, bool isRequired);

    /// <summary>
    /// A value setter.
    /// </summary>
    public delegate void ValueSetter(IModel model, object value);

    /// <summary>
    /// If this field must not be null before being returned by the auto build step.
    /// </summary>
    public bool NotNull {
      get => _notNull ??= false;
      set => _notNull = value;
    } internal bool? _notNull;

    /// <summary>
    /// If this field's parameter must be provided to the builder
    /// </summary>
    public bool IsRequiredAsAParameter {
      get;
      set;
    } = false;
    internal bool _checkedRequired;

    /// <summary>
    /// An overrideable getter
    /// </summary>
    public virtual ValueGetter Getter {
      get;
      set;
    }

    /// <summary>
    /// An overrideable validator
    /// </summary>
    public virtual ValueValidator Validator {
      get;
      set;
    }

    /// <summary>
    /// The setter
    /// </summary>
    protected internal virtual ValueSetter Setter {
      get;
      protected set;
    }

    /// <summary>
    /// Optional override name for the parameter expected by the builder to build this property with.
    /// Defaults to the name of the property this is attached to.
    /// </summary>
    public string ParameterName {
      get;
      init;
    }

    /// <summary>
    /// The property on the archetype that will be used as a default if no value is provided to the builder.
    /// This defaults to: "Default" + ParameterName.
    /// DefaultAttribute will override this default behavior.
    /// </summary>
    public string DefaultArchetypePropertyName {
      get;
      init;
    }

    /// <summary>
    /// The name of a DefaultValueGetter delegate field that can be used to set the default value of this property instead of the DefaultArchetypePropertyName
    /// Defaults to null and uses DefaultArchetypePropertyName instead.
    /// DefaultAttribute will override this default behavior.
    /// </summary>
    public string DefaultValueGetterDelegateName {
      get;
      init;
    }

    /// <summary>
    /// The name of the optional value validator.
    /// This can be a field, property, or funciton that matches the delegate ValueValidator on this same model.
    /// </summary>
    public string ValueValidatorName {
      get;
      init;
    }

    /// <summary>
    /// The build order. Defaults to 0.
    /// </summary>
    public int Order {
      get;
      init;
    } = 0;

    /// <summary>
    /// If this is true, this field will only be auto-set from the default value, not the provided one.
    /// </summary>
    public bool FromDefaultOnly {
      get;
      init;
    } = false;

    /// <summary>
    /// Mark a field for auto-inclusion in the default builder ctor
    /// </summary>
    public AutoBuildAttribute() { }

    /// <summary>
    /// An exception thrown by the auto-builder.
    /// </summary>
    public class Exception : System.ArgumentException {

      /// <summary>
      /// The model type that threw the exepction 
      /// </summary>
      public System.Type ModelTypeBeingBuilt { get; }

      ///<summary><inheritdoc/></summary>
      public Exception(System.Type modelType, string message, System.Exception innerException) 
        : base(message, innerException) { ModelTypeBeingBuilt = modelType; }
    }

    internal static IEnumerable<(string name, Func<IModel, IBuilder, IModel> function)> _generateAutoBuilderSteps(System.Type modelType, Universe universe)
      => modelType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .Select(p => (p, a: p.GetCustomAttribute<AutoBuildAttribute>(true)))
        .Where(e => e.a is not null)
        .Select(e => {
          universe.ExtraContexts.OnLoaderAutoBuildPropertyCreationStart(modelType, e.a, e.p);

          var entry = (
            order: e.a.Order,
            name: e.a.ParameterName ?? e.p.Name,
            func: new Func<IModel, IBuilder, IModel>((m, b) 
              => _autoBuildModelProperty(m, b, e.p, e.a, modelType))
          );

          universe.ExtraContexts.OnLoaderAutoBuildPropertyCreationComplete(modelType, e.a, e.p, entry);
          return entry;
        }).OrderBy(e => e.order)
        .Select(e => (e.name, e.func));

    /// <summary>
    /// TODO: this needs to be heavily optomized.
    /// </summary>
    static IModel _autoBuildModelProperty(IModel m, IBuilder b, PropertyInfo p, AutoBuildAttribute attributeData, System.Type modelType) {
      try {
        ValueSetter setter = attributeData.Setter ??= new((m,v) => p.Set(m, v));
        if (setter is null) {
          System.Type baseType = p.DeclaringType;
          while (setter is null && baseType is not null) {
            PropertyInfo propertyInfo = baseType.GetProperty(
              p.Name,
              BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
            );

            if (propertyInfo is null) {
              break;
            }

            setter = new((m, v) => propertyInfo.Set(m, v));
            baseType = baseType.BaseType;
          }

          if (setter is null) throw new AccessViolationException($"Can not find a setter for property: {p.Name}, on model: {p.DeclaringType.FullName}, for auto-builder property attribute setup");
        }

        if (!attributeData._checkedRequired) {
          attributeData.IsRequiredAsAParameter = attributeData.IsRequiredAsAParameter || p.GetCustomAttributes().Where(a => a.GetType().Name == "RequiredAttribute").Any();
          attributeData._checkedRequired = true;
        }

        ValueGetter getter = attributeData.Getter
          ??= attributeData.FromDefaultOnly
            ? (IModel model, IBuilder builder, PropertyInfo property, AutoBuildAttribute attributeData, bool isRequired) 
              => GetDefaultValue(m, _validateInternalNotPassedIn(b, attributeData.ParameterName ?? p.Name, m), p, attributeData, isRequired)
            : attributeData.IsRequiredAsAParameter
              ? BuildDefaultGetterForRequiredValueFromBuilder(p)
              : BuildDefaultGetterFromBuilderOrDefault(p);

        object value = getter(
          m,
          b,
          p,
          attributeData,
          attributeData.IsRequiredAsAParameter
        );
        
        // get the validator if we need to
        if (attributeData.ValueValidatorName is not null) {
          attributeData.Validator ??= _generateValidator(attributeData.ValueValidatorName, m, p);
        }

        if (attributeData.Validator is not null) {
          try {
            if (!attributeData.Validator.Invoke(value, b, m, out string message, out System.Exception exception)) {
              throw new ArgumentException($"Invalid Value: {value}, tried to set to parameter/property: {attributeData.ParameterName ?? p.Name}, via auto builder: {message}", exception);
            }
          }
          catch (Exception e) {
            throw new ArgumentException($"Value caused Exception during Validation: {value?.ToString() ?? "null"}, tried to set to parameter/property: {attributeData.ParameterName ?? p.Name}, via auto builder", e);
          }
        }

        attributeData._notNull 
          ??= p.GetCustomAttributes()
            .Where(a => a.GetType().Name == "NotNullAttribute")
            .Any();

        if (attributeData.NotNull && value is null) {
          throw new ArgumentNullException(attributeData.ParameterName ?? p.Name);
        }

        setter.Invoke(m, value);
        return m;
      } catch (System.Exception e) {
        throw new Exception(modelType, $"Field: {p.Name} on Model Type: {modelType}, could not be auto-built due to an inner exception.", e);
      }
    }

    static IBuilder _validateInternalNotPassedIn(IBuilder builder, string parameterName, IModel model) 
      => builder.Has(parameterName)
        ? throw new AccessViolationException($"Cannot pass a value in for Internal Autobuild Parameter: {parameterName}, on model: {model.GetType().FullName}.")
        : builder;

    static ValueValidator _generateValidator(string valueValidatorName, IModel model, PropertyInfo property) {
      var @delegate = property.DeclaringType.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .FirstOrDefault(m => m.Name == valueValidatorName
          && (
            (m is PropertyInfo p)
              && p.PropertyType == typeof(AutoBuildAttribute.ValueValidator)
            || (m is FieldInfo f)
              && f.FieldType == typeof(AutoBuildAttribute.ValueValidator)
            || (m is MethodInfo l)
              && (l.ReturnType == typeof(bool))
              && (l.GetParameters().Select(p => p.ParameterType).SequenceEqual(new System.Type[] {
                typeof(object),
                typeof(IBuilder),
                typeof(IModel),
                typeof(string),
                typeof(Exception)
              }) || l.GetParameters().Select(p => p.ParameterType).SequenceEqual(new System.Type[] {
                property.PropertyType,
                typeof(IBuilder),
                typeof(IModel),
                typeof(string),
                typeof(Exception)
              }))
            ));

      if(@delegate is MethodInfo method) {
        return new ValueValidator((object v, IBuilder b, IModel m, out string t, out System.Exception x) => {
          object[] parameters = new object[] { v, b, b, null, null };
          if((bool)method.Invoke(b, parameters)) {
            t = parameters[3] as string;
            x = parameters[4] as Exception;

            return true;
          }
          else {
            t = parameters[3] as string;
            x = parameters[4] as Exception;

            return false;
          }
        });
      }
      else {
        ValueValidator defaultValidatorDelegate = (@delegate is PropertyInfo p
            ? p.Get(model)
            : @delegate is FieldInfo f
              ? f.GetValue(model)
              : throw new System.Exception($"Unknown member type for default builder attribute default value")
          ) as AutoBuildAttribute.ValueValidator;

        return defaultValidatorDelegate;
      }
    }

    /// <summary>
    /// The default getter for getting a default value for a field.
    /// </summary>
    public static ValueGetter GetDefaultValue = (IModel model, IBuilder builder, PropertyInfo property, AutoBuildAttribute attributeData, bool isRequired) => {
      /// get via default attribute box value.
      // TODO: instead of an overall override, this should be able to be deferred then used if the cached functions provide null in the end.
      var defaultValueAttributeData = property.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault() as DefaultValueAttribute;
      if (defaultValueAttributeData is not null) {
        return defaultValueAttributeData.Value;
      }

      /// get via getter on the model
      if (attributeData.DefaultValueGetterDelegateName is not null) {
        var @delegate = property.DeclaringType.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
          .FirstOrDefault(m => m.Name == attributeData.DefaultValueGetterDelegateName
            && (
              (m is PropertyInfo p)
                && p.PropertyType == typeof(AutoBuildAttribute.DefaultValueGetter)
              || (m is FieldInfo f)
                && f.FieldType == typeof(AutoBuildAttribute.DefaultValueGetter)
              || (m is MethodInfo l)
                && (l.ReturnType == typeof(object) || l.ReturnType == property.PropertyType)
                && l.GetParameters().Select(p => p.GetType()).SequenceEqual(new System.Type[] {
                  typeof(IBuilder),
                  typeof(IModel)
                })
            ));

        if (@delegate is null) {
          throw new MissingMemberException($"Could not find a property or field named {attributeData.DefaultValueGetterDelegateName}, on model type {property.DeclaringType}, that is a delegate of type {typeof(AutoBuildAttribute.DefaultValueGetter).FullName}, or a function with the same name and the return type {typeof(IModel).FullName}, and parameters: ({nameof(IBuilder)}, {nameof(IModel)})");
        }

        if (@delegate is MethodInfo method) {
          // TODO: cache this
          return method.Invoke(model, new object[] { model, builder });
        }
        else {
          DefaultValueGetter defaultGetterDelegate = (@delegate is PropertyInfo p
              ? p.GetValue(model)
              : @delegate is FieldInfo f
                ? f.GetValue(model)
                : throw new System.Exception($"Unknown member type for default builder attribute default value")
            ) as AutoBuildAttribute.DefaultValueGetter;

          // TODO: cache this
          return defaultGetterDelegate.Invoke(builder, model);
        }
      }

      /// get via name on the archetype
      System.Type linkedArchetypeComponentType = null;
      Func<IBuilder, object> valueSource = b => b.Archetype;
      if (attributeData.DefaultArchetypePropertyName is not null) {
        MemberInfo archetypeDefaultProvider = null;

        /// components check their linked component first
        if (model is IModel.IComponent component 
          && builder.Archetype.Id.Universe.Components._archetypeComponentsLinkedToModelComponents
            .Reverse
            .TryGetValue(
              Components.GetComponentBaseType(component.GetType()),
              out linkedArchetypeComponentType
            )
        ) {
          archetypeDefaultProvider = linkedArchetypeComponentType.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == attributeData.DefaultArchetypePropertyName
              && (
                (m is PropertyInfo p) && p.PropertyType == property.PropertyType
                || (m is FieldInfo f) && f.FieldType == property.PropertyType
                || (m is MethodInfo l)
                  && (l.ReturnType == typeof(object) || l.ReturnType == property.PropertyType)
                  && l.GetParameters().Select(p => p.GetType()).SequenceEqual(new System.Type[] {
                  typeof(IBuilder),
                  typeof(IModel)
                  })
              ));

          if (archetypeDefaultProvider is not null) {
            valueSource = b => (b as IComponent.IBuilder).Parent?.Factory is Archetype a 
              && a.TryToGetComponent(Components.GetKey(linkedArchetypeComponentType), out var component) 
                ? component 
                : null;
          }
        }

        archetypeDefaultProvider ??= builder.Archetype.GetType().GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
          .FirstOrDefault(m => m.Name == attributeData.DefaultArchetypePropertyName
            && (
              (m is PropertyInfo p) && p.PropertyType == property.PropertyType
              || (m is FieldInfo f) && f.FieldType == property.PropertyType
              || (m is MethodInfo l)
                && (l.ReturnType == typeof(object) || l.ReturnType == property.PropertyType)
                && l.GetParameters().Select(p => p.GetType()).SequenceEqual(new System.Type[] {
                  typeof(IBuilder),
                  typeof(IModel)
                })
            ));

        if (archetypeDefaultProvider is null) {
          throw new MissingMemberException($"Could not find a property or field named {attributeData.DefaultArchetypePropertyName}, on model type {property.DeclaringType}, that is of type {property.PropertyType.FullName}, or a function with the same name and the return type, with and parameters: ({nameof(IBuilder)}, {nameof(IModel)})");
        }

        if (archetypeDefaultProvider is MethodInfo method) {
          // TODO: cache this
          return method.Invoke(valueSource(builder), new object[] { model, builder });
        }
        else {
          DefaultValueGetter defaultGetterDelegate = (archetypeDefaultProvider is PropertyInfo p
            ? new DefaultValueGetter((b, m) => p.GetValue(valueSource(b)))
            : archetypeDefaultProvider is FieldInfo f
              ? new((b, m) => f.GetValue(valueSource(b)))
              : throw new System.Exception($"Unknown member type for default builder attribute default value")
          );

          // TODO: cache this
          return defaultGetterDelegate.Invoke(builder, model);
        }
      }

      /// try to find the standard default property:
      // if this ia component with a linked component we check the linked type first.
      PropertyInfo archetypeDefaultProperty = null;
      if (linkedArchetypeComponentType is not null) {
        archetypeDefaultProperty = linkedArchetypeComponentType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
          .FirstOrDefault(p =>
            ((p.Name == "Default" + (attributeData.ParameterName ?? property.Name))
              || (p.Name.Trim('_').ToLower() == ("Default" + (attributeData.ParameterName ?? property.Name)).ToLower()))
            && p.PropertyType == property.PropertyType
          );
      }

      archetypeDefaultProperty ??= builder.Archetype.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .FirstOrDefault(p =>
          ((p.Name == "Default" + (attributeData.ParameterName ?? property.Name))
            || (p.Name.Trim('_').ToLower() == ("Default" + (attributeData.ParameterName ?? property.Name)).ToLower()))
          && p.PropertyType == property.PropertyType
        );

      if (archetypeDefaultProperty is not null) {
        // TODO: cache this
        return archetypeDefaultProperty.GetValue(valueSource(builder));
      }

      return property.GetValue(model);
    };

    /// <summary>
    /// Used to build the default ValueGetter for non required items.
    /// </summary>
    public static ValueGetter BuildDefaultGetterFromBuilderOrDefault(PropertyInfo property, ValueGetter onFailureDefaultOveride = null, bool allowTypeMismachesForDefaultFallthough = false) 
      => new((m, b, p, a, _) => {
        if (b.TryToGet(a.ParameterName ?? p.Name, out var providedValue)) {
          if (providedValue is null) {
            return null;
          } if (property.PropertyType.IsAssignableFrom(providedValue.GetType())) {
            return providedValue;
          }
          else if (providedValue.TryToCastTo(property.PropertyType, out var result)) {
            return result;
          }
          else if (!allowTypeMismachesForDefaultFallthough) {
            throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param: {a.ParameterName ?? p.Name}, as desired Type: {p.PropertyType.ToFullHumanReadableNameString()}. The provided invalid value has Type {providedValue?.GetType().ToFullHumanReadableNameString() ?? "null"} instead and cannot be cast to the required type.");
          }
        }

        return (onFailureDefaultOveride ?? GetDefaultValue).Invoke(m, b, p, a, a.IsRequiredAsAParameter);
      });

    /// <summary>
    /// Used to build the default ValueGetter for required items.
    /// </summary>
    public static ValueGetter BuildDefaultGetterForRequiredValueFromBuilder(PropertyInfo property) 
      => (_, b, p, a, _) => { 
        if (!b.TryGetValue(a.ParameterName ?? p.Name, out var providedValue)) {
          throw new IModel.IBuilder.Param.MissingException($"Tried to construct a model without the required param: {a.ParameterName ?? p.Name} of type {p.PropertyType.ToFullHumanReadableNameString()} being provided. If this is a test, try adding a default value for the empty model for this required param to the Archetype's DefaultTestParams field.");
        }

        if (providedValue is null) {
          return null;
        }

        if (property.PropertyType.IsAssignableFrom(providedValue.GetType())) {
          return providedValue;
        }

        try {
          return providedValue.CastTo(p.PropertyType);
        }
        catch (Exception e) {
          throw new IModel.IBuilder.Param.MissmatchException($"Tried to get param: {a.ParameterName ?? p.Name}, as desired Type: {p.PropertyType.ToFullHumanReadableNameString()}. The provided invalid value has Type {providedValue?.GetType().ToFullHumanReadableNameString() ?? "null"} instead and cannot be cast to the required type.", e);
        }
      };
  }
}
