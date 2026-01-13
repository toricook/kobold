namespace Kobold.Extensions.Animation.Components
{
    /// <summary>
    /// Automatically switches animations based on movement direction.
    /// Works with the Velocity component to determine which direction animation to play.
    /// </summary>
    public struct DirectionalAnimation
    {
        /// <summary>
        /// Animation name to play when velocity is near zero
        /// </summary>
        public string? IdleAnimation;

        /// <summary>
        /// Animation name to play when moving up
        /// </summary>
        public string? UpAnimation;

        /// <summary>
        /// Animation name to play when moving down
        /// </summary>
        public string? DownAnimation;

        /// <summary>
        /// Animation name to play when moving left
        /// </summary>
        public string? LeftAnimation;

        /// <summary>
        /// Animation name to play when moving right
        /// </summary>
        public string? RightAnimation;

        /// <summary>
        /// Animation name to play when moving up-left (8-way movement)
        /// </summary>
        public string? UpLeftAnimation;

        /// <summary>
        /// Animation name to play when moving up-right (8-way movement)
        /// </summary>
        public string? UpRightAnimation;

        /// <summary>
        /// Animation name to play when moving down-left (8-way movement)
        /// </summary>
        public string? DownLeftAnimation;

        /// <summary>
        /// Animation name to play when moving down-right (8-way movement)
        /// </summary>
        public string? DownRightAnimation;

        /// <summary>
        /// Whether to use 8-directional movement (true) or 4-directional (false)
        /// </summary>
        public bool UseEightDirections;

        /// <summary>
        /// Minimum speed (magnitude of velocity) required to trigger movement animations.
        /// Below this threshold, idle animation plays.
        /// </summary>
        public float MinimumSpeed;

        /// <summary>
        /// How to resolve diagonal movement to 4-way directions when UseEightDirections is false
        /// </summary>
        public DirectionPriority DirectionPriority;

        /// <summary>
        /// Create a 4-directional animation setup with common animations
        /// </summary>
        public static DirectionalAnimation FourWay(
            string? up = null,
            string? down = null,
            string? left = null,
            string? right = null,
            string? idle = null,
            float minimumSpeed = 10f,
            DirectionPriority priority = DirectionPriority.Horizontal)
        {
            return new DirectionalAnimation
            {
                UpAnimation = up,
                DownAnimation = down,
                LeftAnimation = left,
                RightAnimation = right,
                IdleAnimation = idle,
                UseEightDirections = false,
                MinimumSpeed = minimumSpeed,
                DirectionPriority = priority
            };
        }

        /// <summary>
        /// Create an 8-directional animation setup with all directions
        /// </summary>
        public static DirectionalAnimation EightWay(
            string? up = null,
            string? down = null,
            string? left = null,
            string? right = null,
            string? upLeft = null,
            string? upRight = null,
            string? downLeft = null,
            string? downRight = null,
            string? idle = null,
            float minimumSpeed = 10f)
        {
            return new DirectionalAnimation
            {
                UpAnimation = up,
                DownAnimation = down,
                LeftAnimation = left,
                RightAnimation = right,
                UpLeftAnimation = upLeft,
                UpRightAnimation = upRight,
                DownLeftAnimation = downLeft,
                DownRightAnimation = downRight,
                IdleAnimation = idle,
                UseEightDirections = true,
                MinimumSpeed = minimumSpeed
            };
        }

        /// <summary>
        /// Create a typical top-down game animation setup (down = front, up = back)
        /// </summary>
        public static DirectionalAnimation TopDown(
            string? front = null,
            string? back = null,
            string? left = null,
            string? right = null,
            string? idle = null,
            float minimumSpeed = 10f)
        {
            return FourWay(
                up: back,
                down: front,
                left: left,
                right: right,
                idle: idle,
                minimumSpeed: minimumSpeed
            );
        }
    }

    /// <summary>
    /// Determines how diagonal movement is resolved to 4-way directions
    /// </summary>
    public enum DirectionPriority
    {
        /// <summary>
        /// Prefer horizontal (left/right) over vertical (up/down) when moving diagonally
        /// </summary>
        Horizontal,

        /// <summary>
        /// Prefer vertical (up/down) over horizontal (left/right) when moving diagonally
        /// </summary>
        Vertical
    }
}
