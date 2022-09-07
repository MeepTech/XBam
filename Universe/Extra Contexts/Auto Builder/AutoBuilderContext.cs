using static Meep.Tech.XBam.AutoBuildAttribute;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using Meep.Tech.Reflection;
using Meep.Tech.Collections.Generic;
using Meep.Tech.Data.Archetypes.Traits;

namespace Meep.Tech.XBam.Configuration {

  /// <summary>
  /// Configures the Auto Builder on Models.
  /// </summary>
  public partial class AutoBuilderContext : Universe.ExtraContext, IModelAutoBuilder {
    Dictionary<Archetype, (Func<IBuilder, IModel> ctor, DelegateCollection<Func<IModel, IBuilder, IModel>>? steps)> _modelAutoBuilderSteps
      = new();
    IModelAutoBuilder.ISettings IModelAutoBuilder.Options => Options;

    ///<summary><inheritdoc/></summary>
    public new Settings Options
      => base.Options as Settings
        ?? throw new ArgumentNullException(nameof(Options));

    ///<summary><inheritdoc/></summary>
    public AutoBuilderContext(Settings? settings = null) 
      : base(settings ?? new()) {}

    ///<summary><inheritdoc/></summary>
    public bool HasAutoBuilderSteps<TArchetype>(TArchetype? archetype = null) where TArchetype : Archetype
      => _modelAutoBuilderSteps.TryToGet(archetype ?? typeof(TArchetype).AsArchetype()).steps != null;

    #region Generate Auto Builder Steps

    void _initializeAutoBuilderSettings(Archetype archetype, bool setToEmpty = false) {
      if (setToEmpty) {
        _modelAutoBuilderSteps[archetype] = (
          _modelAutoBuilderSteps.TryToGet(archetype).ctor ?? ((IFactory)archetype)._modelConstructor,
          null
        )!;
        archetype.ModelTypeProduced = null!;
        Universe?.Archetypes._rootArchetypeTypesByBaseModelType
          .Where(e => e.Value == GetType())
          .Select(e => e.Key)
          .ToList()
          .ForEach(k => Universe.Archetypes._rootArchetypeTypesByBaseModelType.Remove(k));
      }
      else {
        IModel model
          = Universe.Loader.GetOrBuildTestModel(
              archetype,
              archetype.ModelTypeProduced
          );

        // register it
        Type constructedModelType = model.GetType();
        archetype.ModelTypeProduced = constructedModelType;
        Universe.Archetypes._rootArchetypeTypesByBaseModelType[constructedModelType.FullName]
          = archetype.Type;

        /// add auto builder properties based on the model type:
        _modelAutoBuilderSteps[archetype] = (
          archetype.ModelConstructor,
          _generateAutoBuilderSteps(constructedModelType)
            .ToDictionary(
             e => e.name,
             e => e.function
            )
        )!;

        archetype.ModelConstructor = builder => {
          var model = _modelAutoBuilderSteps[archetype].ctor(builder);
          IConfigureModelForAutoBuild? configure;
          if ((configure = archetype as IConfigureModelForAutoBuild) is not null) {
            configure.OnAutoBuildInitialized(ref model, builder!);
          }

          _modelAutoBuilderSteps[archetype].steps?
            .ForEach(a => model = a.Value(model, builder!));

          if (configure is not null) {
            configure.OnAutoBuildStepsCompleted(ref model, builder!);
          }

          return model;
        };
      }
    }

    internal IEnumerable<(string name, Func<IModel, IBuilder, IModel> function)> _generateAutoBuilderSteps(System.Type modelType)
      => modelType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .Select(p => (p, a: p.GetCustomAttribute<AutoBuildAttribute>(true)))
        .Where(e => e.a is not null)
        .Select(e => {
          Options.OnLoaderAutoBuildPropertyCreationStart?.Invoke(modelType, e.a, e.p);

          var entry = (
            order: e.a.Order,
            name: e.a.ParameterName ?? e.p.Name,
            func: new Func<IModel, IBuilder, IModel>((m, b)
              => _autoBuildModelProperty(m, b, e.p, e.a, modelType))
          );

          Options.OnLoaderAutoBuildPropertyCreationComplete?.Invoke(modelType, e.a, e.p, entry);
          return entry;
        }).OrderBy(e => e.order)
        .Select(e => (e.name, e.func));

    /// <summary>
    /// TODO: this needs to be heavily optomized.
    /// </summary>
    static IModel _autoBuildModelProperty(IModel m, IBuilder b, PropertyInfo p, AutoBuildAttribute attributeData, System.Type modelType) {
      try {
        ValueSetter setter = attributeData.Setter ??= new((m, v) => p.Set(m, v));
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
      }
      catch (System.Exception e) {
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

      if (@delegate is MethodInfo method) {
        return new ValueValidator((object v, IBuilder b, IModel m, out string t, out System.Exception x) => {
          object[] parameters = new object[] { v, b, b, null!, null! };
          if ((bool)method.Invoke(b, parameters)) {
            t = (string)parameters[3];
            x = (Exception)parameters[4];

            return true;
          }
          else {
            t = (string)parameters[3];
            x = (Exception)parameters[4];

            return false;
          }
        });
      }
      else {
        ValueValidator defaultValidatorDelegate = (ValueValidator)(
          @delegate is PropertyInfo p
            ? p.Get(model)
            : @delegate is FieldInfo f
              ? f.GetValue(model)
              : throw new System.Exception($"Unknown member type for default builder attribute default value")
        );

        return defaultValidatorDelegate;
      }
    }

    #endregion

    #region Extra Context Events

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Loader> OnLoaderInitializationStart
      => loader => loader.Options.TestModelBuilderTypeMismatchExceptionHandlers.Add(
        typeof(AutoBuilderContext.Exception),
        (Type modelBase, System.Exception ex, out Type? foundType) => {
          var e = (AutoBuilderContext.Exception)ex;
          if (e.ModelTypeBeingBuilt != modelBase) {
            foundType = e.ModelTypeBeingBuilt;
            return true;
          }

          foundType = null;
          return false;
        });

    ///<summary><inheritdoc/></summary>
    protected internal override Action<Archetype> OnLoaderArchetypeModelConstructorSetComplete
      => archetype
        => _initializeAutoBuilderSettings(archetype, ((IFactory)archetype)._modelConstructor is null);

    #endregion
  }
}
