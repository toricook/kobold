using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Assets;
using Kobold.Core.Components;
using Kobold.Extensions.Gameplay.Components;
using Kobold.Core.Events;
using Kobold.Extensions.Portals;
using Kobold.Extensions.Tilemaps;
using System.Numerics;

namespace Kobold.Extensions.Progression
{
    /// <summary>
    /// Indicates which stair the player should spawn at when restoring a level.
    /// </summary>
    internal enum SpawnLocation
    {
        /// <summary>Spawn at stairUp (player came from above, descending)</summary>
        StairUp,
        /// <summary>Spawn at stairDown (player came from below, ascending)</summary>
        StairDown
    }

    /// <summary>
    /// Manages level progression, snapshots, and transitions in a generation-agnostic way.
    /// Publishes GenerateNewLevelEvent for games to handle their own generation,
    /// then subscribes to LevelReadyEvent to create stairs and spawn player.
    /// </summary>
    public class LevelProgressionSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private Entity? _gameStateEntity;
        private Entity? _playerEntity;
        private SpriteSheet? _spriteSheet;

        public LevelProgressionSystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;

            // Subscribe to portal requests and level ready events
            _eventBus.Subscribe<LevelGenerationRequestEvent>(OnLevelGenerationRequest);
            _eventBus.Subscribe<LevelReadyEvent>(OnLevelReady);
        }

        /// <summary>
        /// Called by PortalSystem when player activates a stair portal.
        /// Determines direction and initiates level transition.
        /// </summary>
        private void OnLevelGenerationRequest(LevelGenerationRequestEvent evt)
        {
            // Find game state entity with ProgressionState
            if (!FindGameStateEntity())
            {
                Console.WriteLine("LevelProgressionSystem: No ProgressionState found");
                return;
            }

            // Find player entity
            if (!FindPlayerEntity())
            {
                Console.WriteLine("LevelProgressionSystem: No player entity found");
                return;
            }

            ref var state = ref _gameStateEntity.Value.Get<ProgressionState>();

            // Parse direction from levelId
            if (evt.LevelId == "depth_down")
            {
                HandleDescend(ref state, _playerEntity.Value);
            }
            else if (evt.LevelId == "depth_up")
            {
                HandleAscend(ref state, _playerEntity.Value);
            }
        }

        /// <summary>
        /// Called by game code when level generation is complete.
        /// Creates stairs and spawns player at appropriate position.
        /// </summary>
        private void OnLevelReady(LevelReadyEvent evt)
        {
            // Find game state and player if not already cached
            if (!FindGameStateEntity() || !FindPlayerEntity())
            {
                Console.WriteLine("LevelProgressionSystem: Missing game state or player entity");
                return;
            }

            ref var state = ref _gameStateEntity.Value.Get<ProgressionState>();

            // Validate depth matches
            if (evt.Depth != state.CurrentDepth)
            {
                Console.WriteLine($"LevelProgressionSystem: Depth mismatch. Expected {state.CurrentDepth}, got {evt.Depth}");
                return;
            }

            // Create stairs and spawn player
            if (evt.TileMap != null)
            {
                CreateStairsAndSpawnPlayer(evt.TileMap, evt.Depth, _playerEntity.Value);
            }
            else
            {
                // No tilemap (e.g., Asteroids) - spawn at origin
                ref var transform = ref _playerEntity.Value.Get<Transform>();
                transform.Position = Vector2.Zero;
            }
        }

        /// <summary>
        /// Handles descending to next level (depth + 1).
        /// Saves current level, clears entities, and publishes GenerateNewLevelEvent.
        /// </summary>
        private void HandleDescend(ref ProgressionState state, Entity player)
        {
            // Check if at max depth
            if (state.CurrentDepth >= state.MaxDepth - 1)
            {
                Console.WriteLine($"LevelProgressionSystem: Already at max depth {state.MaxDepth - 1}");
                return;
            }

            // Save current level snapshot
            SaveCurrentLevel(ref state);

            // Push current depth to history
            state.DepthHistory.Push(state.CurrentDepth);

            // Increment depth
            state.CurrentDepth++;

            // Clear current level entities
            ClearCurrentLevel();

            // Check if we've visited this depth before
            if (state.LevelSnapshots.ContainsKey(state.CurrentDepth))
            {
                // Restore from snapshot - spawn at stairUp (coming from above)
                var snapshot = state.LevelSnapshots[state.CurrentDepth];
                RestoreLevel(snapshot, player, SpawnLocation.StairUp);
            }
            else
            {
                // Publish event for game to generate new level
                _eventBus.Publish(new GenerateNewLevelEvent(state.CurrentDepth, isRevisit: false));
            }
        }

        /// <summary>
        /// Handles ascending to previous level (depth - 1).
        /// Clears entities and restores previous level from snapshot.
        /// </summary>
        private void HandleAscend(ref ProgressionState state, Entity player)
        {
            // Check if at surface
            if (state.CurrentDepth <= 0)
            {
                Console.WriteLine("LevelProgressionSystem: Already at surface level");
                return;
            }

            // Pop previous depth from history
            if (state.DepthHistory.Count == 0)
            {
                Console.WriteLine("LevelProgressionSystem: No depth history to ascend");
                return;
            }

            int previousDepth = state.DepthHistory.Pop();

            // Clear current level
            ClearCurrentLevel();

            // Restore previous level from snapshot
            if (state.LevelSnapshots.ContainsKey(previousDepth))
            {
                var snapshot = state.LevelSnapshots[previousDepth];
                state.CurrentDepth = previousDepth;
                // Spawn at stairDown (coming from below, ascending)
                RestoreLevel(snapshot, player, SpawnLocation.StairDown);
            }
            else
            {
                Console.WriteLine($"LevelProgressionSystem: No snapshot found for depth {previousDepth}");
                state.CurrentDepth = previousDepth;
                _eventBus.Publish(new GenerateNewLevelEvent(previousDepth, isRevisit: true));
            }
        }

        /// <summary>
        /// Saves current level state to snapshot dictionary.
        /// Captures tilemap, camera, and stair positions.
        /// </summary>
        private void SaveCurrentLevel(ref ProgressionState state)
        {
            var snapshot = new LevelStateSnapshot
            {
                Depth = state.CurrentDepth,
                LevelId = $"depth_{state.CurrentDepth}"
            };

            // Find tilemap entity
            var tilemapQuery = new QueryDescription().WithAll<TileMapComponent>();
            _world.Query(in tilemapQuery, (Entity entity, ref TileMapComponent tilemap) =>
            {
                snapshot.TileMap = tilemap.TileMap.Clone();
                snapshot.TileSet = tilemap.TileSet; // TileSet is immutable, can share reference
            });

            // Find camera entity
            var cameraQuery = new QueryDescription().WithAll<Camera>();
            _world.Query(in cameraQuery, (Entity entity, ref Camera camera) =>
            {
                snapshot.CameraWidth = camera.ViewportWidth;
                snapshot.CameraHeight = camera.ViewportHeight;
            });

            // Find stair positions
            var stairQuery = new QueryDescription().WithAll<StairComponent, Transform>();
            _world.Query(in stairQuery, (Entity entity, ref StairComponent stair, ref Transform transform) =>
            {
                if (stair.Direction == StairDirection.Up)
                {
                    snapshot.StairUpPosition = transform.Position;
                }
                else if (stair.Direction == StairDirection.Down)
                {
                    snapshot.StairDownPosition = transform.Position;
                }
            });

            // Store snapshot
            state.LevelSnapshots[state.CurrentDepth] = snapshot;
            Console.WriteLine($"LevelProgressionSystem: Saved snapshot for depth {state.CurrentDepth}");
        }

        /// <summary>
        /// Clears all entities except player and game state.
        /// Destroys tilemap, camera, stairs, and other level entities.
        /// </summary>
        private void ClearCurrentLevel()
        {
            var entitiesToDestroy = new List<Entity>();

            // Collect all entities except player and game state
            var allEntitiesQuery = new QueryDescription();
            _world.Query(in allEntitiesQuery, (Entity entity) =>
            {
                // Don't destroy player
                if (entity.Has<Player>())
                    return;

                // Don't destroy game state entities
                if (entity.Has<ProgressionState>())
                    return;

                entitiesToDestroy.Add(entity);
            });

            // Destroy collected entities
            foreach (var entity in entitiesToDestroy)
            {
                _world.Destroy(entity);
            }

            Console.WriteLine($"LevelProgressionSystem: Cleared {entitiesToDestroy.Count} entities");
        }

        /// <summary>
        /// Restores level from snapshot.
        /// Recreates tilemap, camera, stairs, and spawns player.
        /// </summary>
        private void RestoreLevel(LevelStateSnapshot snapshot, Entity player, SpawnLocation spawnLocation)
        {
            Console.WriteLine($"LevelProgressionSystem: Restoring level at depth {snapshot.Depth}");

            // Restore tilemap
            if (snapshot.TileMap != null && snapshot.TileSet != null)
            {
                // Find sprite sheet from player's SpriteRenderer or search for it
                if (_spriteSheet == null)
                {
                    FindSpriteSheet();
                }

                if (_spriteSheet != null)
                {
                    _world.Create(new TileMapComponent(
                        snapshot.TileMap,
                        snapshot.TileSet,
                        _spriteSheet.Texture,
                        _spriteSheet
                    ));
                }
            }

            // Restore camera
            var camera = new Camera(snapshot.CameraWidth, snapshot.CameraHeight, smoothSpeed: 5f);
            if (snapshot.TileMap != null)
            {
                camera.SetBounds(
                    snapshot.TileMap.Width * snapshot.TileMap.TileWidth,
                    snapshot.TileMap.Height * snapshot.TileMap.TileHeight
                );
            }
            _world.Create(camera);

            // Recreate stairs and spawn player
            CreateStairsFromSnapshot(snapshot, player, spawnLocation);
        }

        /// <summary>
        /// Creates stairs at positions stored in snapshot and spawns player.
        /// </summary>
        private void CreateStairsFromSnapshot(LevelStateSnapshot snapshot, Entity player, SpawnLocation spawnLocation)
        {
            if (_spriteSheet == null)
            {
                Console.WriteLine("LevelProgressionSystem: No sprite sheet available for stairs");
                return;
            }

            // Create stairUp (if not at surface)
            if (snapshot.Depth > 0)
            {
                StairFactory.CreateStairUp(_world, snapshot.StairUpPosition, snapshot.Depth, _spriteSheet);
            }

            // Create stairDown (if not at max depth)
            if (!_gameStateEntity.HasValue)
                return;

            ref var state = ref _gameStateEntity.Value.Get<ProgressionState>();
            if (snapshot.Depth < state.MaxDepth - 1)
            {
                StairFactory.CreateStairDown(_world, snapshot.StairDownPosition, snapshot.Depth, _spriteSheet);
            }

            // Spawn player at appropriate stair based on direction
            ref var transform = ref player.Get<Transform>();
            if (spawnLocation == SpawnLocation.StairUp)
            {
                // Coming from above (descending) - spawn at stairUp
                transform.Position = snapshot.StairUpPosition;
            }
            else
            {
                // Coming from below (ascending) - spawn at stairDown
                transform.Position = snapshot.StairDownPosition;
            }
        }

        /// <summary>
        /// Creates stairs on newly generated level and spawns player.
        /// Called after LevelReadyEvent is received.
        /// </summary>
        private void CreateStairsAndSpawnPlayer(TileMap tileMap, int depth, Entity player)
        {
            if (_spriteSheet == null)
            {
                FindSpriteSheet();
            }

            if (_spriteSheet == null)
            {
                Console.WriteLine("LevelProgressionSystem: No sprite sheet available for stairs");
                return;
            }

            // Find positions for stairs
            var (stairUpPos, stairDownPos) = StairFactory.FindStairPositions(tileMap);

            // Create stairUp (if not at surface)
            if (depth > 0)
            {
                StairFactory.CreateStairUp(_world, stairUpPos, depth, _spriteSheet);
            }

            // Create stairDown (if not at max depth)
            if (!_gameStateEntity.HasValue)
                return;

            ref var state = ref _gameStateEntity.Value.Get<ProgressionState>();
            if (depth < state.MaxDepth - 1)
            {
                StairFactory.CreateStairDown(_world, stairDownPos, depth, _spriteSheet);
            }

            // Spawn player at appropriate position
            ref var transform = ref player.Get<Transform>();

            // If we descended, spawn at stairUp (where we came from)
            // If we're at depth 0 (initial), spawn at stairUp position
            if (depth > 0 && state.DepthHistory.Count > 0)
            {
                // Coming from above - spawn at stairUp
                transform.Position = stairUpPos;
            }
            else
            {
                // First time or special case - spawn at stairUp
                transform.Position = stairUpPos;
            }

            Console.WriteLine($"LevelProgressionSystem: Created stairs at depth {depth}");
        }

        /// <summary>
        /// Finds game state entity with ProgressionState component.
        /// </summary>
        private bool FindGameStateEntity()
        {
            if (_gameStateEntity.HasValue)
                return true;

            var query = new QueryDescription().WithAll<ProgressionState>();
            _world.Query(in query, (Entity entity) =>
            {
                _gameStateEntity = entity;
            });

            return _gameStateEntity.HasValue;
        }

        /// <summary>
        /// Finds player entity.
        /// </summary>
        private bool FindPlayerEntity()
        {
            if (_playerEntity.HasValue)
                return true;

            var query = new QueryDescription().WithAll<Player>();
            _world.Query(in query, (Entity entity) =>
            {
                _playerEntity = entity;
            });

            return _playerEntity.HasValue;
        }

        /// <summary>
        /// Finds sprite sheet by searching for TileMapComponent.
        /// </summary>
        private void FindSpriteSheet()
        {
            var query = new QueryDescription().WithAll<TileMapComponent>();
            _world.Query(in query, (Entity entity, ref TileMapComponent tilemap) =>
            {
                _spriteSheet = tilemap.SpriteSheet;
            });
        }

        public void Update(float deltaTime)
        {
            // This system is event-driven, no per-frame updates needed
        }
    }
}
