using System.Collections.Generic;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A tagged collection that can only be read, not modified.
  /// </summary>
  public interface IReadOnlyTagedCollection<TTag, TValue> : IEnumerable<KeyValuePair<IEnumerable<TTag>, TValue>> {

    /// <summary>
    /// Fetch a set of values by tag.
    /// </summary>
    IEnumerable<TValue> this[TTag tag] { get; }

    /// <summary>
    /// Fetch a set of values by tag.
    /// </summary>
    bool TryToGetAnyValuesFor(TTag tag, out IEnumerable<TValue> values);

    /// <summary>
    /// Fetch all the tags for a given value
    /// </summary>
    IEnumerable<TTag> this[TValue value] { get; }

    /// <summary>
    /// Fetch a set of values by tag.
    /// </summary>
    bool TryToGetAnyTagsFor(TValue value, out IEnumerable<TTag> tags);

    /// <summary>
    /// All distinct tags
    /// </summary>
    IEnumerable<TTag> Tags { get; }

    /// <summary>
    /// All distinct values
    /// </summary>
    IEnumerable<TValue> Values { get; }

    /// <summary>
    /// Find the values that match the most tags in order
    /// </summary>
    IEnumerable<TValue> FindBestMatches(IEnumerable<TTag> orderedTags);

    /// <summary>
    /// Find the values that match the most tags in order
    /// </summary>
    IEnumerable<TValue> FindBestMatches(params TTag[] tags);

    /// <summary>
    /// Find the values that match ALL of the given tags in any order
    /// </summary>
    /// <param name="allowValuesToHaveOtherNonMatchingTags">If false, values cannot have any extra tags besides ones in the provided list. If this is true, they can have un-provided tags as well, but must also have all of the provided tags</param>
    IEnumerable<TValue> FindExactMatches(IEnumerable<TTag> tags, bool allowValuesToHaveOtherNonMatchingTags = true);

    /// <summary>
    /// Find matches given tags with specified weights
    /// The higher the weight, the more wanted the tag
    /// </summary>
    IEnumerable<TValue> FindWeightedMatches(IEnumerable<(TTag tag, int weight)> @params);

    /// <summary>
    /// Find the best matches, taking into account tag order
    /// </summary>
    IEnumerable<TValue> FindWeightedMatches(IEnumerable<TTag> orderedTags, int weightMultiplier = 2);

    /// <summary>
    /// Find the best matches, taking into account tag order
    /// </summary>
    IEnumerable<TValue> FindWeightedMatches(int weightMultiplier, params TTag[] orderedTags);

    /// <summary>
    /// Find matches given tags with specified weights
    /// The higher the weight, the more wanted the tag
    /// </summary>
    IEnumerable<TValue> FindWeightedMatches(params (TTag tag, int weight)[] @params);

    /// <summary>
    /// Find the best matches, taking into account tag order
    /// </summary>
    IEnumerable<TValue> FindWeightedMatches(params TTag[] orderedTags);

    /// <summary>
    /// Find the first value with the tags, or a default one with the best match
    /// </summary>
    TValue FirstWithTagsOrDefault(IEnumerable<TTag> tags);

    /// <summary>
    /// Find the first value with the tags, or a default one with the best match
    /// </summary>
    TValue FirstWithTagsOrDefault(params TTag[] tags);

    /// <summary>
    /// Find the values that match the most tags
    /// </summary>
    IEnumerable<TValue> GetAllSortedByBestMatch(IEnumerable<TTag> orderedTags);

    /// <summary>
    /// Find the values that match the most tags
    /// </summary>
    IEnumerable<TValue> GetAllSortedByBestMatch(params TTag[] tags);

    /// <summary>
    /// Find matches given tags with specified weights
    /// The higher the weight, the more wanted the tag
    /// </summary>
    IEnumerable<TValue> GetAllSortedByWeight(IEnumerable<(TTag tag, int weight)> @params);

    /// <summary>
    /// Find the best matches, taking into account tag order
    /// </summary>
    IEnumerable<TValue> GetAllSortedByWeight(IEnumerable<TTag> orderedTags);

    /// <summary>
    /// Find the best matches, taking into account tag order
    /// </summary>
    IEnumerable<TValue> GetAllSortedByWeight(IList<TTag> orderedTags, int weightMultiplier = 2);

    /// <summary>
    /// Find the best matches, taking into account tag order
    /// </summary>
    IEnumerable<TValue> GetAllSortedByWeight(int weightMultiplier, params TTag[] orderedTags);

    /// <summary>
    /// Find matches given tags with specified weights
    /// The higher the weight, the more wanted the tag
    /// </summary>
    IEnumerable<TValue> GetAllSortedByWeight(params (TTag tag, int weight)[] @params);

    /// <summary>
    /// Find the best matches, taking into account tag order
    /// </summary>
    IEnumerable<TValue> GetAllSortedByWeight(params TTag[] orderedTags);

    /// <summary>
    /// Select the values that match the most tags in order into a new collection
    /// </summary>
    TagedCollection<TTag, TValue> SelectBestMatches(IEnumerable<TTag> orderedTags);

    /// <summary>
    /// Select the values that match the most tags in order into a new TagedCollection
    /// </summary>
    TagedCollection<TTag, TValue> SelectBestMatches(params TTag[] tags);

    /// <summary>
    /// Select the values that match any of the tags, unordered, into a new TagedCollection
    /// </summary>
    TagedCollection<TTag, TValue> SelectMatches(IEnumerable<TTag> tags);

    /// <summary>
    /// Select the values that match any of the tags, unordered, into a new TagedCollection
    /// </summary>
    TagedCollection<TTag, TValue> SelectMatches(params TTag[] tags);

    /// <summary>
    /// Select the values that match ALL of the tags, unordered, into a new TagedCollection
    /// </summary> 
    /// <param name="allowValuesToHaveOtherNonMatchingTags">If false, values cannot have any extra tags besides ones in the provided list. If this is true, they can have un-provided tags as well, but must also have all of the provided tags</param>
    TagedCollection<TTag, TValue> SelectExactMatches(IEnumerable<TTag> tags, bool allowValuesToHaveOtherNonMatchingTags = true);

    /// <summary>
    /// Select matches given tags with specified weights into a new TagedCollection
    /// The higher the weight, the more wanted the tag
    /// </summary>
    TagedCollection<TTag, TValue> SelectWeightedMatches(IEnumerable<(TTag tag, int weight)> @params);

    /// <summary>
    /// Select the best matches, taking into account tag order into a new TagedCollection
    /// </summary>
    TagedCollection<TTag, TValue> SelectWeightedMatches(IEnumerable<TTag> orderedTags, int weightMultiplier = 2);

    /// <summary>
    /// Select the best matches, taking into account tag order into a new TagedCollection
    /// </summary>
    TagedCollection<TTag, TValue> SelectWeightedMatches(int weightMultiplier, params TTag[] orderedTags);

    /// <summary>
    /// Select matches given tags with specified weights into a new TagedCollection
    /// The higher the weight, the more wanted the tag
    /// </summary>
    TagedCollection<TTag, TValue> SelectWeightedMatches(params (TTag tag, int weight)[] @params);

    /// <summary>
    /// Select the best matches, taking into account tag order into a new TagedCollection
    /// </summary>
    TagedCollection<TTag, TValue> SelectWeightedMatches(params TTag[] orderedTags);
  }
}