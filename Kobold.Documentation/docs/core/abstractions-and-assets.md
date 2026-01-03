# Abstractions and Assets Architecture

This document explains Kobold's **Abstractions** and **Assets** layers - the foundation for platform independence and high-level asset management.

## Table of Contents

- [Overview](#overview)
- [The Abstractions Layer](#the-abstractions-layer)
- [The Assets Layer](#the-assets-layer)
- [Platform Implementations](#platform-implementations)
- [Best Practices](#best-practices)

## Overview

Kobold separates concerns into distinct architectural layers:

```
┌─────────────────────────────────────┐
│      Game Logic (ECS)               │  ← Your game code
├─────────────────────────────────────┤
│      Assets Layer                   │  ← High-level wrappers (SpriteSheet, Tileset, TextureAtlas)
├─────────────────────────────────────┤
│      Abstractions Layer             │  ← Platform interfaces (ITexture, IRenderer, IContentLoader)
│   - Engine/                         │  ← Game engine abstractions (IGameEngine, ISystem)
│   - Assets/                         │  ← Asset loading (IContentLoader)
│   - Rendering/                      │  ← Graphics (IRenderer, ITexture)
│   - Input/                          │  ← Input (IInputManager)
│   - Audio/                          │  ← Audio (IAudioPlayer, ISoundEffect)
├─────────────────────────────────────┤
│   Platform Implementation           │  ← MonoGame, Raylib, etc.
│   (Kobold.Monogame)                 │
└─────────────────────────────────────┘
```

### Key Principles

1. **Abstractions** = Low-level platform interfaces
2. **Assets** = High-level data structures built on abstractions
3. **Complete separation** between game logic and platform code
4. **Dependency injection** for platform services

## The Abstractions Layer

Location: `Kobold/Abstractions/`

The Abstractions layer defines **platform-agnostic interfaces** for all hardware and platform-specific functionality.

### Core Abstractions

#### `Abstractions/Engine/`

**IGameEngine** - The game lifecycle interface
```csharp
public interface IGameEngine
{
    void Initialize();
    void Update(float deltaTime);
    void Render();
    void Shutdown();
}
```

**ISystem** - Interface for update systems
```csharp
public interface ISystem
{
    void Update(float deltaTime);
}
```

**IRenderSystem** - Interface for render-only systems
```csharp
public interface IRenderSystem
{
    void Render();
}
```

**IContentLoader** - Platform-agnostic asset loading
```csharp
public interface IContentLoader
{
    string ContentRoot { get; }
    ITexture LoadTexture(string path);
    ISoundEffect LoadSoundEffect(string path);
    IMusic LoadMusic(string path);
    bool TextureExists(string path);
    bool SoundExists(string path);
}
```

#### `Abstractions/Rendering/`

**IRenderer** - Drawing operations
```csharp
public interface IRenderer
{
    void Begin();
    void End();
    void DrawRectangle(Vector2 position, Vector2 size, Color color);
    void DrawTexture(ITexture texture, Vector2 position, Vector2 scale = default);
    void DrawSprite(ITexture texture, Vector2 position, Rectangle sourceRect,
                    Vector2 scale, float rotation, Color tint);
    void DrawText(string text, Vector2 position, Color color, float fontSize = 16f);
    void DrawTriangle(Vector2[] points, Vector2 position, float rotation, Color color);
    void DrawTriangleFilled(Vector2[] points, Vector2 position, float rotation, Color color);
    void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f);
}
```

**ITexture** - Texture abstraction
```csharp
public interface ITexture
{
    int Width { get; }
    int Height { get; }
}
```

#### `Abstractions/Input/`

**IInputManager** - Input handling
```csharp
public interface IInputManager
{
    bool IsKeyPressed(KeyCode key);
    bool IsKeyDown(KeyCode key);
    bool IsKeyUp(KeyCode key);
    Vector2 GetMousePosition();
    bool IsMouseButtonPressed(MouseButton button);
}
```

Includes `KeyCode` and `MouseButton` enums for platform-independent input.

#### `Abstractions/Audio/`

**IAudioPlayer** - Music playback control
```csharp
public interface IAudioPlayer
{
    IMusic? CurrentMusic { get; }
    float MusicVolume { get; set; }
    bool IsPlaying { get; }
    void PlayMusic(IMusic music, bool loop = true);
    void StopMusic();
    void PauseMusic();
    void ResumeMusic();
}
```

**ISoundEffect** - Sound effect abstraction
```csharp
public interface ISoundEffect : IDisposable
{
    void Play(float volume = 1.0f);
    ISoundEffectInstance CreateInstance();
}
```

**ISoundEffectInstance** - Controllable sound instance
```csharp
public interface ISoundEffectInstance : IDisposable
{
    float Volume { get; set; }
    bool IsLooped { get; set; }
    SoundState State { get; }
    void Play();
    void Pause();
    void Resume();
    void Stop();
}
```

**IMusic** - Music track abstraction
```csharp
public interface IMusic : IDisposable
{
    string Name { get; }
    TimeSpan Duration { get; }
}
```

### Why Abstractions Matter

**Platform Independence**
```csharp
// Your game code - works with any platform
public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Uses IContentLoader - platform agnostic
        var texture = ContentLoader.LoadTexture("player.png");

        // Uses IRenderer - platform agnostic
        var entity = World.Create(
            new Transform(),
            SpriteRenderer.FullTexture(texture)  // ITexture
        );
    }
}
```

**Testability**
```csharp
// Unit test - no graphics device needed
[Test]
public void TestGameLogic()
{
    var mockRenderer = new MockRenderer();
    var mockInput = new MockInputManager();

    var game = new MyGame();
    game.SetRenderer(mockRenderer);
    game.SetInputManager(mockInput);
    game.Initialize();

    game.Update(1.0f);

    // Verify game logic without rendering
}
```

**Future Flexibility**

If you ever need to switch from MonoGame to another engine:
1. Implement the abstractions for the new platform
2. Your game code doesn't change at all
3. Only platform-specific code is replaced

## The Assets Layer

Location: `Kobold/Assets/`

The Assets layer provides **high-level asset wrappers** that build on top of the abstractions. These classes add game-oriented functionality on top of the low-level platform interfaces.

### Asset Types

#### SpriteSheet

Manages uniform grid-based sprite extraction from a texture.

```csharp
// Load a sprite sheet
var texture = ContentLoader.LoadTexture("characters.png");
var config = new SpriteSheetConfig
{
    SpriteWidth = 32,
    SpriteHeight = 32,
    Columns = 8,
    Rows = 4
};
var spriteSheet = new SpriteSheet(texture, config);

// Get individual frames
Rectangle frame = spriteSheet.GetFrame(5);  // 6th sprite

// Create animations
config.Animations["walk"] = new AnimationData
{
    Row = 0,
    FrameCount = 4,
    Fps = 8,
    Loop = true
};
var walkAnimation = spriteSheet.CreateAnimationClip("walk");
```

**Key Features:**
- Grid-based sprite extraction
- Animation definitions
- Named regions
- Frame indexing (by ID or row/col)

#### Tileset

Manages tile collections for building tilemaps.

```csharp
// Load a tileset
var texture = ContentLoader.LoadTexture("terrain.png");
var config = new TilesetConfig
{
    TileWidth = 16,
    TileHeight = 16,
    Columns = 16,
    Rows = 8
};
var tileset = new Tileset(texture, config);

// Get tile rectangles
Rectangle grassTile = tileset.GetTileRect(0);
Rectangle waterTile = tileset.GetTileRect(5);

// Add tile metadata
config.TileMetadata[5] = new TileMetadata
{
    Name = "Water",
    Tags = new List<string> { "liquid", "impassable" }
};
```

**Key Features:**
- Tile ID to rectangle mapping
- Tile metadata (tags, properties)
- Animated tiles
- Spacing and margin support

#### TextureAtlas

Manages irregularly-sized sprite regions in a packed texture.

```csharp
// Load a texture atlas
var texture = ContentLoader.LoadTexture("sprites.png");
var config = new TextureAtlasConfig();
config.Regions.Add(new AtlasRegion
{
    Name = "player_idle",
    Bounds = new Rectangle(0, 0, 32, 48),
    Pivot = new Vector2(0.5f, 1.0f)  // Bottom-center pivot
});
config.Regions.Add(new AtlasRegion
{
    Name = "enemy_bat",
    Bounds = new Rectangle(32, 0, 24, 16),
    Pivot = new Vector2(0.5f, 0.5f)  // Center pivot
});

var atlas = new TextureAtlas(texture, config);

// Get sprite regions by name
var playerRegion = atlas.GetRegion("player_idle");
Rectangle playerRect = playerRegion.Bounds;
```

**Key Features:**
- Named sprite regions
- Non-uniform sprite sizes
- Custom pivot points
- Rotation support (for packed atlases)
- Original size tracking (for trimmed sprites)

### Assets vs Abstractions

| Layer | Purpose | Examples |
|-------|---------|----------|
| **Abstractions** | Platform interfaces | `ITexture`, `IRenderer`, `ISoundEffect` |
| **Assets** | High-level wrappers | `SpriteSheet`, `Tileset`, `TextureAtlas` |

**Assets build on Abstractions:**

```csharp
// Abstraction (low-level)
ITexture texture = ContentLoader.LoadTexture("sprites.png");
Renderer.DrawTexture(texture, position);

// Asset (high-level)
SpriteSheet spriteSheet = new SpriteSheet(texture, config);
Rectangle frame = spriteSheet.GetFrame(0);
Renderer.DrawSprite(texture, position, frame, scale, rotation, Color.White);
```

### Asset Configuration

All asset types support JSON serialization for external configuration:

```json
{
  "spriteWidth": 32,
  "spriteHeight": 32,
  "columns": 8,
  "rows": 4,
  "animations": {
    "walk": {
      "row": 0,
      "frameCount": 4,
      "fps": 8,
      "loop": true
    }
  }
}
```

This allows designers to configure assets without changing code.

## Platform Implementations

### Kobold.Monogame

The `Kobold.Monogame` project implements all abstractions using MonoGame.

**MonoGameFileContentLoader** → implements `IContentLoader`
```csharp
public class MonoGameFileContentLoader : IContentLoader
{
    public ITexture LoadTexture(string path)
    {
        using (var stream = File.OpenRead(path))
        {
            var texture = Texture2D.FromStream(_graphicsDevice, stream);
            return new MonoGameTexture(texture);
        }
    }
}
```

**MonoGameRenderer** → implements `IRenderer`
```csharp
public class MonoGameRenderer : IRenderer
{
    public void DrawTexture(ITexture texture, Vector2 position, Vector2 scale)
    {
        var mgTexture = ((MonoGameTexture)texture).Texture;
        _spriteBatch.Draw(mgTexture, position, ...);
    }
}
```

**MonoGameInputManager** → implements `IInputManager`
**MonoGameAudioPlayer** → implements `IAudioPlayer`
**MonoGameSoundEffect** → implements `ISoundEffect`
**MonoGameMusic** → implements `IMusic`

### Creating Alternative Implementations

To support a new platform (e.g., Raylib):

1. Create a new project (e.g., `Kobold.Raylib`)
2. Implement all abstractions:
   - `IRenderer`
   - `IContentLoader`
   - `IInputManager`
   - `IAudioPlayer`
   - Audio interfaces
3. Create a host class that wires everything together
4. Your game code remains unchanged!

## Best Practices

### Do's ✅

**Use abstractions in game code**
```csharp
public class PlayerSystem : ISystem
{
    private readonly IInputManager _input;

    public void Update(float deltaTime)
    {
        if (_input.IsKeyDown(KeyCode.W))
        {
            // Move player
        }
    }
}
```

**Use assets for organized data**
```csharp
// Good - organized asset management
var spriteSheet = Assets.GetSpriteSheet("player");
var animation = spriteSheet.CreateAnimationClip("walk");
```

**Inject platform services**
```csharp
public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Platform services already injected via SetRenderer, SetInputManager, etc.
        var playerSystem = new PlayerSystem(InputManager);
        SystemManager.AddSystem(playerSystem, 0);
    }
}
```

### Don'ts ❌

**Don't reference MonoGame types in game code**
```csharp
// Bad - tight coupling to MonoGame
using Microsoft.Xna.Framework.Graphics;

public class MySystem : ISystem
{
    private Texture2D _texture;  // ❌ MonoGame type
}

// Good - use abstractions
public class MySystem : ISystem
{
    private ITexture _texture;  // ✅ Platform agnostic
}
```

**Don't bypass the abstractions**
```csharp
// Bad - directly using platform code
var mgTexture = ((MonoGameTexture)texture).Texture;
spriteBatch.Draw(mgTexture, ...);  // ❌

// Good - use IRenderer
Renderer.DrawTexture(texture, position);  // ✅
```

**Don't implement platform-specific logic in assets**
```csharp
// Bad - SpriteSheet shouldn't know about MonoGame
public class SpriteSheet
{
    private Texture2D _texture;  // ❌
}

// Good - SpriteSheet uses abstractions
public class SpriteSheet
{
    private ITexture _texture;  // ✅
}
```

## Summary

### Abstractions Layer
- **Purpose:** Define platform-agnostic interfaces
- **Location:** `Kobold/Abstractions/`
- **Subfolders:**
  - `Engine/` - Game engine abstractions (`IGameEngine`, `ISystem`, `IRenderSystem`)
  - `Assets/` - Asset loading (`IContentLoader`)
  - `Rendering/` - Graphics (`IRenderer`, `ITexture`)
  - `Input/` - Input handling (`IInputManager`)
  - `Audio/` - Audio playback (`IAudioPlayer`, `ISoundEffect`, `IMusic`)
- **Used by:** Game logic, Assets layer

### Assets Layer
- **Purpose:** High-level asset wrappers and data structures
- **Location:** `Kobold/Assets/`
- **Examples:** `SpriteSheet`, `Tileset`, `TextureAtlas`
- **Built on:** Abstractions layer (uses `ITexture`, etc.)

### Platform Implementation
- **Purpose:** Implement abstractions for specific platforms
- **Location:** `Kobold.Monogame/` (or other platform projects)
- **Examples:** `MonoGameRenderer`, `MonoGameTexture`
- **Implements:** All abstractions

This layered architecture ensures your game code remains portable, testable, and maintainable while still leveraging platform-specific optimizations where needed.

## Learn More

- [Architecture Overview](architecture.md) - ECS and platform abstraction
- [Asset Manager](asset-manager.md) - High-level asset management
- [Components Reference](components.md) - Built-in ECS components
- [MonoGame Integration](../monogame/) - MonoGame implementation details
