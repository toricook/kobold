using Kobold.Core.Abstractions.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Extensions.Input.Components
{
    /// <summary>
    /// Component for entities controlled by player input
    /// </summary>
    public struct PlayerControlled
    {
        public float Speed;
        public bool VerticalOnly;
        public bool HorizontalOnly;
        public KeyCode UpKey;
        public KeyCode DownKey;
        public KeyCode LeftKey;
        public KeyCode RightKey;
        public KeyCode AlternateUpKey;
        public KeyCode AlternateDownKey;
        public KeyCode AlternateLeftKey;
        public KeyCode AlternateRightKey;

        public PlayerControlled(float speed, bool verticalOnly = false, bool horizontalOnly = false,
            KeyCode up = KeyCode.Up, KeyCode down = KeyCode.Down,
            KeyCode left = KeyCode.Left, KeyCode right = KeyCode.Right,
            KeyCode altUp = KeyCode.W, KeyCode altDown = KeyCode.S,
            KeyCode altLeft = KeyCode.A, KeyCode altRight = KeyCode.D)
        {
            Speed = speed;
            VerticalOnly = verticalOnly;
            HorizontalOnly = horizontalOnly;
            UpKey = up;
            DownKey = down;
            LeftKey = left;
            RightKey = right;
            AlternateUpKey = altUp;
            AlternateDownKey = altDown;
            AlternateLeftKey = altLeft;
            AlternateRightKey = altRight;
        }

        public static PlayerControlled CreateVerticalOnly(float speed, KeyCode up = KeyCode.Up, KeyCode down = KeyCode.Down,
            KeyCode altUp = KeyCode.W, KeyCode altDown = KeyCode.S)
        {
            return new PlayerControlled(speed, verticalOnly: true, horizontalOnly: false,
                up, down, KeyCode.Left, KeyCode.Right, altUp, altDown, KeyCode.A, KeyCode.D);
        }

        public static PlayerControlled CreateHorizontalOnly(float speed, KeyCode left = KeyCode.Left, KeyCode right = KeyCode.Right,
            KeyCode altLeft = KeyCode.A, KeyCode altRight = KeyCode.D)
        {
            return new PlayerControlled(speed, verticalOnly: false, horizontalOnly: true,
                KeyCode.Up, KeyCode.Down, left, right, KeyCode.W, KeyCode.S, altLeft, altRight);
        }

        public static PlayerControlled FullMovement(float speed)
        {
            return new PlayerControlled(speed, verticalOnly: false, horizontalOnly: false);
        }
    }
}
