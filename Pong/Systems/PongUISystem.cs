using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Systems
{
    /// Simple Pong-specific UI system that listens for game state events
    /// and directly manages UI entities
    /// </summary>
    public class PongUISystem : ISystem, IEventHandler<GameStateChangedEvent<CoreGameState>>
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly List<Entity> _uiEntities = new();

        public PongUISystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;

            // Subscribe to state change events
            _eventBus.Subscribe<GameStateChangedEvent<CoreGameState>>(this);

        }

        public void Update(float deltaTime)
        {
            // Could add UI animations here if needed
        }

        public void Handle(GameStateChangedEvent<CoreGameState> eventData)
        {
            // Clear existing UI
            ClearUI();

            // Create new UI based on the state
            switch (eventData.NewState.State)
            {
                case StandardGameState.GameOver:
                    CreateGameOverUI(eventData.NewState);
                    break;

                case StandardGameState.Playing:
                    // No extra UI needed for playing
                    break;

                case StandardGameState.Paused:
                    CreatePausedUI();
                    break;
            }
        }

        private void CreateGameOverUI(CoreGameState gameState)
        {

            // Game Over title - UI layer
            var titleEntity = _world.Create(
                new Transform(new Vector2(250f, 250f)),
                TextRenderer.UIText("GAME OVER!", Color.Red, 32f)
            );
            _uiEntities.Add(titleEntity);

            // Winner message - UI layer
            if (!string.IsNullOrEmpty(gameState.Message))
            {
                var messageEntity = _world.Create(
                    new Transform(new Vector2(200f, 290f)),
                    TextRenderer.UIText(gameState.Message, Color.Yellow, 20f)
                );
                _uiEntities.Add(messageEntity);
            }

            // Restart instruction - UI layer
            var instructionEntity = _world.Create(
                new Transform(new Vector2(250f, 350f)),
                TextRenderer.UIText("Press SPACE to restart", Color.Gray, 16f)
            );
            _uiEntities.Add(instructionEntity);

        }

        private void CreatePausedUI()
        {

            // Paused title - UI layer
            var pausedEntity = _world.Create(
                new Transform(new Vector2(350f, 300f)),
                TextRenderer.UIText("PAUSED", Color.White, 24f)
            );
            _uiEntities.Add(pausedEntity);

            // Resume instruction - UI layer
            var resumeEntity = _world.Create(
                new Transform(new Vector2(280f, 340f)),
                TextRenderer.UIText("Press SPACE to resume", Color.Gray, 16f)
            );
            _uiEntities.Add(resumeEntity);

        }

        private void ClearUI()
        {

            foreach (var entity in _uiEntities)
            {
                if (_world.IsAlive(entity))
                {
                    _world.Destroy(entity);
                }
            }

            _uiEntities.Clear();
        }


    }
}
