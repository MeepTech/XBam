using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;
using Meep.Tech.Reflection;

namespace Meep.Tech.XBam.Json.Configuration {

  public partial class ModelJsonSerializerContext {

    /// <summary>
    /// The default contract resolver class used for json serialization by ECSBAM
    /// </summary>
    public class DefaultContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver {
      static readonly PropertyInfo _factoryProp
        = typeof(IComponent).GetProperty(nameof(IComponent.Factory));
      static readonly PropertyInfo _idProp
        = typeof(IUnique).GetProperty(nameof(IUnique.Id));
      static readonly PropertyInfo _universeProp
        = typeof(IResource).GetProperty(nameof(IResource.Universe));
      readonly Archetypes.JsonStringConverter _factoryToStringJsonConverter;
      readonly Models.ComponentsToJsonConverter _componentsJsonConverter;
      readonly IModelJsonSerializer _serializationContext;

      /// <summary>
      /// the universe this resolver is for
      /// </summary>
      public Universe Universe {
        get;
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public DefaultContractResolver(Universe universe) {
        IgnoreSerializableAttribute = false;
        NamingStrategy = new CamelCaseNamingStrategy() {
          OverrideSpecifiedNames = false
        };

        Universe = universe;
        _componentsJsonConverter = new(Universe);
        _factoryToStringJsonConverter = new(Universe);
        _serializationContext = universe.GetExtraContext<IModelJsonSerializer>();
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
        var baseProps = base.CreateProperties(type, memberSerialization);
        ConfigureBaseProperties(type, baseProps, _serializationContext, CreateProperty, memberSerialization, _factoryToStringJsonConverter);

        return baseProps;
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
        JsonProperty baseProperty = base.CreateProperty(member, memberSerialization);
        ConfigureBaseProperty(member, baseProperty, _serializationContext, _factoryToStringJsonConverter, _componentsJsonConverter);

        return baseProperty;
      }

      /// <summary>
      /// Used to configure the base properties on a contract resolver for xbam.
      /// </summary>
      public static void ConfigureBaseProperties(
        Type type,
        IList<JsonProperty> baseProps,
        IModelJsonSerializer context,
        Func<MemberInfo, MemberSerialization, JsonProperty> resolverPropertyConstructor,
        MemberSerialization memberSerialization,
        Archetypes.JsonStringConverter factoryToStringJsonConverter
      ) {
        // add universe (if applicable)
        if (typeof(IResource).IsAssignableFrom(type)) {
          var universeProp = baseProps
            .FirstOrDefault(prop => prop.PropertyName == nameof(Universe).ToLower() && prop.PropertyType == typeof(Universe));

          if (universeProp is not null) {
            if (!context.Options.IncludeUniverseKey) {
              baseProps.Remove(universeProp);
            }
          }
          else {
            if (context.Options.IncludeUniverseKey) {
              universeProp = resolverPropertyConstructor(_universeProp, memberSerialization);
              baseProps.Add(universeProp);
            }
          }
        }

        // Add unique ids if there isn't one already
        if (typeof(IUnique).IsAssignableFrom(type)) {
          JsonProperty idJsonProp;
          if ((idJsonProp = baseProps.FirstOrDefault(prop => prop.PropertyName == "id")) == null) {
            idJsonProp = resolverPropertyConstructor(_idProp, memberSerialization);
            baseProps.Add(idJsonProp);
          }

          idJsonProp.Order = int.MinValue + 1;
          baseProps = baseProps.OrderBy(p => p.Order ?? -1).ToList();
        }

        // add component factory
        if (typeof(IComponent).IsAssignableFrom(type) || (type.IsAssignableToGeneric(typeof(IModel<>)) && !type.IsAssignableToGeneric(typeof(IModel<,>)))) {
          JsonProperty factoryJsonProp;
          if ((factoryJsonProp = baseProps.FirstOrDefault(prop => prop.PropertyName == "key")) == null) {
            factoryJsonProp = resolverPropertyConstructor(_factoryProp, memberSerialization);
            baseProps.Add(factoryJsonProp);
          }

          factoryJsonProp.Order = int.MinValue;
          factoryJsonProp.PropertyName = Models.ComponentsToJsonConverter.ComponentKeyPropertyName;
          factoryJsonProp.Converter = factoryToStringJsonConverter;
          baseProps = baseProps.OrderBy(p => p.Order ?? -1).ToList();
        }
      }

      /// <summary>
      /// Used to configure the base properties on a contract resolver for xbam.
      /// </summary>
      public static void ConfigureBaseProperty(
        MemberInfo member,
        JsonProperty baseProperty,
        IModelJsonSerializer context,
        Archetypes.JsonStringConverter factoryToStringJsonConverter,
        Models.ComponentsToJsonConverter componentsJsonConverter,
        Action<
          MemberInfo,
          JsonProperty,
          IModelJsonSerializer,
          Archetypes.JsonStringConverter,
          Models.ComponentsToJsonConverter
        >? doBeforeMarkingComplete = null
      ) {
        if (member.GetCustomAttribute(typeof(ArchetypePropertyAttribute)) is not null) {
          // Archetype is always first:
          baseProperty.Order = int.MinValue;
          baseProperty.PropertyName = nameof(Archetype).ToLower();
          baseProperty.Converter = factoryToStringJsonConverter;
          baseProperty.ObjectCreationHandling = ObjectCreationHandling.Replace;
        }

        if (member.GetCustomAttribute(typeof(ModelComponentsProperty)) is not null) {
          baseProperty.Order = int.MaxValue;
          baseProperty.Converter = componentsJsonConverter;
          baseProperty.PropertyName = nameof(Components).ToLower();
          baseProperty.ObjectCreationHandling = ObjectCreationHandling.Replace;
        }

        /// Any property with a set method is included. Opt out is the default.
        if (context.Options.PropertiesMustOptOutForJsonSerialization
          && member is PropertyInfo property
          && typeof(IModel).IsAssignableFrom(property.DeclaringType)
        ) {
          if (property.SetMethod != null) {
            baseProperty.Writable = true;
          }
        }

        doBeforeMarkingComplete?.Invoke(member, baseProperty, context, factoryToStringJsonConverter, componentsJsonConverter);

        context.Options.OnLoaderModelJsonPropertyCreationComplete?.Invoke(member, baseProperty);
      }
    }
  }
}