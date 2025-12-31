using Arch.Core;
using CaveExplorer.Components;
using CaveExplorer.Components.PickupEffects;
using CaveExplorer.Systems;
using Kobold.Core;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Assets;
using Kobold.Core.Components;
using Kobold.Core.Components.Gameplay;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Kobold.Extensions.Pickups;
using Kobold.Extensions.Portals;
using Kobold.Extensions.Progression;
using Kobold.Extensions.SaveSystem;
using Kobold.Extensions.Tilemaps;
using Kobold.Extensions.Procedural;
using Kobold.Extensions.UI.Components;
using Kobold.Extensions.UI.Systems;
using System.Drawing;
using System.Numerics;

namespace CaveExplorer
{
    public class CaveExplorerGame : GameEngineBase
    {
        // Entity references
        private Entity _gameStateEntity;
        private SpriteSheet _spriteSheet;

        // Save system
        private SaveManager _saveManager;

        public CaveExplorerGame() : base()
        {
        }

        public CaveExplorerGame(IRenderer renderer, IInputManager inputManager)
            : base(renderer, inputManager)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            // Create game state entity
            CreateGameState();

            // Initialize save system
            InitializeSaveSystem();

            // Initialize and register systems
            InitializeSystems();

            // Subscribe to level generation events
            EventBus.Subscribe<GenerateNewLevelEvent>(OnGenerateNewLevel);

            // Create initial game entities
            CreateInitialEntities();
        }

        private void InitializeSaveSystem()
        {
            _saveManager = new SaveManager(World, EventBus, "CaveExplorer");

            // Register component serializers for custom components
            // The save system already handles core components (Transform, Velocity, etc.)
            // Register any CaveExplorer-specific components here if needed
        }

        private void InitializeSystems()
        {
            // Configure physics (top-down game, no gravity)
            var physicsConfig = new PhysicsConfig
            {
                EnableGravity = false,
                EnableDamping = true,
                EnableRotationalPhysics = false,
                EnableThrust = false,
                EnableSpeedLimits = true,
                DefaultLinearDamping = 5.0f,
                DefaultAngularDamping = 0.1f,
                UseMultiplicativeDamping = true
            };

            // Boundary system not needed - we use camera bounds and tile collision instead
            // (Map is larger than screen, so screen boundaries don't apply)

            // Configure collision system
            var collisionConfig = new CollisionConfig
            {
                EnableCollisionResponse = true
            };

            // Create UI systems (run always, not just during gameplay)
            var uiInputSystem = new UIInputSystem(World, InputManager, EventBus);
            var uiButtonSystem = new UIButtonSystem(World, EventBus);
            var uiAnchorSystem = new UIAnchorSystem(World, new Vector2(Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT));
            var menuSystem = new MenuSystem(World, EventBus, InputManager, _saveManager, _gameStateEntity);
            var coinDisplaySystem = new CoinDisplaySystem(World, EventBus);

            // Create gameplay systems
            var inputSystem = new InputSystem(InputManager, World);
            var cameraSystem = new CameraSystem(World);
            var tileMapCollisionSystem = new TileMapCollisionSystem(World);
            var physicsSystem = new PhysicsSystem(World, physicsConfig);
            var collisionSystem = new CollisionSystem(World, EventBus, collisionConfig);
            var triggerSystem = new TriggerSystem(World, EventBus, InputManager, KeyCode.E);
            var pickupSystem = new Kobold.Extensions.Pickups.PickupSystem(World, EventBus);
            var portalSystem = new PortalSystem(World, EventBus);
            var progressionSystem = new LevelProgressionSystem(World, EventBus);
            var destructionSystem = new DestructionSystem(World, EventBus);
            var renderSystem = new RenderSystem(Renderer, World);
            var tileMapRenderSystem = new TileMapRenderSystem(Renderer, World);

            // Register UI systems (always run, even in menu)
            SystemManager.AddSystem(uiInputSystem, SystemUpdateOrder.INPUT - 10, requiresGameplayState: false);
            SystemManager.AddSystem(uiButtonSystem, SystemUpdateOrder.UI, requiresGameplayState: false);
            SystemManager.AddSystem(uiAnchorSystem, SystemUpdateOrder.UI + 1, requiresGameplayState: false);
            SystemManager.AddSystem(menuSystem, SystemUpdateOrder.UI + 2, requiresGameplayState: false);
            SystemManager.AddSystem(coinDisplaySystem, SystemUpdateOrder.UI + 3, requiresGameplayState: true); // Update coin display during gameplay

            // Register gameplay systems (only run during gameplay)
            SystemManager.AddSystem(inputSystem, SystemUpdateOrder.INPUT, requiresGameplayState: true);
            SystemManager.AddSystem(cameraSystem, SystemUpdateOrder.INPUT + 50, requiresGameplayState: true); // Update camera after input
            SystemManager.AddSystem(tileMapCollisionSystem, SystemUpdateOrder.PHYSICS - 1, requiresGameplayState: true);
            SystemManager.AddSystem(physicsSystem, SystemUpdateOrder.PHYSICS, requiresGameplayState: true);
            SystemManager.AddSystem(collisionSystem, SystemUpdateOrder.COLLISION, requiresGameplayState: true);
            SystemManager.AddSystem(triggerSystem, SystemUpdateOrder.COLLISION + 1, requiresGameplayState: true);
            SystemManager.AddSystem(pickupSystem, SystemUpdateOrder.COLLISION + 2, requiresGameplayState: true);
            SystemManager.AddSystem(portalSystem, SystemUpdateOrder.COLLISION + 3, requiresGameplayState: true);
            SystemManager.AddSystem(progressionSystem, SystemUpdateOrder.COLLISION + 4, requiresGameplayState: true);
            SystemManager.AddSystem(destructionSystem, SystemUpdateOrder.CLEANUP, requiresGameplayState: true);

            // Render systems always run (now using camera-aware core RenderSystem)
            SystemManager.AddRenderSystem(tileMapRenderSystem);
            SystemManager.AddRenderSystem(renderSystem);
        }

