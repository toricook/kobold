---
layout: default
title: Asset Manager
parent: Core Framework
nav_order: 5
---

# Asset Manager - Loading and Managing Assets

The `AssetManager` class handles loading, caching, and managing textures and sprite sheets in Kobold. It's accessible via `GameEngineBase.Assets`.

## Overview

AssetManager provides:
- Automatic texture caching (loads each texture only once)
- Sprite sheet support with JSON configuration files
- Preloading for loading screens
- Memory management (unload unused assets)

## Loading Textures

### LoadTexture()

Loads a texture from file. Automatically caches so subsequent loads return the same instance.

```csharp
ITexture LoadTexture(string path)
```

**Example:**
```csharp
protected override void Initialize()
{
    base.Initialize();

    // Load texture (relative to content root)
    var playerTexture = Assets.LoadTexture("player.png");

    // Create entity with texture
    World.Create(
        new Transform { Position = new Vector2(400, 300) },
        SpriteRenderer.FullTexture(playerTexture)
    );

    // Load again - returns cached instance (fast!)
    var sameTexture = Assets.LoadTexture("player.png");
}
```

**Caching:** Once loaded, textures stay in memory until explicitly unloaded. Loading the same path multiple times returns the cached texture.

### GetTexture()

Gets a previously loaded texture without loading from disk.

```csharp
ITexture? GetTexture(string path)
```

Returns `null` if the texture hasn't been loaded yet.

**Example:**
```csharp
var texture = Assets.GetTexture("player.png");
if (texture != null)
{
    // Texture is already loaded
}
```

### IsTextureLoaded()

Checks if a texture is in the cache.

```csharp
bool IsTextureLoaded(string path)
```

**Example:**
```csharp
if (!Assets.IsTextureLoaded("enemy.png"))
{
    Assets.LoadTexture("enemy.png");
}
```

## Sprite Sheets

Sprite sheets pack multiple sprites into a single texture, improving performance and simplifying asset management.

### SpriteSheet JSON Format

Kobold uses JSON files to define sprite sheet layouts:

```json
{
  "texture": "characters.png",
  "spriteWidth": 32,
  "spriteHeight": 32,
  "columns": 8,
  "rows": 4,
  "spacing": 0,
  "margin": 0,
  "namedRegions": {
    "player_idle": { "x": 0, "y": 0, "width": 32, "height": 32 },
    "player_walk_1": { "x": 32, "y": 0, "width": 32, "height": 32 },
    "player_walk_2": { "x": 64, "y": 0, "width": 32, "height": 32 }
  },
  "animations": {
    "walk": {
      "frames": [1, 2, 3, 2],
      "frameDuration": 0.1,
      "loop": true
    },
    "idle": {
      "frames": [0],
      "frameDuration": 0.5,
      "loop": true
    }
  }
}
```

### LoadSpriteSheet()

Loads a sprite sheet configuration and the associated texture.

```csharp
SpriteSheet LoadSpriteSheet(string path)
```

**Example:**
```csharp
// Load sprite sheet
var spriteSheet = Assets.LoadSpriteSheet("characters.json");
var texture = Assets.LoadTexture("characters.png");

// Get a specific frame
Rectangle frame0 = spriteSheet.GetFrame(0);
Rectangle frame5 = spriteSheet.GetFrame(5);

// Get by row/column
Rectangle topLeft = spriteSheet.GetFrameByRowCol(row: 0, col: 0);

// Get named region
Rectangle playerIdle = spriteSheet.GetNamedRegion("player_idle");

// Create entity with sprite
World.Create(
    new Transform { Position = new Vector2(200, 200) },
    SpriteRenderer.FromSpriteSheet(texture, frame0)
);
```

### SpriteSheet Methods

**GetFrame(index)** - Get frame by index (left-to-right, top-to-bottom)
```csharp
Rectangle GetFrame(int index)
```

**GetFrameByRowCol(row, col)** - Get frame by grid position
```csharp
Rectangle GetFrameByRowCol(int row, int col)
```

**GetNamedRegion(name)** - Get frame by name
```csharp
Rectangle GetNamedRegion(string name)
```

**GetAnimationFrames(animationName)** - Get all frames for an animation
```csharp
Rectangle[] GetAnimationFrames(string animationName)
```

**CreateAnimationClip(animationName)** - Create AnimationClip from config
```csharp
AnimationClip CreateAnimationClip(string animationName)
```

**CreateAllAnimations()** - Create all animations as dictionary
```csharp
Dictionary<string, AnimationClip> CreateAllAnimations()
```

### Animation Example

```csharp
var spriteSheet = Assets.LoadSpriteSheet("player.json");
var texture = Assets.LoadTexture("player.png");

// Create all animations from sprite sheet
var animation = new Animation
{
    Clips = spriteSheet.CreateAllAnimations()
};
animation.Play("idle");

World.Create(
    new Transform { Position = new Vector2(400, 300) },
    SpriteRenderer.FromSpriteSheet(texture, spriteSheet.GetFrame(0)),
    animation
);
```

