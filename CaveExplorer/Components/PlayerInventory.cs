namespace CaveExplorer.Components
{
    /// <summary>
    /// Component tracking player's collected items and resources.
    /// Attach to the player entity.
    /// </summary>
    public struct PlayerInventory
    {
        /// <summary>
        /// Total coins collected by the player
        /// </summary>
        public int Coins { get; set; }

        public PlayerInventory()
        {
            Coins = 0;
        }
    }
}
