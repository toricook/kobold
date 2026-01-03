using Arch.Core;
using Arch.Core.Extensions;
using CaveExplorer.Components;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Extensions.Pickups;

namespace CaveExplorer.Systems
{
    /// <summary>
    /// Updates the coin counter UI display when coins are collected.
    /// Listens to ItemCollectedEvent and updates all entities with CoinCounterUI tag
    /// to show the current coin count from the player's inventory.
    /// </summary>
    public class CoinDisplaySystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;

        public CoinDisplaySystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;

            // Subscribe to item collection events to update the display
            _eventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
        }

        public void Update(float deltaTime)
        {
            // Update display every frame to ensure it's always current
            UpdateCoinDisplay();
        }

        private void OnItemCollected(ItemCollectedEvent evt)
        {
            // Update the coin display when any item is collected
            UpdateCoinDisplay();
        }

        private void UpdateCoinDisplay()
        {
            // Find the player and get their coin count
            var playerQuery = new QueryDescription().WithAll<Player, PlayerInventory>();
            int coinCount = 0;

            _world.Query(in playerQuery, (ref PlayerInventory inventory) =>
            {
                coinCount = inventory.Coins;
            });

            // Update all coin counter UI entities
            var uiQuery = new QueryDescription().WithAll<CoinCounterUI, TextRenderer>();

            _world.Query(in uiQuery, (ref TextRenderer textRenderer) =>
            {
                textRenderer.Text = $"Coins: {coinCount}";
            });
        }
    }
}
