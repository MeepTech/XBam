using Meep.Tech.XBam;
using System.Collections.Generic;

namespace Meep.Tech.Messaging {
  public abstract partial class Event {

    /// <summary>
    /// A tag for a an event. Tags work like channels that can be listened into by observers.
    /// </summary>
    public class Tag : XBam.Tag<Tag> {
      static readonly Dictionary<string, Tag> _withExtraContext = new();

      /// <summary>
      /// Make a new tag.
      /// </summary>
      public Tag(string key, Universe universe = null)
        : base(key, universe) { }
    }
  }
}