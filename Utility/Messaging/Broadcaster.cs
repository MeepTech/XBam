using Meep.Tech.Collections.Generic;
using Meep.Tech.XBam;
using System.Collections.Generic;

// TODO: should this be it's own library?
namespace Meep.Tech.Messaging {

  /// <summary>
  /// Can be used to send/recive events
  /// </summary>
  public interface IBroadcaster {

    /// <summary>
    /// Send the event announcement to all applicable observers subscribed to this broadcaster, taking into account the event's tags.
    /// </summary>
    void Announce(IEvent @event, HashSet<Event.Tag> extraTags = null);

    /// <summary>
    /// Subscribe the given observer to the given tags/channels
    /// </summary>
    void Subscribe(IObserver observer, IEnumerable<Event.Tag> tags);

    /// <summary>
    /// UnSubscribe the given observer from the given tags/channels
    /// </summary>
    void UnSubscribe(IObserver observer, IEnumerable<Event.Tag> tags);
  }

  /// <summary>
  /// Can be used to send/recive events
  /// </summary>
  public class Broadcaster : IBroadcaster {
    Dictionary<Event.Tag, ICollection<IObserver>> _listeners = new();

    ///<summary><inheritdoc/></summary>
    public void Announce(IEvent @event, HashSet<Event.Tag> extraTags = null) {
      @event.Tags.ConcatIfNotNull(extraTags)
        .ForEach(t => _listeners[t]
          .ForEach(o => o.Observe(@event)));
    }

    ///<summary><inheritdoc/></summary>
    public void Subscribe(IObserver observer, IEnumerable<Event.Tag> tags) {
      tags.ForEach(t => _listeners.AddToValueCollection(t, observer));
    }

    ///<summary><inheritdoc/></summary>
    public void UnSubscribe(IObserver observer, IEnumerable<Event.Tag> tags) {
      tags.ForEach(t => _listeners.RemoveFromValueCollection(t, observer));
    }
  }

  /// <summary>
  /// Helper extensions for broadcasting events.
  /// </summary>
  public static class BroadcasterExtensions {

    /// <summary>
    /// Set up a default broadcaster/messenger for this universe.
    /// </summary>
    /// <param name="broadcaster">(optional) provide your own broadcaster. If this isnt provided, a default one is made.</param>
    public static void SetUpBroadcaster(this Universe universe, IBroadcaster broadcaster = null)
      => universe.SetExtraContext<Messenger>(new(broadcaster));

    /// <summary>
    /// Get the broadcaster attached to this universe, if there is one.
    /// </summary>
    /// <param name="universe"></param>
    /// <returns></returns>
    public static IBroadcaster GetBroadcaster(this Universe universe)
      => universe.GetExtraContext<Messenger>()?._broadcaster;

    /// <summary>
    /// Send the event announcement to all applicable observers subscribed to the messenger/broadcaster attached to this universe.
    /// </summary>
    public static void Broadcast(this Universe universe, IEvent @event, HashSet<Event.Tag> extraTags = null) 
      => universe.GetBroadcaster().Announce(@event, extraTags);

    /// <summary>
    /// Send the event announcement to all applicable observers subscribed to the messenger/broadcaster attached to this observer's universe.
    /// </summary>
    public static void Broadcast<TObserver>(this TObserver observer, IEvent @event, HashSet<Event.Tag> extraTags = null) 
      where TObserver : IObserver, IModel
        => observer.Universe.GetBroadcaster().Announce(@event, extraTags);

    /// <summary>
    /// Unsubscribe this observer to the given tags.
    /// </summary>
    public static void SubscribeTo<TObserver>(this TObserver observer, HashSet<Event.Tag> tags) 
      where TObserver : IObserver, IModel
        => observer.Universe.GetBroadcaster().Subscribe(observer, tags);

    /// <summary>
    /// Unsubscribe this observer from the given tags.
    /// </summary>
    public static void UnSubscribeFrom<TObserver>(this TObserver observer, HashSet<Event.Tag> tags) 
      where TObserver : IObserver, IModel
        => observer.Universe.GetBroadcaster().UnSubscribe(observer, tags);
  }
}