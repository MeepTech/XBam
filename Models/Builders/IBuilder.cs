using System.Collections.Generic;

namespace Meep.Tech.XBam {

  /// <summary>
  /// The base interface for builders
  /// </summary>
  public partial interface IBuilder : IEnumerable<KeyValuePair<string, object>>, IReadOnlyDictionary<string, object> {

    /// <summary>
    /// The universe this is being built in
    /// </summary>
    Universe Universe {
      get;
    }
    
    /// <summary>
    /// The archetype that initialize the building and made the builder
    /// </summary>
    Archetype Archetype {
      get;
    }

    /// <summary>
    /// The parameters contained in this builder as a list.
    /// </summary>
    IEnumerable<KeyValuePair<string, object>> Parameters {
      get;
    }

    /// <summary>
    /// Return the builder with a new value appended.
    /// </summary>
    IBuilder Add(KeyValuePair<string, object> parameter);

    /// <summary>
    /// Return the builder with a new value appended.
    /// </summary>
    IBuilder Add(string key, object value)
      => Add(new KeyValuePair<string, object>(key, value));

    /// <summary>
    /// Try to get a parameter by the key 
    /// </summary>
    object Get(string key);

    /// <summary>
    /// Try to get a parameter by the key 
    /// </summary>
    bool TryToGet(string key, out object value);

    /// <summary>
    /// See if there's a param with this key
    /// </summary>
    bool Has(string key);

    /// <summary>
    /// Execute a builder
    /// </summary>
    IModel Make();
  }

  /// <summary>
  /// The base interface for builders
  /// </summary>
  public interface IBuilder<TModelBase>
    : IBuilder
    where TModelBase : IModel<TModelBase> 
  {

    /// <summary>
    /// Return the builder with a new value appended.
    /// </summary>
    new IBuilder<TModelBase> Add(KeyValuePair<string, object> parameter);
    IBuilder IBuilder.Add(KeyValuePair<string, object> parameter)
      => Add(parameter);

    /// <summary>
    /// Return the builder with a new value appended.
    /// </summary>
    new IBuilder<TModelBase> Add(string key, object value)
      => Add(new KeyValuePair<string, object>(key, value));
    IBuilder IBuilder.Add(string key, object value)
      => Add(key, value);

    /// <summary>
    /// Execute a builder
    /// </summary>
    new TModelBase Make();

    IModel IBuilder.Make()
      => Make();
  }
}