using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Events;
using Kobold.Core.Systems;

namespace Kobold.Core
{
    public abstract class GameEngineBase : IGameEngine
    {
        protected World World;
        protected IRenderer Renderer;
        protected IInputManager InputManager;
        protected EventBus EventBus;
        protected SystemManager SystemManager;

        private bool _isInitialized = false;

        public GameEngineBase()
        {
            World = World.Create();
            EventBus = new EventBus();
            SystemManager = new SystemManager(EventBus, World);
        }

        // Alternative constructor for direct injection
        public GameEngineBase(IRenderer renderer, IInputManager inputManager) : this()
        {
            Renderer = renderer;
            InputManager = inputManager;
        }

        // Setters for post-construction injection
        public void SetRenderer(IRenderer renderer)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Cannot set renderer after initialization");
            Renderer = renderer;
        }

        public void SetInputManager(IInputManager inputManager)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Cannot set input manager after initialization");
            InputManager = inputManager;
        }

        public virtual void Initialize()
        {
            if (Renderer == null)
                throw new InvalidOperationException("Renderer must be set before initialization");
            if (InputManager == null)
                throw new InvalidOperationException("InputManager must be set before initialization");

            _isInitialized = true;
        }

        public virtual void Update(float deltaTime)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Game engine must be initialized before updating");

            SystemManager.UpdateAll(deltaTime);
        }

        public virtual void Render()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Game engine must be initialized before rendering");
            Renderer.Begin();
            // Override in derived classes
            Renderer.End();
        }

        public virtual void Shutdown()
        {
            SystemManager.ClearSystems();
            EventBus.Clear();
            World.Dispose();
        }
    }
}