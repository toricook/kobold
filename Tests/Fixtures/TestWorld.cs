using Arch.Core;
using Kobold.Core.Events;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Tests.Helpers
{
    public class TestWorld : IDisposable
    {
        public World World { get; }
        public EventBus EventBus { get; }
        public List<IEvent> PublishedEvents { get; }

        public TestWorld()
        {
            World = World.Create();
            EventBus = new EventBus();
            PublishedEvents = new List<IEvent>();

            // Capture all events for assertions
            EventBus.Subscribe<IEvent>(evt => PublishedEvents.Add(evt));
        }

        public T GetLastEvent<T>() where T : IEvent
        {
            return PublishedEvents.OfType<T>().LastOrDefault();
        }

        public IEnumerable<T> GetEvents<T>() where T : IEvent
        {
            return PublishedEvents.OfType<T>();
        }

        public void ClearEvents()
        {
            PublishedEvents.Clear();
        }

        public void Dispose()
        {
            World.Dispose();
            EventBus.Clear();
        }
    }
}