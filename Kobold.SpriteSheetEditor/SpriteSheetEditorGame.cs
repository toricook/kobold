using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kobold.Core.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace SpriteSheetEditor
{
    public class SpriteSheetEditorGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        // Background
        private Texture2D _checkerboardTexture;

        // Simple bitmap font
        private Texture2D _fontTexture;
        private const int CHAR_WIDTH = 6;
        private const int CHAR_HEIGHT = 8;

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
        private bool _isNamingMode = false;
        private string _inputBuffer = "";
        private bool _showHelp = true;

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

            // Create simple bitmap font
            CreateFontTexture();

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

        private void CreateFontTexture()
        {
            // Create a 5x7 bitmap font for ASCII 32-126 (95 printable characters)
            int charsPerRow = 16;
            int numRows = 6;
            int texWidth = CHAR_WIDTH * charsPerRow;
            int texHeight = CHAR_HEIGHT * numRows;

            _fontTexture = new Texture2D(GraphicsDevice, texWidth, texHeight);
            Color[] data = new Color[texWidth * texHeight];

            // Fill with transparent
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            // Simple 5x7 font data for all printable ASCII (each char is 7 bytes, bits represent pixels)
            // Format: each byte is a row, bits 0-4 are the 5 pixels
            byte[][] fontData = new byte[95][];

            // Space (32)
            fontData[0] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            // ! (33)
            fontData[1] = new byte[] { 0x04, 0x04, 0x04, 0x04, 0x00, 0x04, 0x00 };
            // " (34)
            fontData[2] = new byte[] { 0x0A, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00 };
            // Skip to commonly used characters...

            // 0-9 (48-57)
            fontData[48-32] = new byte[] { 0x0E, 0x11, 0x13, 0x15, 0x19, 0x0E, 0x00 }; // 0
            fontData[49-32] = new byte[] { 0x04, 0x0C, 0x04, 0x04, 0x04, 0x0E, 0x00 }; // 1
            fontData[50-32] = new byte[] { 0x0E, 0x11, 0x02, 0x04, 0x08, 0x1F, 0x00 }; // 2
            fontData[51-32] = new byte[] { 0x0E, 0x11, 0x06, 0x01, 0x11, 0x0E, 0x00 }; // 3
            fontData[52-32] = new byte[] { 0x02, 0x06, 0x0A, 0x12, 0x1F, 0x02, 0x00 }; // 4
            fontData[53-32] = new byte[] { 0x1F, 0x10, 0x1E, 0x01, 0x11, 0x0E, 0x00 }; // 5
            fontData[54-32] = new byte[] { 0x06, 0x08, 0x1E, 0x11, 0x11, 0x0E, 0x00 }; // 6
            fontData[55-32] = new byte[] { 0x1F, 0x01, 0x02, 0x04, 0x08, 0x08, 0x00 }; // 7
            fontData[56-32] = new byte[] { 0x0E, 0x11, 0x0E, 0x11, 0x11, 0x0E, 0x00 }; // 8
            fontData[57-32] = new byte[] { 0x0E, 0x11, 0x11, 0x0F, 0x01, 0x0E, 0x00 }; // 9

            // : (58)
            fontData[58-32] = new byte[] { 0x00, 0x04, 0x00, 0x00, 0x04, 0x00, 0x00 };

            // A-Z (65-90)
            fontData[65-32] = new byte[] { 0x0E, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x00 }; // A
            fontData[66-32] = new byte[] { 0x1E, 0x11, 0x1E, 0x11, 0x11, 0x1E, 0x00 }; // B
            fontData[67-32] = new byte[] { 0x0E, 0x11, 0x10, 0x10, 0x11, 0x0E, 0x00 }; // C
            fontData[68-32] = new byte[] { 0x1E, 0x11, 0x11, 0x11, 0x11, 0x1E, 0x00 }; // D
            fontData[69-32] = new byte[] { 0x1F, 0x10, 0x1E, 0x10, 0x10, 0x1F, 0x00 }; // E
            fontData[70-32] = new byte[] { 0x1F, 0x10, 0x1E, 0x10, 0x10, 0x10, 0x00 }; // F
            fontData[71-32] = new byte[] { 0x0E, 0x11, 0x10, 0x17, 0x11, 0x0F, 0x00 }; // G
            fontData[72-32] = new byte[] { 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11, 0x00 }; // H
            fontData[73-32] = new byte[] { 0x0E, 0x04, 0x04, 0x04, 0x04, 0x0E, 0x00 }; // I
            fontData[74-32] = new byte[] { 0x01, 0x01, 0x01, 0x11, 0x11, 0x0E, 0x00 }; // J
            fontData[75-32] = new byte[] { 0x11, 0x12, 0x1C, 0x12, 0x11, 0x11, 0x00 }; // K
            fontData[76-32] = new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x1F, 0x00 }; // L
            fontData[77-32] = new byte[] { 0x11, 0x1B, 0x15, 0x11, 0x11, 0x11, 0x00 }; // M
            fontData[78-32] = new byte[] { 0x11, 0x19, 0x15, 0x13, 0x11, 0x11, 0x00 }; // N
            fontData[79-32] = new byte[] { 0x0E, 0x11, 0x11, 0x11, 0x11, 0x0E, 0x00 }; // O
            fontData[80-32] = new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x10, 0x10, 0x00 }; // P
            fontData[81-32] = new byte[] { 0x0E, 0x11, 0x11, 0x15, 0x12, 0x0D, 0x00 }; // Q
            fontData[82-32] = new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x12, 0x11, 0x00 }; // R
            fontData[83-32] = new byte[] { 0x0E, 0x11, 0x0C, 0x03, 0x11, 0x0E, 0x00 }; // S
            fontData[84-32] = new byte[] { 0x1F, 0x04, 0x04, 0x04, 0x04, 0x04, 0x00 }; // T
            fontData[85-32] = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x0E, 0x00 }; // U
            fontData[86-32] = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x0A, 0x04, 0x00 }; // V
            fontData[87-32] = new byte[] { 0x11, 0x11, 0x11, 0x15, 0x1B, 0x11, 0x00 }; // W
            fontData[88-32] = new byte[] { 0x11, 0x11, 0x0A, 0x04, 0x0A, 0x11, 0x00 }; // X
            fontData[89-32] = new byte[] { 0x11, 0x11, 0x0A, 0x04, 0x04, 0x04, 0x00 }; // Y
            fontData[90-32] = new byte[] { 0x1F, 0x01, 0x02, 0x04, 0x08, 0x1F, 0x00 }; // Z

            // a-z (97-122) - same as uppercase for simplicity
            for (int i = 0; i < 26; i++)
            {
                fontData[97-32+i] = fontData[65-32+i];
            }

            // Common symbols
            fontData[40-32] = new byte[] { 0x02, 0x04, 0x08, 0x08, 0x04, 0x02, 0x00 }; // (
            fontData[41-32] = new byte[] { 0x08, 0x04, 0x02, 0x02, 0x04, 0x08, 0x00 }; // )
            fontData[45-32] = new byte[] { 0x00, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x00 }; // -
            fontData[46-32] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00 }; // .
            fontData[47-32] = new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x00, 0x00 }; // /
            fontData[95-32] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x00 }; // _
            fontData[124-32] = new byte[] { 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x00 }; // |

            // Render all characters
            for (int ch = 0; ch < 95; ch++)
            {
                if (fontData[ch] != null)
                {
                    DrawCharFromData(data, texWidth, ch, fontData[ch]);
                }
            }

            _fontTexture.SetData(data);
        }

        private void DrawCharFromData(Color[] data, int texWidth, int charIndex, byte[] charData)
        {
            int charsPerRow = 16;
            int charX = (charIndex % charsPerRow) * CHAR_WIDTH;
            int charY = (charIndex / charsPerRow) * CHAR_HEIGHT;

            for (int y = 0; y < 7 && y < charData.Length; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    if ((charData[y] & (1 << (4 - x))) != 0)
                    {
                        int px = charX + x;
                        int py = charY + y;
                        if (px < texWidth && py < texWidth / charsPerRow * CHAR_HEIGHT)
                        {
                            data[py * texWidth + px] = Color.White;
                        }
                    }
                }
            }
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
                title += " | Press F to load a sprite sheet";
            }

            if (_selectedRegion.HasValue)
            {
                title += $" | Selected: ({_selectedRegion.Value.X}, {_selectedRegion.Value.Y})";
            }

            Window.Title = title;
        }

        private void HandleInput(KeyboardState keyState, MouseState mouseState)
        {
            // Toggle help
            if (keyState.IsKeyDown(Keys.H) && !_previousKeyState.IsKeyDown(Keys.H))
                _showHelp = !_showHelp;

            // Handle naming mode separately
            if (_isNamingMode)
            {
                HandleNamingInput(keyState);
                return;
            }

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

            // Load texture (L or F key)
            if ((keyState.IsKeyDown(Keys.L) || keyState.IsKeyDown(Keys.F)) && !_previousKeyState.IsKeyDown(Keys.L) && !_previousKeyState.IsKeyDown(Keys.F))
                LoadTexture();

            // Save config (S key with Ctrl)
            if (keyState.IsKeyDown(Keys.LeftControl) && keyState.IsKeyDown(Keys.S) && !_previousKeyState.IsKeyDown(Keys.S))
                SaveConfig();

            // Name selected region (N key)
            if (keyState.IsKeyDown(Keys.N) && !_previousKeyState.IsKeyDown(Keys.N) && _selectedRegion.HasValue)
            {
                StartNaming();
            }

            // Delete named region (Delete key)
            if (keyState.IsKeyDown(Keys.Delete) && !_previousKeyState.IsKeyDown(Keys.Delete) && !string.IsNullOrEmpty(_currentRegionName))
            {
                DeleteNamedRegion();
            }

            // Mouse selection
            if (_loadedTexture != null && mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
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

                // Check if this region has a name
                _currentRegionName = "";
                foreach (var kvp in _namedRegions)
                {
                    if (kvp.Value.Equals(_selectedRegion.Value))
                    {
                        _currentRegionName = kvp.Key;
                        break;
                    }
                }
            }
        }

        private void StartNaming()
        {
            _isNamingMode = true;
            _inputBuffer = _currentRegionName; // Start with existing name if any
        }

        private void HandleNamingInput(KeyboardState keyState)
        {
            // Escape cancels naming
            if (keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape))
            {
                _isNamingMode = false;
                _inputBuffer = "";
                return;
            }

            // Enter confirms naming
            if (keyState.IsKeyDown(Keys.Enter) && !_previousKeyState.IsKeyDown(Keys.Enter))
            {
                if (!string.IsNullOrWhiteSpace(_inputBuffer) && _selectedRegion.HasValue)
                {
                    // Remove old entry if we're renaming
                    if (!string.IsNullOrEmpty(_currentRegionName) && _namedRegions.ContainsKey(_currentRegionName))
                    {
                        _namedRegions.Remove(_currentRegionName);
                    }

                    // Add the new name
                    _namedRegions[_inputBuffer.Trim()] = _selectedRegion.Value;
                    _currentRegionName = _inputBuffer.Trim();
                }
                _isNamingMode = false;
                _inputBuffer = "";
                return;
            }

            // Backspace removes last character
            if (keyState.IsKeyDown(Keys.Back) && !_previousKeyState.IsKeyDown(Keys.Back))
            {
                if (_inputBuffer.Length > 0)
                    _inputBuffer = _inputBuffer.Substring(0, _inputBuffer.Length - 1);
                return;
            }

            // Handle text input (letters, numbers, underscore)
            foreach (Keys key in keyState.GetPressedKeys())
            {
                if (_previousKeyState.IsKeyDown(key))
                    continue; // Already handled

                string character = GetCharFromKey(key, keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift));
                if (!string.IsNullOrEmpty(character))
                    _inputBuffer += character;
            }
        }

        private string GetCharFromKey(Keys key, bool shift)
        {
            // Handle letters
            if (key >= Keys.A && key <= Keys.Z)
            {
                char c = (char)('a' + (key - Keys.A));
                if (shift) c = char.ToUpper(c);
                return c.ToString();
            }

            // Handle numbers
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                if (shift)
                {
                    // Shifted number symbols
                    string[] symbols = { ")", "!", "@", "#", "$", "%", "^", "&", "*", "(" };
                    return symbols[key - Keys.D0];
                }
                return ((char)('0' + (key - Keys.D0))).ToString();
            }

            // Handle special characters
            switch (key)
            {
                case Keys.Space: return " ";
                case Keys.OemMinus: return shift ? "_" : "-";
                case Keys.OemPeriod: return shift ? ">" : ".";
                case Keys.OemComma: return shift ? "<" : ",";
                case Keys.OemQuestion: return shift ? "?" : "/";
                default: return null;
            }
        }

        private void DeleteNamedRegion()
        {
            if (_namedRegions.ContainsKey(_currentRegionName))
            {
                _namedRegions.Remove(_currentRegionName);
                _currentRegionName = "";
                Console.WriteLine($"Deleted named region");
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
            // Use a file dialog to select a PNG file
            using (var fileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                fileDialog.Filter = "PNG Images (*.png)|*.png|All Files (*.*)|*.*";
                fileDialog.Title = "Select Sprite Sheet";
                fileDialog.InitialDirectory = Directory.GetCurrentDirectory();

                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        using (var stream = File.OpenRead(fileDialog.FileName))
                        {
                            _loadedTexture = Texture2D.FromStream(GraphicsDevice, stream);
                            _loadedTexturePath = fileDialog.FileName;

                            // Clear previous named regions when loading new texture
                            _namedRegions.Clear();
                            _selectedRegion = null;
                            _currentRegionName = "";

                            // Try to load existing JSON config if it exists
                            LoadExistingConfig(fileDialog.FileName);

                            Console.WriteLine($"Loaded texture: {fileDialog.FileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading texture: {ex.Message}");
                    }
                }
            }
        }

        private void LoadExistingConfig(string texturePath)
        {
            string configPath = Path.ChangeExtension(texturePath, ".json");
            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var config = System.Text.Json.JsonSerializer.Deserialize<SpriteSheetConfig>(json, options);

                    if (config != null)
                    {
                        _spriteWidth = config.SpriteWidth;
                        _spriteHeight = config.SpriteHeight;
                        _spacing = config.Spacing;
                        _margin = config.Margin;

                        // Load named regions
                        _namedRegions.Clear();
                        foreach (var region in config.NamedRegions)
                        {
                            _namedRegions[region.Key] = new Rectangle(
                                region.Value.X,
                                region.Value.Y,
                                region.Value.Width,
                                region.Value.Height
                            );
                        }

                        Console.WriteLine($"Loaded existing config from: {configPath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading config: {ex.Message}");
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

                // Draw named regions
                DrawNamedRegions();

                // Draw selected region highlight
                if (_selectedRegion.HasValue)
                {
                    DrawRectangleOutline(_selectedRegion.Value, Color.Yellow, 2);
                }
            }

            _spriteBatch.End();

            // Draw UI overlays (text)
            _spriteBatch.Begin();
            DrawUI();
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

        private void DrawNamedRegions()
        {
            foreach (var kvp in _namedRegions)
            {
                // Draw outline for named regions in cyan
                DrawRectangleOutline(kvp.Value, Color.Cyan, 1);

                // Draw name label above the region
                Vector2 labelPos = WorldToScreen(new Vector2(kvp.Value.X, kvp.Value.Y - 15));
                DrawTextWithBackground(kvp.Key, labelPos, Color.Cyan, Color.Black);
            }
        }

        private void DrawUI()
        {
            // Draw help overlay
            if (_showHelp)
            {
                DrawHelpOverlay();
            }

            // Draw naming mode UI
            if (_isNamingMode)
            {
                DrawNamingUI();
            }

            // Draw current selection info
            if (_selectedRegion.HasValue && !_isNamingMode)
            {
                DrawSelectionInfo();
            }
        }

        private void DrawHelpOverlay()
        {
            int y = 10;
            int lineHeight = 20;
            Color bgColor = new Color(0, 0, 0, 180);
            Color textColor = Color.White;

            DrawFilledRectangle(new Rectangle(5, 5, 350, 280), bgColor);

            DrawText("SPRITE SHEET EDITOR CONTROLS", new Vector2(10, y), Color.Yellow);
            y += lineHeight * 2;

            DrawText("WASD: Pan camera", new Vector2(10, y), textColor); y += lineHeight;
            DrawText("Q/E: Zoom out/in", new Vector2(10, y), textColor); y += lineHeight;
            DrawText("Arrow Keys: Adjust grid size", new Vector2(10, y), textColor); y += lineHeight;
            DrawText("G: Toggle grid", new Vector2(10, y), textColor); y += lineHeight;
            DrawText("H: Toggle this help", new Vector2(10, y), textColor); y += lineHeight;
            y += lineHeight / 2;
            DrawText("Left Click: Select sprite tile", new Vector2(10, y), Color.Lime); y += lineHeight;
            DrawText("N: Name selected tile", new Vector2(10, y), Color.Lime); y += lineHeight;
            DrawText("Delete: Remove name from tile", new Vector2(10, y), Color.Lime); y += lineHeight;
            y += lineHeight / 2;
            DrawText("F or L: Load sprite sheet file", new Vector2(10, y), Color.Cyan); y += lineHeight;
            DrawText("Ctrl+S: Save JSON config", new Vector2(10, y), Color.Orange); y += lineHeight;
            DrawText("Esc: Exit", new Vector2(10, y), textColor);
        }

        private void DrawNamingUI()
        {
            int width = 400;
            int height = 100;
            int x = (_graphics.PreferredBackBufferWidth - width) / 2;
            int y = (_graphics.PreferredBackBufferHeight - height) / 2;

            DrawFilledRectangle(new Rectangle(x, y, width, height), new Color(0, 0, 0, 220));
            DrawRectangleOutlineScreen(new Rectangle(x, y, width, height), Color.Yellow, 2);

            DrawText("Enter sprite name:", new Vector2(x + 10, y + 10), Color.White);
            DrawText(_inputBuffer + "_", new Vector2(x + 10, y + 40), Color.Yellow);
            DrawText("Press Enter to confirm, Esc to cancel", new Vector2(x + 10, y + 70), Color.Gray);
        }

        private void DrawSelectionInfo()
        {
            string info = $"Selected: ({_selectedRegion.Value.X}, {_selectedRegion.Value.Y})";
            if (!string.IsNullOrEmpty(_currentRegionName))
                info += $" | Name: {_currentRegionName}";
            else
                info += " | Press N to name";

            DrawTextWithBackground(info, new Vector2(10, _graphics.PreferredBackBufferHeight - 30), Color.Yellow, new Color(0, 0, 0, 180));
        }

        private void DrawText(string text, Vector2 position, Color color)
        {
            if (string.IsNullOrEmpty(text)) return;

            int charsPerRow = 16;
            float x = position.X;

            // Draw shadow first for better readability
            foreach (char c in text)
            {
                int charCode = c - 32; // ASCII offset
                if (charCode < 0 || charCode >= 95)
                {
                    x += CHAR_WIDTH; // Skip unknown characters
                    continue;
                }

                int srcX = (charCode % charsPerRow) * CHAR_WIDTH;
                int srcY = (charCode / charsPerRow) * CHAR_HEIGHT;

                Rectangle sourceRect = new Rectangle(srcX, srcY, CHAR_WIDTH, CHAR_HEIGHT);

                // Draw shadow (offset by 1 pixel)
                _spriteBatch.Draw(_fontTexture, new Vector2(x + 1, position.Y + 1), sourceRect, Color.Black);

                x += CHAR_WIDTH;
            }

            // Draw text on top of shadow
            x = position.X;
            foreach (char c in text)
            {
                int charCode = c - 32; // ASCII offset
                if (charCode < 0 || charCode >= 95)
                {
                    x += CHAR_WIDTH; // Skip unknown characters
                    continue;
                }

                int srcX = (charCode % charsPerRow) * CHAR_WIDTH;
                int srcY = (charCode / charsPerRow) * CHAR_HEIGHT;

                Rectangle sourceRect = new Rectangle(srcX, srcY, CHAR_WIDTH, CHAR_HEIGHT);
                _spriteBatch.Draw(_fontTexture, new Vector2(x, position.Y), sourceRect, color);

                x += CHAR_WIDTH;
            }
        }

        private void DrawTextWithBackground(string text, Vector2 position, Color textColor, Color bgColor)
        {
            // Background box
            int padding = 4;
            int width = text.Length * 8 + padding * 2;
            int height = 16 + padding * 2;
            DrawFilledRectangle(new Rectangle((int)position.X - padding, (int)position.Y - padding, width, height), bgColor);
            DrawText(text, position, textColor);
        }

        private void DrawFilledRectangle(Rectangle rect, Color color)
        {
            var pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { color });
            _spriteBatch.Draw(pixel, rect, color);
        }

        private void DrawRectangleOutlineScreen(Rectangle rect, Color color, int thickness)
        {
            var pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { color });

            _spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            _spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            _spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            _spriteBatch.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
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
