# Kobold.Extensions

**Optional game features that many games use, but not all.**

Kobold.Extensions provides specialized systems and components that are common in games but not universally required. While Kobold.Core gives you the essentials, Extensions adds the features that make your game richer and more polished.

## Philosophy

Kobold.Extensions follows the principle of **useful but optional**:
- ✅ Include: Features that many games use but aren't strictly necessary
- ✅ Include: Specialized systems that would bloat Kobold.Core
- ❌ Exclude: Game-specific logic (that stays in your game project)

## What's Included

### Current Features

#### 🗺️ Tilemaps ✅
Grid-based level design and rendering system.

**Implemented:**
- ✅ Multi-layer tilemaps with configurable dimensions
- ✅ Tileset management with tile properties
- ✅ Collision detection and resolution (solid, platform, trigger)
- ✅ Animated tile support
- ✅ World/tile coordinate conversion
- ✅ ECS integration with TilemapComponent
- ✅ Collision layers (Solid, Platform, Trigger, Water, Ice, Ladder)
- ✅ Tile properties (friction, damage, custom data)

**Coming soon:**
- ⏳ Tiled (.tmx) map format support
- ⏳ Procedural tile generation helpers
- ⏳ Chunk-based loading for large maps

### Planned Extensions

#### ⚡ Particle Systems
Visual effects for explosions, fire, smoke, trails, etc.

#### 🤖 AI & Pathfinding
- A* pathfinding
- State machines for AI
- Behavior trees
- Flocking and steering behaviors

#### 💬 Dialogue System
- Conversation management
- Branching dialogue trees
- Localization support
- Character portraits and typing effects

#### 🎬 Animation System
- Sprite sheet animations
- Animation state machines
- Tweening and easing
- Skeletal animation support

#### 🎵 Audio Extensions
- Sound effects pooling
- Music transitions
- 3D spatial audio
- Audio mixing and DSP

#### 🌟 Screen Effects
- Camera shake
- Screen transitions
- Post-processing effects
- Particle backgrounds

#### 💾 Save/Load System
- Save state management
- Serialization helpers
- Cloud save integration
- Checkpoint system

#### 📊 UI Framework
- Menu systems
- Health bars and HUDs
- Inventory systems
- Modal dialogs

## When to Use Extensions vs Core

| Feature | Location | Reason |
|---------|----------|--------|
| Transform, Velocity | **Core** | Every game needs entities to move |
| Collision Detection | **Core** | Most games need collision |
| Input Handling | **Core** | Essential for player interaction |
| Tilemaps | **Extensions** | Not all games use tile-based levels |
| Particle Effects | **Extensions** | Nice to have, not essential |
| Dialogue System | **Extensions** | Only narrative games need this |
| AI Pathfinding | **Extensions** | Action games might not need it |

## Architecture

```
Kobold.Extensions/
├── Tilemaps/          # Grid-based level system
│   ├── TileMap.cs
│   ├── TileLayer.cs
│   ├── TileSet.cs
│   └── TilemapSystem.cs
├── Particles/         # Particle effects (planned)
├── AI/                # Pathfinding and behaviors (planned)
├── Dialogue/          # Conversation system (planned)
├── Animation/         # Advanced animations (planned)
└── Audio/             # Audio management (planned)
```

## Installation

```bash
# Install Core first
dotnet add package Kobold.Core

# Then install Extensions
dotnet add package Kobold.Extensions
```

Or reference both in your `.csproj`:
```xml
<ItemGroup>
  <ProjectReference Include="..\Kobold\Kobold.Core.csproj" />
  <ProjectReference Include="..\Kobold.Extensions\Kobold.Extensions.csproj" />
</ItemGroup>
```

## Example Usage

### Using Tilemaps

```csharp
using Kobold.Extensions.Tilemaps;
using Kobold.Core.Components;

// Create a tilemap
var tileMap = new TileMap(
    width: 50,
    height: 30,
    tileWidth: 16,
    tileHeight: 16,
    layerCount: 2
);

// Create a tileset
var tileSet = new TileSet(tileWidth: 16, tileHeight: 16);
tileSet.SetTileProperties(0, TileProperties.Solid());
tileSet.SetTileProperties(1, TileProperties.Platform());

// Set tiles
tileMap.SetTile(layer: 0, x: 5, y: 10, tileId: 0);
tileMap.Fill(layer: 1, x: 0, y: 29, width: 50, height: 1, tileId: 0);

// Create tilemap entity
var tilemapEntity = world.Create(
    new Transform(Vector2.Zero),
    new TilemapComponent(tileMap, tileSet)
    {
        EnableCollision = true,
        RenderLayer = 0
    }
);

// Add tilemap systems to your game
var tilemapSystem = new TilemapSystem(world);
var collisionSystem = new TilemapCollisionSystem(world, eventBus);
systemManager.AddSystem(tilemapSystem);
systemManager.AddSystem(collisionSystem);
```

### Combining Core + Extensions

```csharp
using Kobold.Core;
using Kobold.Core.Systems;
using Kobold.Extensions.Tilemaps;

// Core systems (essential)
var physics = new PhysicsSystem(world);
var collision = new CollisionSystem(world, eventBus);

// Extension systems (optional)
var tilemap = new TilemapSystem(world, myTilemap);
// var particles = new ParticleSystem(world); // When available
// var dialogue = new DialogueSystem(world);   // When available

// Update loop
void Update(float deltaTime)
{
    // Core
    physics.Update(deltaTime);
    collision.Update(deltaTime);

    // Extensions
    tilemap.Update(deltaTime);
}
```

## Design Principles

1. **Modular** - Each extension is independent and optional
2. **Consistent** - Follows Kobold.Core patterns and conventions
3. **Performant** - Optimized for game loops, but not at Core's expense
4. **Documented** - Each extension has examples and API docs
5. **Tested** - Comprehensive tests for all extensions

## Contributing

We welcome contributions! Here's how extensions are organized:

### Adding a New Extension

1. **Create a folder** for your feature (e.g., `Particles/`)
2. **Implement core classes** following ECS patterns
3. **Create a System** that integrates with Kobold.Core
4. **Add tests** in the Tests project
5. **Document** with XML comments and examples
6. **Update this README** with your extension

### Extension Checklist

- [ ] Follows ECS patterns (Components + System)
- [ ] Works with Kobold.Core's event bus
- [ ] Includes unit and system tests
- [ ] Has XML documentation
- [ ] Includes usage examples
- [ ] Updates README.md
- [ ] Minimal dependencies (only add if necessary)

## Roadmap

### Phase 1 ✅ Complete
- ✅ Project structure
- ✅ Tilemaps - Core implementation
- ✅ Tilemaps - Collision system
- ✅ Tilemaps - Multi-layer support
- ✅ Tilemaps - Tile properties and effects

### Phase 2
- ⏳ Particle system
- ⏳ Animation framework
- ⏳ Basic AI state machines

### Phase 3
- ⏳ Dialogue system
- ⏳ A* pathfinding
- ⏳ UI framework

### Phase 4
- ⏳ Save/load system
- ⏳ Audio extensions
- ⏳ Screen effects

## Related Packages

- **Kobold.Core** - Essential game engine features
- **Kobold.Monogame** - MonoGame platform implementation

## License

[Add your license here]

---

**Note:** Kobold.Extensions is under active development. Features marked with 🚧 are in progress, and those with ⏳ are planned for future releases.
