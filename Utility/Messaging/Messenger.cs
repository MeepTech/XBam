using System.Collections.Generic;

// TODO: should this be it's own library?
namespace Meep.Tech.Messaging {

  /// <summary>
  /// Context for a broadcaster built into the universe.
  /// </summary>
  public class Messenger : Meep.Tech.XBam.Universe.ExtraContext, IBroadcaster {
    internal IBroadcaster _broadcaster;

    internal Messenger(IBroadcaster broadcaster = null) {
      _broadcaster = broadcaster ?? new Broadcaster();
    }

    ///<summary><inheritdoc/></summary>
    public void Announce(IEvent @event, HashSet<Event.Tag> extraTags = null) 
      => _broadcaster.Announce(@event, extraTags);

    ///<summary><inheritdoc/></summary>
    public void Subscribe(IObserver observer, IEnumerable<Event.Tag> tags) 
      => _broadcaster.Subscribe(observer, tags);

    ///<summary><inheritdoc/></summary>
    public void UnSubscribe(IObserver observer, IEnumerable<Event.Tag> tags) 
      => _broadcaster.UnSubscribe(observer, tags);
  }
}