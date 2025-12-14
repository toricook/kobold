using Kobold.Core.Abstractions.Input;
using Microsoft.Xna.Framework.Input;
using System.Numerics;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;

namespace Kobold.Monogame
{
    public class MonoGameInputManager : IInputManager
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;

        private readonly Dictionary<KeyCode, XnaKeys> _keyMapping = new()
        {
            { KeyCode.W, XnaKeys.W },
            { KeyCode.A, XnaKeys.A },
            { KeyCode.S, XnaKeys.S },
            { KeyCode.D, XnaKeys.D },
            { KeyCode.Space, XnaKeys.Space },
            { KeyCode.Enter, XnaKeys.Enter },
            { KeyCode.Escape, XnaKeys.Escape },
            { KeyCode.Up, XnaKeys.Up },
            { KeyCode.Down, XnaKeys.Down },
            { KeyCode.Left, XnaKeys.Left },
            { KeyCode.Right, XnaKeys.Right },
            { KeyCode.P, XnaKeys.P },
        };

        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _previousMouseState = _currentMouseState;

            _currentKeyboardState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();
        }

        public bool IsKeyPressed(KeyCode key)
        {
            if (!_keyMapping.TryGetValue(key, out var xnaKey)) return false;
            return _currentKeyboardState.IsKeyDown(xnaKey) && !_previousKeyboardState.IsKeyDown(xnaKey);
        }

        public bool IsKeyDown(KeyCode key)
        {
            if (!_keyMapping.TryGetValue(key, out var xnaKey)) return false;
            return _currentKeyboardState.IsKeyDown(xnaKey);
        }

        public bool IsKeyUp(KeyCode key)
        {
            if (!_keyMapping.TryGetValue(key, out var xnaKey)) return false;
            return _currentKeyboardState.IsKeyUp(xnaKey);
        }

        public Vector2 GetMousePosition()
        {
            return new Vector2(_currentMouseState.X, _currentMouseState.Y);
        }

        public bool IsMouseButtonPressed(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released,
                MouseButton.Right => _currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released,
                MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Pressed && _previousMouseState.MiddleButton == ButtonState.Released,
                _ => false
            };
        }
    }
}