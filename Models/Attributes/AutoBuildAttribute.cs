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
      set;
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
    /// The default getter for getting a default value for a field.
    /// </summary>
    public static ValueGetter GetDefaultValue 
      = (IModel model, IBuilder builder, PropertyInfo property, AutoBuildAttribute attributeData, bool isRequired) 
        => {
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
              DefaultValueGetter defaultGetterDelegate = (DefaultValueGetter)(@delegate is PropertyInfo p
                  ? p.GetValue(model)
                  : @delegate is FieldInfo f
                    ? f.GetValue(model)
                    : throw new System.Exception($"Unknown member type for default builder attribute default value")
                );

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
              && builder.Archetype.Universe.Components._archetypeComponentsLinkedToModelComponents
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
