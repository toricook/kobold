using Kobold.Extensions.Physics.Systems;
using Kobold.Extensions.Physics.Components;
using Kobold.Extensions.Collision.Systems;
using Kobold.Extensions.Collision.Components;
using Kobold.Extensions.Input.Systems;
using Kobold.Extensions.Input.Components;
using Kobold.Extensions.Boundaries.Systems;
using Kobold.Extensions.Boundaries.Components;
using Kobold.Extensions.Destruction.Systems;
using Kobold.Extensions.Destruction.Components;
using Kobold.Extensions.Gameplay.Components;
﻿using Arch.Core;
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