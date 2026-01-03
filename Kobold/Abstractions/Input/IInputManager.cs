using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions.Input
{
    /// <summary>
    /// The input manager is responsible for getting input from the hardware
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// Check if a key was just pressed this frame (was up last frame, down this frame)
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key was just pressed</returns>
        bool IsKeyPressed(KeyCode key);

        /// <summary>
        /// Check if a key is currently being held down
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key is down</returns>
        bool IsKeyDown(KeyCode key);

        /// <summary>
        /// Check if a key was just released this frame (was down last frame, up this frame)
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key was just released</returns>
        bool IsKeyUp(KeyCode key);

        /// <summary>
        /// Get the current mouse cursor position in screen coordinates
        /// </summary>
        /// <returns>Mouse position as a 2D vector</returns>
        Vector2 GetMousePosition();

        /// <summary>
        /// Check if a mouse button was just pressed this frame
        /// </summary>
        /// <param name="button">The mouse button to check</param>
        /// <returns>True if the button was just pressed</returns>
        bool IsMouseButtonPressed(MouseButton button);
    }

    /// <summary>
    /// Keyboard keys supported by the input system
    /// </summary>
    public enum KeyCode
    {
        /// <summary>W key</summary>
        W,
        /// <summary>A key</summary>
        A,
        /// <summary>S key</summary>
        S,
        /// <summary>D key</summary>
        D,
        /// <summary>Space bar</summary>
        Space,
        /// <summary>Enter/Return key</summary>
        Enter,
        /// <summary>Escape key</summary>
        Escape,
        /// <summary>Up arrow key</summary>
        Up,
        /// <summary>Down arrow key</summary>
        Down,
        /// <summary>Left arrow key</summary>
        Left,
        /// <summary>Right arrow key</summary>
        Right,
        /// <summary>P key</summary>
        P,
        /// <summary>E key</summary>
        E
        // Add more as needed
    }

    /// <summary>
    /// Mouse buttons supported by the input system
    /// </summary>
    public enum MouseButton
    {
        /// <summary>Left mouse button</summary>
        Left,
        /// <summary>Right mouse button</summary>
        Right,
        /// <summary>Middle mouse button (scroll wheel click)</summary>
        Middle
    }
}
