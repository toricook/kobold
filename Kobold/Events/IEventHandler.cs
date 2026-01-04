namespace Kobold.Core.Events
{
    /// <summary>
    /// Interface for handling events of a specific type.
    /// Implement this interface to create strongly-typed event handlers that can be registered with the EventBus.
    /// </summary>
    /// <typeparam name="T">The type of event this handler processes</typeparam>
    /// <remarks>
    /// Event handlers are registered with the EventBus via Subscribe() and are automatically invoked
    /// when events of the matching type are published.
    ///
    /// Example:
    /// <code>
    /// public class PlayerDefeatedHandler : IEventHandler&lt;PlayerDefeatedEvent&gt;
    /// {
    ///     public void Handle(PlayerDefeatedEvent evt)
    ///     {
    ///         Console.WriteLine($"Player {evt.PlayerEntity} was defeated!");
    ///         // Trigger game over logic, update UI, etc.
    ///     }
    /// }
    ///
    /// // Register the handler:
    /// eventBus.Subscribe(new PlayerDefeatedHandler());
    ///
    /// // Or use lambda syntax:
    /// eventBus.Subscribe&lt;PlayerDefeatedEvent&gt;(evt => {
    ///     Console.WriteLine("Player defeated!");
    /// });
    /// </code>
    /// </remarks>
    /// <seealso cref="EventBus"/>
    /// <seealso cref="IEvent"/>
    public interface IEventHandler<in T> where T : IEvent
    {
        /// <summary>
        /// Handles the specified event.
        /// This method is called by the EventBus when an event of type T is published.
        /// </summary>
        /// <param name="eventData">The event data to process</param>
        void Handle(T eventData);
    }
}
