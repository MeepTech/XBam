using Meep.Tech.Collections.Generic;
using Meep.Tech.XBam.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Meep.Tech.XBam {
  public partial interface IReadableComponentStorage {

    /// <summary>
    /// Simple container for a collection of components.
    /// </summary>
    public class ReadOnlyModelComponentCollection : IReadOnlyDictionary<string, IModel.IComponent> {

      /// <summary>
      /// The model all the components belong to.
      /// </summary>
      public IModel Storage {
        get;
      }

      /// <summary>
      /// Make a ReadOnlyModelComponentCollection for a parent storage model.
      /// </summary>
      public ReadOnlyModelComponentCollection(IModel parentStorage, IReadOnlyDictionary<string, IModel.IComponent> initialItems = null) {
        Storage = parentStorage;
        _entries = initialItems.ToDictionary(
          component => component.Key,
          component => component.Value
        );
      }

      /// <summary>
      /// Make a ReadOnlyModelComponentCollection for a parent storage model.
      /// </summary>
      public ReadOnlyModelComponentCollection(IModel parentStorage, Dictionary<string, IModel.IComponent> initialItems = null) {
        Storage = parentStorage;
        _entries = initialItems;
      }

      #region IReadOnlyDictionary Implementation
      ///<summary><inheritdoc/></summary>
      public Dictionary<string, IModel.IComponent> _entries
        = new();

      ///<summary><inheritdoc/></summary>
      public IModel.IComponent this[string key] => ((IReadOnlyDictionary<string, IModel.IComponent>)_entries)[key];

      ///<summary><inheritdoc/></summary>
      public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, IModel.IComponent>)_entries).Keys;

      ///<summary><inheritdoc/></summary>
      public IEnumerable<IModel.IComponent> Values => ((IReadOnlyDictionary<string, IModel.IComponent>)_entries).Values;

      ///<summary><inheritdoc/></summary>
      public int Count => ((IReadOnlyCollection<KeyValuePair<string, IModel.IComponent>>)_entries).Count;

      ///<summary><inheritdoc/></summary>
      public bool ContainsKey(string key) {
        return ((IReadOnlyDictionary<string, IModel.IComponent>)_entries).ContainsKey(key);
      }

      ///<summary><inheritdoc/></summary>
      public IEnumerator<KeyValuePair<string, IModel.IComponent>> GetEnumerator() {
        return ((IEnumerable<KeyValuePair<string, IModel.IComponent>>)_entries).GetEnumerator();
      }

      ///<summary><inheritdoc/></summary>
      public bool TryGetValue(string key, out IModel.IComponent value) {
        return ((IReadOnlyDictionary<string, IModel.IComponent>)_entries).TryGetValue(key, out value);
      }

      ///<summary><inheritdoc/></summary>
      IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)_entries).GetEnumerator();
      }
      #endregion
    }

    /// <summary>
    /// Used to convert a collection of components to and from a json object by key
    /// </summary>
    public class ComponentsToJsonConverter : JsonConverter<ReadOnlyModelComponentCollection> {

      /// <summary>
      /// The universe this is for
      /// </summary>
      public Universe Universe {
        get;
      }

      ///<summary><inheritdoc/></summary>
      public ComponentsToJsonConverter(Universe universe) {
        Universe = universe;
      }

      public override void WriteJson(JsonWriter writer, [AllowNull] ReadOnlyModelComponentCollection value, JsonSerializer serializer) {
        writer.WriteStartObject();
        value.ForEach(e => {
          writer.WritePropertyName(e.Key);
          serializer.Serialize(writer, e.Value.ToJson(serializer));
        });
        writer.WriteEndObject();
      }

      public override ReadOnlyModelComponentCollection ReadJson(JsonReader reader, Type objectType, [AllowNull] ReadOnlyModelComponentCollection existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType != JsonToken.StartObject) {
          throw new ArgumentException($"Components Field for ECSBAM Models requires an array Jtoken to deserialize");
        }

        JObject componentsJson = serializer.Deserialize<JObject>(reader);

        Dictionary<string, IModel.IComponent> components = new();
        foreach (var (key, componentToken) in componentsJson) {
          components.Add(key, Deserialize.ToComponent((JObject)componentToken, ontoParent: (IReadableComponentStorage?)existingValue.Storage) as IModel.IComponent);
        }
        
        return new ReadOnlyModelComponentCollection(existingValue.Storage, components);
      }
    }
  }
}
