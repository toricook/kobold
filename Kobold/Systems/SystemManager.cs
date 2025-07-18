using Kobold.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    public class SystemManager
    {
        private readonly List<ISystem> _systems = new();
        private readonly EventBus _eventBus;

        public SystemManager(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void AddSystem(ISystem system)
        {
            _systems.Add(system);
        }

        public void RemoveSystem(ISystem system)
        {
            _systems.Remove(system);
        }

        public void UpdateAll(float deltaTime)
        {
            foreach (var system in _systems)
            {
                system.Update(deltaTime);
            }
        }

        public void ClearSystems()
        {
            _systems.Clear();
        }

        public T GetSystem<T>() where T : class, ISystem
        {
            foreach (var system in _systems)
            {
                if (system is T typedSystem)
                {
                    return typedSystem;
                }
            }
            return null;
        }
    }
}