        private void CreateGameState()
        {
            _gameStateEntity = World.Create(
                new CoreGameState(StandardGameState.Menu), // Start in menu instead of playing
                new ProgressionState()
            );
        }

        private void CreateInitialEntities()
        {
            // Load sprite sheet
            _spriteSheet = Assets.LoadSpriteSheet("sprites");

            // Generate initial cave (depth 0)
            var config = new CellularAutomataConfig
            {
                Width = 64,
                Height = 48,
                TileWidth = 32,
                TileHeight = 32,
                Iterations = 5,
                InitialWallProbability = 0.45f,
                ConnectCaves = true,
                MinCaveSize = 60,
                WallTileId = 1,
                FloorTileId = 0
            };

            var generator = new CellularAutomataGenerator(config);
            var (tileMap, tileSet) = generator.GenerateWithTileSet();

            // Create tilemap entity
            CreateTileMapEntity(tileMap, tileSet);

            // Find valid spawn position for player
            var spawnPosition = FindValidSpawnPosition(tileMap);

            // Create camera entity at player's spawn position
            CreateCamera(tileMap, spawnPosition);

            // Create player entity
            CreatePlayer(spawnPosition);

            // Create UI elements
            CreateCoinCounterUI();

            // Spawn coins randomly on floor tiles
            SpawnCoins(tileMap);

            // Notify progression system that initial level is ready
            EventBus.Publish(new LevelReadyEvent(0, tileMap));
        }

        private void CreateTileMapEntity(TileMap tileMap, TileSet tileSet)
        {
            World.Create(
                new TileMapComponent(tileMap, tileSet, _spriteSheet.Texture, _spriteSheet)
            );
        }

        private Camera CreateCamera(TileMap tileMap, Vector2 initialPosition)
        {
            var camera = new Camera(Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT, smoothSpeed: 5f);

            // Set camera bounds based on map size
            float mapWidth = tileMap.Width * tileMap.TileWidth;
            float mapHeight = tileMap.Height * tileMap.TileHeight;
            camera.SetBounds(mapWidth, mapHeight);

            // Initialize camera at the player's starting position
            camera.Position = initialPosition;

            World.Create(camera);
            return camera;
        }

