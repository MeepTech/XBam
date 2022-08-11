using System.Collections.Generic;

namespace Meep.Tech.Messaging {

  /// <summary>
  /// Base interface for all events
  /// </summary>
  public interface IEvent {

    /// <summary>
    /// The name of the event.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The tags for this event.
    /// </summary>
    public HashSet<Event.Tag> Tags { get; }
  }

  /// <summary>
  /// Base class for object based events.
  /// </summary>
  public abstract partial class Event : IEvent {

    /// <summary>
    /// Overrideable event name.
    /// </summary>
    public virtual string Name 
      => GetType().Name;

    /// <summary>
    /// The tags for this event. This determins who will be notified.
    /// </summary>
    public abstract HashSet<Tag> Tags {
      get;
    }
  }
}