## Preloading

Preload textures during a loading screen to avoid hitches during gameplay.

### PreloadTextures()

Loads multiple textures at once.

```csharp
void PreloadTextures(params string[] paths)
```

**Example:**
```csharp
protected override void Initialize()
{
    base.Initialize();

    // Preload all game textures
    Assets.PreloadTextures(
        "player.png",
        "enemy1.png",
        "enemy2.png",
        "projectile.png",
        "background.png"
    );

    // Now all textures are cached and ready to use
    var player = Assets.GetTexture("player.png");  // Instant!
}
```

## Unloading

Free memory by unloading unused textures.

### UnloadTexture()

Unloads a specific texture from the cache.

```csharp
bool UnloadTexture(string path)
```

Returns `true` if the texture was unloaded, `false` if it wasn't loaded.

**Example:**
```csharp
// Unload unused boss texture after boss fight
Assets.UnloadTexture("boss.png");
```

### UnloadAll()

Unloads all textures from the cache.

```csharp
void UnloadAll()
```

**Example:**
```csharp
// When transitioning between levels
Assets.UnloadAll();
// Then load new level's assets
Assets.PreloadTextures("level2_bg.png", "level2_tiles.png");
```

## Content Root

The content root is set by the `IContentLoader` implementation (usually via MonoGameContentLoader).

```csharp
// MonoGameContentLoader sets this
var loader = new MonoGameContentLoader("Content");  // Content root directory
```

All asset paths are relative to this root:
```csharp
// Loads from "Content/player.png"
Assets.LoadTexture("player.png");

// Loads from "Content/sprites/enemy.png"
Assets.LoadTexture("sprites/enemy.png");
```

## Best Practices

### 1. Preload During Initialize

```csharp
protected override void Initialize()
{
    base.Initialize();

    // Load all assets upfront
    Assets.PreloadTextures(
        "player.png",
        "enemy.png",
        "background.png"
    );

    // Create entities after assets are loaded
    var playerTexture = Assets.GetTexture("player.png");
    // ...
}
```

### 2. Organize Assets in Folders

```
Content/
├── characters/
│   ├── player.png
│   └── enemies.png
├── environment/
│   ├── tiles.png
│   └── background.png
└── ui/
    └── icons.png
```

```csharp
Assets.LoadTexture("characters/player.png");
Assets.LoadTexture("environment/tiles.png");
```

### 3. Use Sprite Sheets for Related Sprites

Instead of:
```
Content/
├── player_idle.png
├── player_walk1.png
├── player_walk2.png
├── player_jump.png
└── ...
```

Use a single sprite sheet:
```
Content/
├── player.png          (single texture with all frames)
└── player.json         (sprite sheet config)
```

Benefits:
- Fewer texture loads
- Better GPU performance (fewer texture switches)
- Easier to manage animations

### 4. Unload Between Levels

```csharp
void LoadLevel(int levelNumber)
{
    // Clear old level assets
    Assets.UnloadAll();

    // Load new level assets
    Assets.PreloadTextures($"level{levelNumber}_bg.png", /* ... */);
}
```

### 5. Check Loaded State When Needed

```csharp
void SpawnEnemy()
{
    if (!Assets.IsTextureLoaded("enemy.png"))
    {
        Assets.LoadTexture("enemy.png");
    }

    var texture = Assets.GetTexture("enemy.png");
    // Create enemy entity...
}
```

## Complete Example

```csharp
public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Preload all assets
        Assets.PreloadTextures(
            "player.png",
            "enemy.png",
            "projectile.png",
            "background.png"
        );

        // Load sprite sheet
        var playerSheet = Assets.LoadSpriteSheet("player.json");
        var playerTexture = Assets.GetTexture("player.png");

        // Create player with animation
        var animation = new Animation
        {
            Clips = playerSheet.CreateAllAnimations()
        };
        animation.Play("idle");

        World.Create(
            new Transform { Position = new Vector2(400, 300) },
            SpriteRenderer.FromSpriteSheet(
                playerTexture,
                playerSheet.GetFrame(0)
            ),
            animation,
            new PlayerControlled()
        );

        // Create background
        var bgTexture = Assets.GetTexture("background.png");
        World.Create(
            new Transform { Position = Vector2.Zero },
            SpriteRenderer.Background(bgTexture)
        );
    }

    protected override void Shutdown()
    {
        // Clean up (optional - Kobold does this automatically)
        Assets.UnloadAll();
        base.Shutdown();
    }
}
```

## See Also

- **[SpriteRenderer Component](components.md#spriterenderer)** - Using loaded textures
- **[Animation](components.md#animation)** - Sprite animation
- **[Sprite Sheet Editor](../tools/sprite-sheet-editor.md)** - Creating sprite sheet configs

---

**Next:** Learn about the [Event Bus](event-bus.md) →
