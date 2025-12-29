using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kobold.Core.Assets;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpriteSheetEditor
{
    public class SpriteSheetEditorGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        // Background
        private Texture2D _checkerboardTexture;

        // Editor state
        private Texture2D? _loadedTexture;
        private string? _loadedTexturePath;

        // Grid settings
        private int _spriteWidth = 32;
        private int _spriteHeight = 32;
        private int _spacing = 0;
        private int _margin = 0;

        // Camera/view
        private Vector2 _cameraPosition = Vector2.Zero;
        private float _zoom = 1.0f;

        // UI state
        private bool _showGrid = true;
        private Dictionary<string, Rectangle> _namedRegions = new Dictionary<string, Rectangle>();
        private Rectangle? _selectedRegion = null;
        private string _currentRegionName = "";

        // Input state
        private KeyboardState _previousKeyState;
        private MouseState _previousMouseState;

        public SpriteSheetEditorGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            Window.Title = "Sprite Sheet Editor";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create checkerboard background texture
            CreateCheckerboardTexture();
        }

        private void CreateCheckerboardTexture()
        {
            // Create a 16x16 checkerboard pattern (8x8 pixel squares)
            int squareSize = 8;
            int textureSize = squareSize * 2;
            _checkerboardTexture = new Texture2D(GraphicsDevice, textureSize, textureSize);

            Color lightGray = new Color(204, 204, 204); // #CCCCCC
            Color darkGray = new Color(153, 153, 153);  // #999999

            Color[] data = new Color[textureSize * textureSize];

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    bool isLightSquare = (x < squareSize) == (y < squareSize);
                    data[y * textureSize + x] = isLightSquare ? lightGray : darkGray;
                }
            }

            _checkerboardTexture.SetData(data);
        }

        protected override void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            // Exit
            if (keyState.IsKeyDown(Keys.Escape))
                Exit();

            // Handle input
            HandleInput(keyState, mouseState);

            // Update window title with current status
            UpdateWindowTitle();

            _previousKeyState = keyState;
            _previousMouseState = mouseState;

            base.Update(gameTime);
        }

        private void UpdateWindowTitle()
        {
            string title = "Sprite Sheet Editor";

            if (_loadedTexture != null)
            {
                int cols = (_loadedTexture.Width - 2 * _margin + _spacing) / (_spriteWidth + _spacing);
                int rows = (_loadedTexture.Height - 2 * _margin + _spacing) / (_spriteHeight + _spacing);
                title += $" | Grid: {_spriteWidth}x{_spriteHeight} | {cols}x{rows} cells ({cols * rows} sprites) | Zoom: {_zoom:F2}x";
            }
            else
            {
                title += " | Press L to load test_sheet.png";
            }

            if (_selectedRegion.HasValue)
            {
                title += $" | Selected: ({_selectedRegion.Value.X}, {_selectedRegion.Value.Y})";
            }

            Window.Title = title;
        }

        private void HandleInput(KeyboardState keyState, MouseState mouseState)
        {
            // Camera controls (WASD)
            float panSpeed = 5f;
            if (keyState.IsKeyDown(Keys.W)) _cameraPosition.Y -= panSpeed;
            if (keyState.IsKeyDown(Keys.S)) _cameraPosition.Y += panSpeed;
            if (keyState.IsKeyDown(Keys.A)) _cameraPosition.X -= panSpeed;
            if (keyState.IsKeyDown(Keys.D)) _cameraPosition.X += panSpeed;

            // Zoom controls (Q/E)
            if (keyState.IsKeyDown(Keys.Q) && !_previousKeyState.IsKeyDown(Keys.Q))
                _zoom = Math.Max(0.25f, _zoom - 0.25f);
            if (keyState.IsKeyDown(Keys.E) && !_previousKeyState.IsKeyDown(Keys.E))
                _zoom = Math.Min(4f, _zoom + 0.25f);

            // Toggle grid
            if (keyState.IsKeyDown(Keys.G) && !_previousKeyState.IsKeyDown(Keys.G))
                _showGrid = !_showGrid;

            // Adjust grid size
            if (keyState.IsKeyDown(Keys.Up) && !_previousKeyState.IsKeyDown(Keys.Up))
                _spriteHeight = Math.Max(1, _spriteHeight + 1);
            if (keyState.IsKeyDown(Keys.Down) && !_previousKeyState.IsKeyDown(Keys.Down))
                _spriteHeight = Math.Max(1, _spriteHeight - 1);
            if (keyState.IsKeyDown(Keys.Right) && !_previousKeyState.IsKeyDown(Keys.Right))
                _spriteWidth = Math.Max(1, _spriteWidth + 1);
            if (keyState.IsKeyDown(Keys.Left) && !_previousKeyState.IsKeyDown(Keys.Left))
                _spriteWidth = Math.Max(1, _spriteWidth - 1);

            // Load texture (L key)
            if (keyState.IsKeyDown(Keys.L) && !_previousKeyState.IsKeyDown(Keys.L))
                LoadTexture();

            // Save config (S key with Ctrl)
            if (keyState.IsKeyDown(Keys.LeftControl) && keyState.IsKeyDown(Keys.S) && !_previousKeyState.IsKeyDown(Keys.S))
                SaveConfig();

            // Mouse selection
            if (_loadedTexture != null && mouseState.LeftButton == ButtonState.Pressed)
            {
                HandleMouseSelection(mouseState);
            }
        }

        private void HandleMouseSelection(MouseState mouseState)
        {
            if (_loadedTexture == null) return;

            // Convert mouse position to world position
            Vector2 mouseWorld = ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));

            // Calculate which grid cell was clicked
            int gridX = (int)((mouseWorld.X - _margin) / (_spriteWidth + _spacing));
            int gridY = (int)((mouseWorld.Y - _margin) / (_spriteHeight + _spacing));

            if (gridX >= 0 && gridY >= 0)
            {
                int x = _margin + gridX * (_spriteWidth + _spacing);
                int y = _margin + gridY * (_spriteHeight + _spacing);

                _selectedRegion = new Rectangle(x, y, _spriteWidth, _spriteHeight);
            }
        }

        private Vector2 ScreenToWorld(Vector2 screenPos)
        {
            return (screenPos / _zoom) + _cameraPosition;
        }

        private Vector2 WorldToScreen(Vector2 worldPos)
        {
            return (worldPos - _cameraPosition) * _zoom;
        }

        private void LoadTexture()
        {
            // For now, use a hardcoded path. In Phase 2, we can add a file dialog.
            string testPath = "test_sheet.png";

            if (File.Exists(testPath))
            {
                using (var stream = File.OpenRead(testPath))
                {
                    _loadedTexture = Texture2D.FromStream(GraphicsDevice, stream);
                    _loadedTexturePath = testPath;
                }
            }
        }

        private void SaveConfig()
        {
            if (_loadedTexture == null || string.IsNullOrEmpty(_loadedTexturePath))
                return;

            var config = new SpriteSheetConfig
            {
                SpriteWidth = _spriteWidth,
                SpriteHeight = _spriteHeight,
                Columns = (_loadedTexture.Width - 2 * _margin + _spacing) / (_spriteWidth + _spacing),
                Rows = (_loadedTexture.Height - 2 * _margin + _spacing) / (_spriteHeight + _spacing),
                Spacing = _spacing,
                Margin = _margin,
                Pivot = new Kobold.Core.Assets.Vector2Data { X = 0.5f, Y = 0.5f }
            };

            // Add named regions
            foreach (var region in _namedRegions)
            {
                config.NamedRegions[region.Key] = new Kobold.Core.Assets.RectangleData
                {
                    X = region.Value.X,
                    Y = region.Value.Y,
                    Width = region.Value.Width,
                    Height = region.Value.Height
                };
            }

            // Save to JSON
            string configPath = Path.ChangeExtension(_loadedTexturePath, ".json");
            System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = System.Text.Json.JsonSerializer.Serialize(config, options);
            File.WriteAllText(configPath, json);

            // Update window title to show save confirmation
            Window.Title = $"Sprite Sheet Editor | SAVED: {configPath}";
            Console.WriteLine($"Saved config to: {configPath}");
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw checkerboard background
            DrawCheckerboardBackground();

            // Draw loaded texture
            if (_loadedTexture != null)
            {
                Vector2 screenPos = WorldToScreen(Vector2.Zero);
                _spriteBatch.Draw(_loadedTexture, screenPos, null, Color.White, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);

                // Draw grid overlay
                if (_showGrid)
                {
                    DrawGrid();
                }

                // Draw selected region highlight
                if (_selectedRegion.HasValue)
                {
                    DrawRectangleOutline(_selectedRegion.Value, Color.Yellow, 2);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGrid()
        {
            if (_loadedTexture == null) return;

            int cols = (_loadedTexture.Width - 2 * _margin + _spacing) / (_spriteWidth + _spacing);
            int rows = (_loadedTexture.Height - 2 * _margin + _spacing) / (_spriteHeight + _spacing);

            // Draw vertical lines
            for (int col = 0; col <= cols; col++)
            {
                int x = _margin + col * (_spriteWidth + _spacing);
                Vector2 start = WorldToScreen(new Vector2(x, _margin));
                Vector2 end = WorldToScreen(new Vector2(x, _margin + rows * (_spriteHeight + _spacing)));
                DrawLine(start, end, Color.Lime);
            }

            // Draw horizontal lines
            for (int row = 0; row <= rows; row++)
            {
                int y = _margin + row * (_spriteHeight + _spacing);
                Vector2 start = WorldToScreen(new Vector2(_margin, y));
                Vector2 end = WorldToScreen(new Vector2(_margin + cols * (_spriteWidth + _spacing), y));
                DrawLine(start, end, Color.Lime);
            }
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            // Simple line drawing using a 1x1 pixel
            var pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { color });

            float distance = Vector2.Distance(start, end);
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            _spriteBatch.Draw(pixel, start, null, color, angle, Vector2.Zero, new Vector2(distance, 1), SpriteEffects.None, 0);
        }

        private void DrawRectangleOutline(Rectangle rect, Color color, int thickness)
        {
            Vector2 topLeft = WorldToScreen(new Vector2(rect.X, rect.Y));
            Vector2 topRight = WorldToScreen(new Vector2(rect.X + rect.Width, rect.Y));
            Vector2 bottomLeft = WorldToScreen(new Vector2(rect.X, rect.Y + rect.Height));
            Vector2 bottomRight = WorldToScreen(new Vector2(rect.X + rect.Width, rect.Y + rect.Height));

            DrawLine(topLeft, topRight, color);
            DrawLine(topRight, bottomRight, color);
            DrawLine(bottomRight, bottomLeft, color);
            DrawLine(bottomLeft, topLeft, color);
        }

        private void DrawCheckerboardBackground()
        {
            // Calculate visible area in world space
            Vector2 topLeft = ScreenToWorld(Vector2.Zero);
            Vector2 bottomRight = ScreenToWorld(new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight));

            // Tile the checkerboard texture across the visible area
            int tileSize = _checkerboardTexture.Width;
            int startX = (int)(topLeft.X / tileSize) * tileSize;
            int startY = (int)(topLeft.Y / tileSize) * tileSize;
            int endX = (int)(bottomRight.X / tileSize + 1) * tileSize;
            int endY = (int)(bottomRight.Y / tileSize + 1) * tileSize;

            for (int y = startY; y < endY; y += tileSize)
            {
                for (int x = startX; x < endX; x += tileSize)
                {
                    Vector2 screenPos = WorldToScreen(new Vector2(x, y));
                    _spriteBatch.Draw(_checkerboardTexture, screenPos, null, Color.White, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
                }
            }
        }

    }
}
