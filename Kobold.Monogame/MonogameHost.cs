using Kobold.Core;
using Kobold.Core.Abstractions;
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

        public MonoGameHost(IGameEngine gameEngine)
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _gameEngine = gameEngine;

            // Set window size for Pong
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
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

            // Inject dependencies into game engine
            if (_gameEngine is GameEngineBase gameEngineBase)
            {
                gameEngineBase.SetRenderer(renderer);
                gameEngineBase.SetInputManager(_inputManager);
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
            GraphicsDevice.Clear(Color.Black); // Black background for Pong
            _gameEngine.Render();
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _gameEngine.Shutdown();
        }
    }
}