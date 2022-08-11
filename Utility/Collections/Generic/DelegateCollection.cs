using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// An ordered collection of delegates
  /// </summary>
  public class DelegateCollection<TAction>
    : OrderedDictionary<string, TAction>,
      IReadOnlyDelegateCollection<TAction>
    where TAction : Delegate 
  {

    /// <summary>
    /// Make a new delegate collection.
    /// </summary>
    public DelegateCollection(IEnumerable<KeyValuePair<string, TAction>> orderedValues = null)
      : base(orderedValues) { }

    /// <summary>
    /// Make a delegate collection from one action.
    /// </summary>
    public static implicit operator DelegateCollection<TAction>(TAction action)
      => new(new KeyValuePair<string, TAction>(0.ToString(), action).AsSingleItemEnumerable());

    /// <summary>
    /// Make a delegate collection from a list of actions.
    /// </summary>
    public static implicit operator DelegateCollection<TAction>(List<TAction> actions)
      => new(actions.Select((action, index) => new KeyValuePair<string, TAction>(index.ToString(), action)));

    /// <summary>
    /// Make a delegate collection from a dictionary of actions.
    /// </summary>
    public static implicit operator DelegateCollection<TAction>(Dictionary<string, TAction> actions)
      => new(actions);

    /// <summary>
    /// Make a delegate collection from an array of actions.
    /// </summary>
    public static implicit operator DelegateCollection<TAction>(TAction[] actions)
      => new(actions.Select((action, index) => new KeyValuePair<string, TAction>(index.ToString(), action)));

    /// <summary>
    /// wrap all the delegates and return a new collection
    /// </summary>
    public DelegateCollection<TNewActionType> ReDelegate<TNewActionType>(Func<TAction, TNewActionType> converter)
      where TNewActionType : Delegate
        => new(this.Select(entry => new KeyValuePair<string, TNewActionType>(
          entry.Key,
          converter(entry.Value)
        )));
  }
}