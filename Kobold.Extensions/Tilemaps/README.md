# Tilemaps Extension

Grid-based level design and rendering system for Kobold Engine.

## Overview

The Tilemaps extension provides a complete solution for creating grid-based game levels using tiles. This is perfect for:
- Platformers (like Super Mario, Celeste)
- Top-down RPGs (like classic Zelda)
- Roguelikes and dungeon crawlers
- Puzzle games
- Strategy games

## Features

### Core Tilemap Features ✅
- ✅ **Multi-layer tilemaps** - Background, foreground, collision layers
- ✅ **Tileset management** - Tile properties, collision, friction, damage
- ✅ **Efficient rendering** - Viewport culling for visible tiles only
- ✅ **Tile properties** - Solid, friction, damage, custom properties
- ✅ **Collision system** - AABB collision with solid and platform tiles
- ✅ **ECS integration** - Full Kobold.Core component integration

### Advanced Features (Implemented)
- ✅ **Collision resolution** - Solid tiles, one-way platforms, triggers
- ✅ **Animated tiles** - Frame-based animation support
- ✅ **Collision layers** - Solid, Platform, Trigger, Water, Ice, Ladder
- ✅ **World/Tile conversion** - Coordinate transformation utilities
- ✅ **Event system** - Tile triggers and damage events

### Future Features
- ⏳ **Tiled format support** - Import maps from Tiled Map Editor
- ⏳ **Autotiling** - Automatically choose correct tile variants
- ⏳ **Procedural generation** - Generate random or algorithmic maps
- ⏳ **Chunk-based loading** - For large or infinite worlds

## API Usage

```csharp
using Kobold.Extensions.Tilemaps;
using Kobold.Core.Components;

// Create a tilemap
var tileMap = new TileMap(
    width: 100,
    height: 50,
    tileWidth: 16,
    tileHeight: 16,
    layerCount: 2
);

// Create tileset and configure tile properties
var tileSet = new TileSet(tileWidth: 16, tileHeight: 16, spacing: 1, margin: 1);
tileSet.SetTileProperties(0, TileProperties.Solid());
tileSet.SetTileProperties(1, TileProperties.Platform());
tileSet.SetTileProperties(2, TileProperties.WithDamage(10, "spike"));

// Set individual tiles
tileMap.SetTile(layer: 0, x: 10, y: 15, tileId: 42);

// Fill a region
tileMap.Fill(layer: 0, x: 0, y: 0, width: 10, height: 10, tileId: 1);

// Create from texture (for rendering)
var textureBasedTileset = TileSet.FromTexture(
    texturePath: "tileset.png",
    textureWidth: 256,
    textureHeight: 256,
    tileWidth: 16,
    tileHeight: 16,
    spacing: 1,
    margin: 1
);

// Add to game world as entity
var tilemapEntity = world.Create(
    new Transform(Vector2.Zero),
    new TilemapComponent(tileMap, tileSet)
    {
        EnableCollision = true,
        RenderLayer = 0,
        Opacity = 1.0f,
        Visible = true
    }
);

// Set up systems
var tilemapSystem = new TilemapSystem(world);
var collisionSystem = new TilemapCollisionSystem(world, eventBus);
systemManager.AddSystem(tilemapSystem);
systemManager.AddSystem(collisionSystem);

// Query tiles
var tileId = tileMap.GetTile(layer: 0, x: 5, y: 10);
var tileProperties = tileSet.GetTileProperties(tileId);

// Coordinate conversion
var (tileX, tileY) = tileMap.WorldToTile(worldX: 100f, worldY: 50f);
var (worldX, worldY) = tileMap.TileToWorld(tileX: 5, tileY: 10);

// Check collision
bool isSolid = tileSet.IsSolid(tileId: 0);

// Get tiles in viewport (for rendering)
var visibleTiles = TilemapSystem.GetVisibleTiles(
    tileMap, layer: 0,
    cameraX: 0, cameraY: 0,
    viewportWidth: 800, viewportHeight: 600
);
```

## Components

### TilemapComponent
ECS component for attaching tilemaps to entities.
```csharp
public struct TilemapComponent
{
    public TileMap TileMap { get; set; }
    public TileSet TileSet { get; set; }
    public bool EnableCollision { get; set; }
    public int RenderLayer { get; set; }
    public float Opacity { get; set; }
    public bool Visible { get; set; }
}
```

### TileProperties
Properties associated with each tile ID.
```csharp
public struct TileProperties
{
    public bool IsSolid { get; set; }
    public TileCollisionLayer CollisionLayer { get; set; }
    public float Friction { get; set; }
    public int Damage { get; set; }
    public string? Type { get; set; }
    public bool IsAnimated { get; set; }
    public int[]? AnimationFrames { get; set; }
    public float AnimationSpeed { get; set; }
    public Dictionary<string, object>? CustomProperties { get; set; }
}
```

## Systems

### TilemapSystem
Manages tilemap updates, animated tiles, and provides rendering utilities.

**Key Methods:**
- `Update(deltaTime)` - Updates animated tiles
- `GetVisibleTiles()` - Returns tiles visible in viewport (for rendering)
- `SetTilemapVisibility()` - Show/hide tilemap
- `SetTilemapOpacity()` - Adjust tilemap transparency

### TilemapCollisionSystem
Handles collision detection and resolution between entities and tiles.

