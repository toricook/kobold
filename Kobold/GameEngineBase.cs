using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Assets;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Abstractions.Audio;
using Kobold.Core.Events;
using Kobold.Core.Systems;

namespace Kobold.Core
{
    public abstract class GameEngineBase : IGameEngine
    {
        protected World World;
        protected IRenderer Renderer;
        protected IInputManager InputManager;
        protected IContentLoader ContentLoader;
        protected IAudioPlayer AudioPlayer;
        protected AssetManager Assets;
        protected AudioManager Audio;
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

        public void SetContentLoader(IContentLoader contentLoader)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Cannot set content loader after initialization");
            ContentLoader = contentLoader;
        }

        public void SetAudioPlayer(IAudioPlayer audioPlayer)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Cannot set audio player after initialization");
            AudioPlayer = audioPlayer;
        }

        public virtual void Initialize()
        {
            if (Renderer == null)
                throw new InvalidOperationException("Renderer must be set before initialization");
            if (InputManager == null)
                throw new InvalidOperationException("InputManager must be set before initialization");
            if (ContentLoader == null)
                throw new InvalidOperationException("ContentLoader must be set before initialization");
            if (AudioPlayer == null)
                throw new InvalidOperationException("AudioPlayer must be set before initialization");

            // Create AssetManager with the ContentLoader and content root
            Assets = new AssetManager(ContentLoader, ContentLoader.ContentRoot);

            // Create AudioManager with AssetManager and AudioPlayer
            Audio = new AudioManager(Assets, AudioPlayer);

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
            SystemManager.RenderAll();
            Renderer.End();
        }

        public virtual void Shutdown()
        {
            // Stop any playing music
            Audio?.StopMusic();

            // Unload all assets
            Assets?.UnloadAll();

            SystemManager.ClearSystems();
            EventBus.Clear();
            World.Dispose();
        }
    }
}