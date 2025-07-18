using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions
{
    public interface IInputManager
    {
        bool IsKeyPressed(KeyCode key);
        bool IsKeyDown(KeyCode key);
        bool IsKeyUp(KeyCode key);
        Vector2 GetMousePosition();
        bool IsMouseButtonPressed(MouseButton button);
    }

    public enum KeyCode
    {
        W, A, S, D, Space, Enter, Escape,
        Up, Down, Left, Right,
        P
        // Add more as needed
    }

    public enum MouseButton
    {
        Left, Right, Middle
    }
}
