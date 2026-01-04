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
    /// Tag component marking a UI entity as the coin counter display.
    /// The CoinDisplaySystem will update entities with this tag to show the current coin count.
    /// </summary>
    public struct CoinCounterUI
    {
    }
}
