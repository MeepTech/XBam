using Meep.Tech.XBam.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Meep.Tech.XBam.Json {

  namespace Configuration {
    /// <summary>
    /// Used for Archetypes with custom json serialization logic
    /// </summary>
    public interface IHaveCustomModelJsonSerializationLogic : IArchetype, ITrait<IHaveCustomModelJsonSerializationLogic> {
      string ITrait<IHaveCustomModelJsonSerializationLogic>.TraitName
        => "Custom Model Json Serialization Logic";
      string ITrait<IHaveCustomModelJsonSerializationLogic>.TraitDescription
        => "Used for Archetypes with custom json serialization logic";

      /// <summary>
      /// Used to deserialize a model with this archetype from a json string
      /// </summary>
      protected internal IModel? DeserializeModelFromJson(Archetype archetype, JObject json, Type? deserializeToTypeOverride = null, JsonSerializer? serializerOverride = null)
        => Universe.GetExtraContext<ModelJsonSerializerContext>().DeserializeModelFromJson((Archetype)this, json, deserializeToTypeOverride, serializerOverride);

      /// <summary>
      /// Used to deserialize a component with this archetype from a json string
      /// </summary>
      protected internal IModel? DeserializeComponentFromJson(Archetype archetype, JObject json, IReadableComponentStorage? ontoParent = null, Type? deserializeToTypeOverride = null, JsonSerializer? serializerOverride = null)
        => Universe.GetExtraContext<ModelJsonSerializerContext>().DeserializeComponentFromJson((Archetype)this, json, ontoParent, deserializeToTypeOverride, serializerOverride);

      /// <summary>
      /// Used to serialize a model with this archetype to a jobject by default
      /// </summary>
      protected internal JObject SerializeModelToJson(IModel model, JsonSerializer? serializerOverride = null)
        => Universe.GetExtraContext<ModelJsonSerializerContext>().SerializeModelToJson((Archetype)this, model, serializerOverride);
    }
  }
}