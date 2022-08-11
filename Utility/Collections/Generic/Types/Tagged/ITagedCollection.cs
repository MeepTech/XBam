using System.Collections.Generic;
namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A read/Write tagged colleciton.
  /// </summary>
  public interface ITagedCollection<TTag, TValue> : IReadOnlyTagedCollection<TTag, TValue> {

    /// <summary>
    /// Add a new value with as many tags as you want
    /// </summary>
    void Add(IEnumerable<TTag> tags, TValue value);

    /// <summary>
    /// Add a new value with as many tags as you want
    /// </summary>
    void Add(TValue value, params TTag[] tags);

    /// <summary>
    /// Remove a value
    /// </summary>
    bool Remove(TValue value);

    /// <summary>
    /// Remove all of the tag connections to the given value.
    /// </summary>
    bool RemoveTagsForItem(TValue value, params TTag[] tags);

    /// <summary>
    /// Remove all of the tag connections to the given value.
    /// </summary>
    bool RemoveTagsForItem(IEnumerable<TTag> tags, TValue value);

    /// <summary>
    /// Remove all values for the given tag
    /// </summary>
    bool RemoveValuesFor(TTag tag);
  }
}