namespace Kobold.Extensions.Progression
{
    /// <summary>
    /// Direction a stair leads to
    /// </summary>
    public enum StairDirection
    {
        /// <summary>Leads to previous level (depth - 1)</summary>
        Up,

        /// <summary>Leads to next level (depth + 1)</summary>
        Down
    }

    /// <summary>
    /// Tag component for stair entities.
    /// Helps identify stairs during entity cleanup and validation.
    /// </summary>
    public struct StairComponent
    {
        /// <summary>
        /// Direction this stair leads (Up or Down)
        /// </summary>
        public StairDirection Direction { get; set; }

        /// <summary>
        /// Target depth this stair leads to
        /// </summary>
        public int TargetDepth { get; set; }

        public StairComponent(StairDirection direction, int targetDepth)
        {
            Direction = direction;
            TargetDepth = targetDepth;
        }
    }
}
