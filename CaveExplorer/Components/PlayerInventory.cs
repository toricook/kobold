using Kobold.Extensions.Physics.Systems;
using Kobold.Extensions.Physics.Components;
using Kobold.Extensions.Collision.Systems;
using Kobold.Extensions.Collision.Components;
using Kobold.Extensions.Input.Systems;
using Kobold.Extensions.Input.Components;
using Kobold.Extensions.Boundaries.Systems;
using Kobold.Extensions.Boundaries.Components;
using Kobold.Extensions.Triggers.Systems;
using Kobold.Extensions.Destruction.Systems;
using Kobold.Extensions.Destruction.Components;
using Kobold.Extensions.Gameplay.Components;
using Kobold.Extensions.GameState.Systems;
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
