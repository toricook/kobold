namespace Kobold.Core.Events
{
    /// <summary>
    /// Marker interface for all events in the Kobold event system.
    /// Implement this interface on your custom event types to make them publishable through the EventBus.
    /// </summary>
    /// <remarks>
    /// Events are used for decoupled communication between systems and components.
    /// Instead of systems directly calling each other, they publish events that other systems can subscribe to.
    /// This promotes loose coupling and makes systems more reusable and testable.
    ///
    /// Example:
    /// <code>
    /// public class PlayerDefeatedEvent : BaseEvent
    /// {
    ///     public Entity PlayerEntity { get; }
    ///     public Entity KillerEntity { get; }
    ///
    ///     public PlayerDefeatedEvent(Entity player, Entity killer)
    ///     {
    ///         PlayerEntity = player;
    ///         KillerEntity = killer;
    ///     }
    /// }
    /// </code>
    /// </remarks>
    /// <seealso cref="EventBus"/>
    /// <seealso cref="BaseEvent"/>
    /// <seealso cref="IEventHandler{T}"/>
    public interface IEvent
    {
    }
}
