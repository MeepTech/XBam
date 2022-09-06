using Meep.Tech.Collections.Generic;
using Meep.Tech.Reflection;
using System;
using System.Collections.Generic;

namespace Meep.Tech.XBam {

  /// <summary>
  /// A tag for a resource.
  /// </summary>
  public interface ITag {

    /// <summary>
    /// The key for the tag.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Make a version of this tag with some required extra context.
    /// Can be used to make specific tags like 'level-up|[CHARACTERID]' vs just 'level-up'
    /// </summary>
    ITag WithExtraContext(params string[] extraContexts);

    /// <summary>
    /// Get a tag of the requested type by it's key.
    /// </summary>
    public static ITag Get<TTag>(string key, Universe universe = null)
      where TTag : Tag<TTag>
        => Tag<TTag>.Get(key, universe);
  }

  /// <summary>
  /// Marks a type of tag as usergenerateable.
  /// </summary>
  public class UserGenerateableAttribute : Attribute { }

  /// <summary>
  /// A tag for a resource.
  /// </summary>
  public class Tag<TTag> : Enumeration<TTag>, ITag where TTag : Tag<TTag> {

    ///<summary><inheritdoc/></summary>
    public string Key 
      => ExternalId.ToString();

    /// <summary>
    /// Make a new tag.
    /// </summary>
    protected Tag(string key)
      : base(key) { }

    Tag(string key, bool registerAsNew)
      : base(typeof(TTag).FullName + key, registerAsNew) {
      Universe.All.Values.ForEach(universe => {
        if (universe.Enumerations._tagsByTypeAndKey.TryGetValue(typeof(TTag).FullName, out var tags)) {
          tags.Add(key, this);
        }
      });
    }

    /// <summary>
    /// Get a tag of this base type by it's key.
    /// </summary>
    public static Tag<TTag> Get(string key, Universe universe = null)
      => (universe ?? Universe.Default).Enumerations._tagsByTypeAndKey.TryGetValue(typeof(TTag).FullName, out var tags)
        ? (TTag)tags[key]
        : typeof(TTag).HasAttribute<UserGenerateableAttribute>()  
          ? (TTag)(tags[key] = new Tag<TTag>(key, true))
          : throw new KeyNotFoundException($"Tag with key: {key}, not found, and cannot be generated as : {typeof(TTag).ToFullHumanReadableNameString()} does not have a UserGenerateableAttribute.");

    /// <summary>
    /// Make a version of this tag with some required extra context.
    /// Can be used to make specific events like 'level-up|[CHARACTERID]' vs just 'level-up'
    /// </summary>
    public Tag<TTag> WithExtraContext(params string[] extraContexts) {
      string key = ExternalId as string + string.Join('|', extraContexts);
      return (Tag<TTag>)(Universe.Default.Enumerations._withExtraContext.TryGetValue(key, out ITag existing)
        ? existing
        : (Universe.Default.Enumerations._withExtraContext[key] = new Tag<TTag>(key, false)));
    }

    /// <summary>
    /// Make a version of this tag with some required extra context.
    /// Can be used to make specific events like 'level-up|[CHARACTERID]' vs just 'level-up'
    /// </summary>
    public Tag<TTag> WithExtraContext(Universe universe, params string[] extraContexts) {
      string key = ExternalId as string + string.Join('|', extraContexts);
      return (Tag<TTag>)(universe.Enumerations._withExtraContext.TryGetValue(key, out ITag existing)
        ? existing
        : (universe.Enumerations._withExtraContext[key] = new Tag<TTag>(key, false)));
    }

    ITag ITag.WithExtraContext(params string[] extraContexts)
      => WithExtraContext(extraContexts);
  }
}
