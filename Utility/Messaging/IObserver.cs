namespace Meep.Tech.Messaging {

  /// <summary>
  /// An object capable of observing, and reacting to, events.
  /// </summary>
  public interface IObserver {

    /// <summary>
    /// How this observer reacts when it observes an event.
    /// </summary>
    internal protected void Observe(IEvent @event);
  }
}