**Features:**
- AABB collision with solid tiles
- One-way platform collision (from above only)
- Trigger tiles (event-based)
- Tile effects (friction, damage)
- Collision layers (Solid, Platform, Trigger, Water, Ice, Ladder)

**Events:**
- `TileTriggerEvent` - Published when entity enters trigger tile
- `TileDamageEvent` - Published when entity takes damage from tile

## Integration with Kobold.Core

The Tilemap system integrates seamlessly with Core systems:

```csharp
// Collision with player
collisionSystem.OnCollision((entity1, entity2) =>
{
    if (entity2.Has<TilemapComponent>())
    {
        var tilemap = entity2.Get<TilemapComponent>().TileMap;
        var playerPos = entity1.Get<Transform>().Position;

        // Check which tile the player hit
        var tileX = (int)(playerPos.X / tilemap.TileWidth);
        var tileY = (int)(playerPos.Y / tilemap.TileHeight);

        if (tilemap.IsSolid(tileX, tileY))
        {
            // Resolve collision
        }
    }
});
```

## Implementation Status

### Milestone 1: Basic Tilemaps ✅
- ✅ TileMap class with multi-layer support
- ✅ TileSet with properties and collision layers
- ✅ GetTile/SetTile/Fill API
- ✅ World/tile coordinate conversion

### Milestone 2: Rendering & Performance ✅
- ✅ Viewport culling (GetVisibleTiles)
- ✅ Multi-layer rendering support
- ✅ Opacity and visibility controls
- ⏳ Platform-specific batch rendering (MonoGame, etc.)

### Milestone 3: Collision ✅
- ✅ Solid tile collision detection
- ✅ AABB collision resolution
- ✅ Tile properties (friction, damage, type)
- ✅ Platform (one-way) collision
- ✅ Trigger tiles with events
- ⏳ Slope/ramp support

### Milestone 4: Tiled Integration
- ⏳ .tmx file parsing
- ⏳ Tileset (.tsx) loading
- ⏳ Object layer support
- ⏳ Custom property support

### Milestone 5: Advanced Features
- ✅ Animated tile support (structure ready)
- ⏳ Autotiling system
- ⏳ Procedural generation helpers
- ⏳ Chunk-based infinite worlds

## Examples

### Example 1: Simple Platformer Level
```csharp
using Kobold.Extensions.Tilemaps;

// Create tilemap and tileset
var tileMap = new TileMap(50, 30, 16, 16, layerCount: 2);
var tileSet = new TileSet(16, 16);

// Configure tiles
const int GROUND = 0;
const int PLATFORM = 1;
const int SPIKE = 2;

tileSet.SetTileProperties(GROUND, TileProperties.Solid());
tileSet.SetTileProperties(PLATFORM, TileProperties.Platform());
tileSet.SetTileProperties(SPIKE, TileProperties.WithDamage(10, "spike"));

// Build level - Create ground
for (int x = 0; x < 50; x++)
{
    tileMap.SetTile(layer: 1, x: x, y: 29, tileId: GROUND);
}

// Create platforms
tileMap.Fill(layer: 1, x: 10, y: 20, width: 5, height: 1, tileId: PLATFORM);
tileMap.Fill(layer: 1, x: 20, y: 15, width: 5, height: 1, tileId: PLATFORM);

// Add spikes
for (int x = 15; x < 20; x++)
{
    tileMap.SetTile(layer: 1, x: x, y: 28, tileId: SPIKE);
}

// Create entity
var tilemapEntity = world.Create(
    new Transform(Vector2.Zero),
    new TilemapComponent(tileMap, tileSet) { EnableCollision = true }
);
```

### Example 2: Dungeon Room
```csharp
var tileMap = new TileMap(20, 15, 16, 16);
var tileSet = new TileSet(16, 16);

const int WALL = 0;
const int FLOOR = 1;

tileSet.SetTileProperties(WALL, TileProperties.Solid());
tileSet.SetTileProperties(FLOOR, new TileProperties { IsSolid = false });

// Fill with walls
tileMap.Fill(layer: 0, x: 0, y: 0, width: 20, height: 15, tileId: WALL);

// Carve out floor
tileMap.Fill(layer: 0, x: 2, y: 2, width: 16, height: 11, tileId: FLOOR);
```

### Example 3: Handling Tile Events
```csharp
// Subscribe to tile events
eventBus.Subscribe<TileTriggerEvent>(evt =>
{
    Console.WriteLine($"Entity {evt.Entity} triggered {evt.TriggerType} at ({evt.TileX}, {evt.TileY})");
});

eventBus.Subscribe<TileDamageEvent>(evt =>
{
    Console.WriteLine($"Entity {evt.Entity} took {evt.Damage} damage from {evt.DamageType}");
    // Apply damage to entity...
});
```

## Contributing

The Tilemap extension is currently in early development. Contributions are welcome!

Priority areas:
1. Core TileMap and TileSet classes
2. Basic rendering system
3. Tiled (.tmx) file parsing
4. Collision integration

## Resources

- [Tiled Map Editor](https://www.mapeditor.org/) - Popular tilemap editor
- [OpenGameArt](https://opengameart.org/) - Free tilesets
- [Kenney Assets](https://kenney.nl/) - Free game assets including tilesets

---

**Status:** ✅ Core Features Complete - Ready to Use!