        private Vector2 FindValidSpawnPosition(TileMap tileMap)
        {
            // Scan for a floor tile to spawn the player on
            // Start from the center and spiral outward
            int centerX = tileMap.Width / 2;
            int centerY = tileMap.Height / 2;

            // First, try the center
            if (tileMap.GetTile(0, centerX, centerY) == 0) // Floor tile
            {
                var (worldX, worldY) = tileMap.TileToWorldCenter(centerX, centerY);
                return new Vector2(worldX, worldY);
            }

            // If center is not valid, search nearby tiles
            for (int radius = 1; radius < tileMap.Width / 2; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int x = centerX + dx;
                        int y = centerY + dy;

                        if (tileMap.IsValidPosition(x, y) && tileMap.GetTile(0, x, y) == 0)
                        {
                            var (worldX, worldY) = tileMap.TileToWorldCenter(x, y);
                            return new Vector2(worldX, worldY);
                        }
                    }
                }
            }

            // Fallback: return center anyway (should never happen with connected caves)
            var (fallbackX, fallbackY) = tileMap.TileToWorldCenter(centerX, centerY);
            return new Vector2(fallbackX, fallbackY);
        }

        private void CreatePlayer(Vector2 position)
        {
            World.Create(
                new Transform(position),
                new Velocity(Vector2.Zero),
                new SpriteRenderer(
                    _spriteSheet.Texture,
                    _spriteSheet.GetNamedRegion("player"),
                    new Vector2(1f, 1f)
                ),
                new BoxCollider(28f, 28f, new Vector2(-14f, -14f)), // Centered 28x28 collider
                PlayerControlled.FullMovement(150f),
                new Player(),
                new PlayerInventory() // Track collected items
            );

            System.Console.WriteLine($"Player created at ({position.X}, {position.Y}) with Player tag and PlayerInventory");
        }

        private void CreateCoinCounterUI()
        {
            // Create coin counter in top-right corner
            World.Create(
                new Transform(Vector2.Zero), // Position will be set by UIAnchorSystem
                new TextRenderer("Coins: 0", Color.White, fontSize: 24f, layer: RenderLayers.UI),
                new UIAnchor(AnchorPoint.TopRight, new Vector2(-200, 20)), // 200px from right, 20px from top
                new CoinCounterUI(),
                new Kobold.Core.Components.UI() // Tag as UI element
            );
        }

        private void SpawnCoins(TileMap tileMap)
        {
            var random = new Random();
            const float spawnChance = 0.05f; // 5% of floor tiles will have coins
            int coinsSpawned = 0;

            // Iterate through all tiles and spawn coins on random floor tiles
            for (int x = 0; x < tileMap.Width; x++)
            {
                for (int y = 0; y < tileMap.Height; y++)
                {
                    // Check if this is a floor tile (tile ID 0)
                    if (tileMap.GetTile(0, x, y) == 0)
                    {
                        // Randomly decide if a coin should spawn here
                        if (random.NextDouble() < spawnChance)
                        {
                            // Convert tile coordinates to world coordinates
                            var (worldX, worldY) = tileMap.TileToWorldCenter(x, y);
                            var position = new Vector2(worldX, worldY);

                            // Create coin entity with trigger-based pickup
                            World.Create(
                                new Transform(position),
                                new SpriteRenderer(
                                    _spriteSheet.Texture,
                                    _spriteSheet.GetNamedRegion("coin"),
                                    new Vector2(1f, 1f)
                                ),
                                new BoxCollider(32f, 32f, new Vector2(-16f, -16f)), // Trigger area
                                new CollisionLayerComponent(CollisionLayer.Trigger), // CRITICAL: Set collision layer for trigger detection
                                new TriggerComponent(
                                    mode: TriggerMode.OnStayWithButton,
                                    activationLayers: CollisionLayer.Player,
                                    triggerTag: "coin"
                                ),
                                new PickupComponent(
                                    effect: new CoinPickupEffect(coinValue: 1),
                                    requiresInteraction: true,
                                    pickupTag: "coin"
                                ),
                                new PowerUp(), // Tag component for categorization
                                new Trigger() // Required tag for trigger system
                            );

                            coinsSpawned++;
                            if (coinsSpawned <= 3)
                            {
                                System.Console.WriteLine($"Spawned coin at tile ({x},{y}) -> world ({worldX},{worldY})");
                            }
                        }
                    }
                }
            }

            System.Console.WriteLine($"Total coins spawned: {coinsSpawned}");
        }

        /// <summary>
        /// Called when progression system requests a new level to be generated.
        /// CaveExplorer handles its own generation using cellular automata.
        /// </summary>
        private void OnGenerateNewLevel(GenerateNewLevelEvent evt)
        {
            System.Console.WriteLine($"CaveExplorerGame: Generating new level at depth {evt.Depth}");

            // CaveExplorer-specific generation using cellular automata
            var config = new CellularAutomataConfig
            {
                Width = 64,
                Height = 48,
                TileWidth = 32,
                TileHeight = 32,
                Iterations = 5,
                InitialWallProbability = 0.45f,
                ConnectCaves = true,
                MinCaveSize = 60,
                WallTileId = 1,
                FloorTileId = 0

                // Could vary difficulty by depth in future:
                // InitialWallProbability = 0.45f + (evt.Depth * 0.02f)
            };

            var generator = new CellularAutomataGenerator(config);
            var (tileMap, tileSet) = generator.GenerateWithTileSet();

            // Create tilemap entity
            CreateTileMapEntity(tileMap, tileSet);

            // Find valid spawn position for camera
            var spawnPosition = FindValidSpawnPosition(tileMap);

            // Create camera entity at spawn position
            CreateCamera(tileMap, spawnPosition);

            // Spawn coins randomly on floor tiles
            SpawnCoins(tileMap);

            // Notify progression system that level is ready
            EventBus.Publish(new LevelReadyEvent(evt.Depth, tileMap));

            System.Console.WriteLine($"CaveExplorerGame: Level {evt.Depth} generation complete");
        }

        private Vector2 FindValidSpawnPosition(TileMap tileMap, int startX, int startY)
        {
            // First, try the specified position
            if (tileMap.GetTile(0, startX, startY) == 0) // Floor tile
            {
                var (worldX, worldY) = tileMap.TileToWorldCenter(startX, startY);
                return new Vector2(worldX, worldY);
            }

            // If not valid, search nearby tiles
            for (int radius = 1; radius < 20; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int x = startX + dx;
                        int y = startY + dy;

                        if (tileMap.IsValidPosition(x, y) && tileMap.GetTile(0, x, y) == 0)
                        {
                            var (worldX, worldY) = tileMap.TileToWorldCenter(x, y);
                            return new Vector2(worldX, worldY);
                        }
                    }
                }
            }

            // Fallback: return the requested position anyway
            var (fallbackX, fallbackY) = tileMap.TileToWorldCenter(startX, startY);
            return new Vector2(fallbackX, fallbackY);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void Render()
        {
            base.Render();
        }
    }
}
