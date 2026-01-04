using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Assets;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Abstractions.Audio;
using Kobold.Core.Events;
using Kobold.Core.Services;
using Kobold.Core.Scenes;

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
        protected SceneManager SceneManager;

        private bool _isInitialized = false;

        public GameEngineBase()
        {
            World = World.Create();
            EventBus = new EventBus();
            SystemManager = new SystemManager(OnSystemError);
        }

        /// <summary>
        /// Default error handler for system exceptions. Logs the error and continues execution.
        /// Override this method to customize error handling behavior.
        /// </summary>
        protected virtual void OnSystemError(ISystem system, Exception exception, string context)
        {
            Console.Error.WriteLine($"[ERROR] System {system?.GetType().Name} threw exception during {context}:");
            Console.Error.WriteLine(exception);
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

            // Create SceneManager with all required services
            SceneManager = new SceneManager(World, Assets, Audio, EventBus, SystemManager, Renderer, InputManager);

            _isInitialized = true;
        }

        public virtual void Update(float deltaTime)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Game engine must be initialized before updating");

            // Update scene manager first (handles transitions and scene updates)
            SceneManager.Update(deltaTime);

            // Determine which system groups should run based on game state
            // If a scene is active, use its system group; otherwise fall back to legacy method
            var activeGroups = SceneManager.HasActiveScene
                ? SceneManager.GetActiveSystemGroup()
                : GetActiveSystemGroups();

            SystemManager.UpdateAll(deltaTime, activeGroups);
        }

        /// <summary>
        /// Determines which system groups should be active based on the current game state.
        /// Override this method to customize which systems run in different game states.
        /// Default behavior: runs Always and Playing systems.
        /// This method is used as a fallback when no scene is active.
        /// </summary>
        protected virtual SystemGroup GetActiveSystemGroups()
        {
            // Default: run always-active systems and playing systems
            // Override this method in derived classes for custom behavior
            return SystemGroup.Always | SystemGroup.Playing;
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
            // Unload any active scenes
            SceneManager?.UnloadCurrentScene();

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