using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;

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
      readonly IFactory.JsonStringConverter _factoryToStringJsonConverter
        = new();
      readonly IReadableComponentStorage.ComponentsToJsonConverter _componentsJsonConverter;
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
        _serializationContext = universe.GetExtraContext<IModelJsonSerializer>();
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
        var baseProps = base.CreateProperties(type, memberSerialization);
        // remove the universe property if there's one. It gets assinged as part of the archetype or key value
        baseProps.Remove(
          baseProps.FirstOrDefault(prop
            => prop.PropertyName == nameof(Universe).ToLower())
        );

        // Add unique ids if there isn't one already
        if (typeof(IUnique).IsAssignableFrom(type)) {
          JsonProperty idJsonProp;
          if ((idJsonProp = baseProps.FirstOrDefault(prop => prop.PropertyName == "id")) == null) {
            idJsonProp = CreateProperty(_idProp, memberSerialization);
            baseProps.Add(idJsonProp);
          }

          idJsonProp.Order = int.MinValue + 1;
          baseProps = baseProps.OrderBy(p => p.Order ?? -1).ToList();
        }

        // add component factory
        if (typeof(Meep.Tech.XBam.IComponent).IsAssignableFrom(type)) {
          JsonProperty factoryJsonProp;
          if ((factoryJsonProp = baseProps.FirstOrDefault(prop => prop.PropertyName == "key")) == null) {
            factoryJsonProp = CreateProperty(_factoryProp, memberSerialization);
            baseProps.Add(factoryJsonProp);
          }

          factoryJsonProp.Order = int.MinValue;
          factoryJsonProp.PropertyName = "key";
          baseProps = baseProps.OrderBy(p => p.Order ?? -1).ToList();
        }

        return baseProps;
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
        JsonProperty baseProperty = base.CreateProperty(member, memberSerialization);
        if (!(member.GetCustomAttribute(typeof(ArchetypePropertyAttribute)) is null)) {
          // Archetype is always first:
          baseProperty.Order = int.MinValue;
          baseProperty.PropertyName = nameof(Archetype).ToLower();
          baseProperty.Converter = _factoryToStringJsonConverter;
          baseProperty.ObjectCreationHandling = ObjectCreationHandling.Replace;
        }

        if (!(member.GetCustomAttribute(typeof(ModelComponentsProperty)) is null)) {
          baseProperty.Order = int.MaxValue;
          baseProperty.Converter = _componentsJsonConverter;
          baseProperty.PropertyName = nameof(Components).ToLower();
          baseProperty.ObjectCreationHandling = ObjectCreationHandling.Replace;
        }

        /// Any property with a set method is included. Opt out is the default.
        if (_serializationContext.Options.PropertiesMustOptOutForJsonSerialization
          && member is PropertyInfo property
          && typeof(IModel).IsAssignableFrom(property.DeclaringType)
        ) {
          if (property.SetMethod != null) {
            baseProperty.Writable = true;
          }
        }

        Universe.ExtraContexts.OnLoaderModelJsonPropertyCreationComplete(member, baseProperty);
        return baseProperty;
      }
    }
  }
}