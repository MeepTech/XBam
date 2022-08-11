using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A read only ordered collection of delegates
  /// </summary>
  public interface IReadOnlyDelegateCollection<TAction>
    : IReadOnlyOrderedDictionary<string, TAction>
    where TAction : Delegate 
  {

    /// <summary>
    /// Change all the delegates and return a new collection
    /// </summary>
    DelegateCollection<TNewActionType> ReDelegate<TNewActionType>(Func<TAction, TNewActionType> converter)
      where TNewActionType : Delegate
        => new(this.Select(entry => new KeyValuePair<string, TNewActionType>(
          entry.Key,
          converter(entry.Value)
        )));
  }
}