using Kobold.Core;
using Kobold.Core.Abstractions.Core;
using Kobold.Core.Configuration;
using Kobold.Monogame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kobold.Monogame
{
    public class MonoGameHost : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _defaultFont;

        private IGameEngine _gameEngine;
        private MonoGameInputManager _inputManager;

        private Color _backgroundColor;

        public MonoGameHost(IGameEngine gameEngine, GameConfig? config = null)
        {
            var gameConfig = config ?? new GameConfig();   

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _gameEngine = gameEngine;

            // Set window size
            _graphics.PreferredBackBufferWidth = gameConfig.ScreenWidth;
            _graphics.PreferredBackBufferHeight = gameConfig.ScreenHeight;


            // Set clear color
            _backgroundColor = MonoGameRenderer.ToXnaColor(gameConfig.BackgroundColor);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            if (File.Exists(Path.Combine(Content.RootDirectory, "DefaultFont.xnb")))
            {
                _defaultFont = Content.Load<SpriteFont>("DefaultFont");
            }
            // Create MonoGame-specific implementations
            _inputManager = new MonoGameInputManager();
            var renderer = new MonoGameRenderer(GraphicsDevice, _spriteBatch, _defaultFont);
            var contentLoader = new MonoGameContentLoader(GraphicsDevice, Content.RootDirectory);

            // Inject dependencies into game engine
            if (_gameEngine is GameEngineBase gameEngineBase)
            {
                gameEngineBase.SetRenderer(renderer);
                gameEngineBase.SetInputManager(_inputManager);
                gameEngineBase.SetContentLoader(contentLoader);
            }

            // Initialize the game engine
            _gameEngine.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _inputManager.Update(); // Update input state
            _gameEngine.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backgroundColor);
            _gameEngine.Render();
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _gameEngine.Shutdown();
        }
    }
}