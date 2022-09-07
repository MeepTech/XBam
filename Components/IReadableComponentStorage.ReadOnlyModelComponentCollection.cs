using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {
  public partial interface IReadableComponentStorage {

    /// <summary>
    /// Simple container for a collection of components.
    /// TODO: put this in it's own file.
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
  }
}