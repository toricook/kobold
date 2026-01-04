using System;
using System.Collections.Generic;
using System.Linq;

namespace Kobold.Core.Events
{
    /// <summary>
    /// Central event bus for publish-subscribe messaging between game systems.
    /// Provides decoupled communication where publishers don't need to know about subscribers.
    /// </summary>
    /// <remarks>
    /// The EventBus enables loose coupling between systems by allowing them to communicate via events
    /// instead of direct method calls. Systems publish events when something interesting happens,
    /// and other systems subscribe to receive notifications.
    ///
    /// Features:
    /// - Type-safe event handling with generics
    /// - Support for event inheritance (handlers for base types receive derived events)
    /// - Lambda-based subscription for convenience
    /// - Automatic cleanup of handlers
    ///
    /// Example usage:
    /// <code>
    /// // Create the event bus (typically done once in your game engine)
    /// var eventBus = new EventBus();
    ///
    /// // Subscribe to events using lambda:
    /// eventBus.Subscribe&lt;PlayerDefeatedEvent&gt;(evt => {
    ///     Console.WriteLine($"Player defeated at {evt.Timestamp}");
    /// });
    ///
    /// // Or using a handler class:
    /// eventBus.Subscribe(new PlayerDefeatedHandler());
    ///
    /// // Publish an event:
    /// eventBus.Publish(new PlayerDefeatedEvent(playerEntity, enemyEntity));
    /// </code>
    /// </remarks>
    /// <seealso cref="IEvent"/>
    /// <seealso cref="IEventHandler{T}"/>
    /// <seealso cref="BaseEvent"/>
    public class EventBus
    {
        private readonly Dictionary<Type, List<object>> _handlers = new();

        public void Subscribe<T>(IEventHandler<T> handler) where T : IEvent
        {
            var eventType = typeof(T);

            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<object>();
            }

            _handlers[eventType].Add(handler);
        }

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            Subscribe(new ActionEventHandler<T>(handler));
        }

        public void Unsubscribe<T>(IEventHandler<T> handler) where T : IEvent
        {
            var eventType = typeof(T);

            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);

                if (handlers.Count == 0)
                {
                    _handlers.Remove(eventType);
                }
            }
        }

        public void Publish<T>(T eventData) where T : IEvent
        {
            var eventType = eventData.GetType();

            // Publish to handlers for this specific type and all base types/interfaces
            foreach (var type in GetEventTypeHierarchy(eventType))
            {
                if (_handlers.TryGetValue(type, out var handlers))
                {
                    foreach (var handler in handlers.ToList()) // ToList to avoid modification during iteration
                    {
                        if (handler is IEventHandler<T> typedHandler)
                        {
                            typedHandler.Handle(eventData);
                        }
                        else
                        {
                            // Handle case where handler expects a base type
                            var handleMethod = handler.GetType().GetMethod("Handle");
                            if (handleMethod != null)
                            {
                                try
                                {
                                    handleMethod.Invoke(handler, new object[] { eventData });
                                }
                                catch
                                {
                                    // Ignore if invoke fails (wrong type)
                                }
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<Type> GetEventTypeHierarchy(Type type)
        {
            // Return the type itself
            yield return type;

            // Return all base classes
            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }

            // Return all interfaces
            foreach (var interfaceType in type.GetInterfaces())
            {
                yield return interfaceType;
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }

    // Helper class for Action-based handlers
    internal class ActionEventHandler<T> : IEventHandler<T> where T : IEvent
    {
        private readonly Action<T> _action;

        public ActionEventHandler(Action<T> action)
        {
            _action = action;
        }

        public void Handle(T eventData)
        {
            _action(eventData);
        }
    }
}
