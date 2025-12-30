# Components Reference

Components are data structs that you attach to entities. This page documents all built-in components in Kobold.Core.

## Core Components

### Transform

Position, rotation, and scale in 2D space. Every visible entity needs a Transform.

**Location:** `Kobold.Core.Components.Core.Transform`

```csharp
public struct Transform
{
    public Vector2 Position;  // World position in pixels
    public float Rotation;    // Rotation in radians
    public Vector2 Scale;     // Scale multiplier (1,1 = normal size)
}
```

**Methods:**
- `Translate(Vector2 delta)` - Move by offset
- `Rotate(float radians)` - Rotate by angle
- `LookAt(Vector2 target)` - Point towards target
- `ToMatrix()` - Get transformation matrix

**Properties:**
- `Forward` - Direction vector (based on rotation)
- `Right` - Right vector (perpendicular to forward)

**Example:**
```csharp
var entity = World.Create(
    new Transform
    {
        Position = new Vector2(400, 300),
        Rotation = 0,
        Scale = new Vector2(1, 1)
    }
);

// Rotate to face right
ref var transform = ref entity.Get<Transform>();
transform.LookAt(new Vector2(500, 300));
```

---

### Velocity

Movement vector in pixels per second.

**Location:** `Kobold.Core.Components.Physics.Velocity`

```csharp
public struct Velocity
{
    public Vector2 Value;  // Pixels per second
}
```

**Properties:**
- `Speed` - Magnitude of velocity
- `Direction` - Normalized direction vector
- `IsMoving` - True if speed > 0

**Methods:**
- `SetDirectionAndSpeed(Vector2 direction, float speed)`
- `Add(Vector2 delta)` - Add to velocity
- `ClampToMaxSpeed(float maxSpeed)` - Limit speed

**Static Factories:**
- `FromDirectionAndSpeed(Vector2 direction, float speed)`
- `FromAngleAndSpeed(float radians, float speed)`

**Example:**
```csharp
World.Create(
    new Transform { Position = new Vector2(100, 100) },
    new Velocity { Value = new Vector2(150, 0) }  // Move right at 150 px/s
);

// Or use factory
var velocity = Velocity.FromAngleAndSpeed(MathF.PI / 4, 200); // 45°, 200 px/s
```

---

### Physics

Physical properties for physics simulation.

**Location:** `Kobold.Core.Components.Physics.Physics`

```csharp
public struct Physics
{
    public float Mass;         // kg (affects gravity)
    public float Restitution;  // Bounciness (0-1)
    public float Damping;      // Velocity decay (0-1)
    public bool IsStatic;      // Immovable object
}
```

**Example:**
```csharp
// Bouncy ball
World.Create(
    new Transform { Position = new Vector2(400, 100) },
    new Velocity { Value = Vector2.Zero },
    new Physics
    {
        Mass = 1.0f,
        Restitution = 0.9f,  // Very bouncy
        Damping = 0.99f      // Minimal air resistance
    }
);
```

---

### BoxCollider

Axis-Aligned Bounding Box (AABB) collision shape.

**Location:** `Kobold.Core.Components.Physics.BoxCollider`

```csharp
public struct BoxCollider
{
    public Vector2 Size;    // Width and height
    public Vector2 Offset;  // Offset from Transform position
}
```

**Methods:**
- `GetWorldPosition(Transform)` - Get world-space position
- `GetWorldBounds(Transform)` - Get AABB bounds
- `GetWorldCenter(Transform)` - Get center point
- `Contains(Vector2)` - Point-in-box test
- `Overlaps(BoxCollider, Transform, Transform)` - Box-box test

**Static Factories:**
- `FromRenderSize(int width, int height)` - Match sprite size
- `Square(float size)` - Square collider
- `Shrunken(int width, int height, int margin)` - Smaller than sprite

**Example:**
```csharp
// Collider matching sprite size
var texture = Assets.LoadTexture("player.png");
World.Create(
    new Transform { Position = new Vector2(200, 200) },
    SpriteRenderer.FullTexture(texture),
    BoxCollider.FromRenderSize(texture.Width, texture.Height)
);

// Custom sized collider
World.Create(
    new Transform { Position = new Vector2(300, 300) },
    new RectangleRenderer { Size = new Vector2(64, 64), Color = Color.Blue },
    BoxCollider.Square(64)
);
```

---

## Rendering Components

### SpriteRenderer

Renders a texture or sprite from a sprite sheet.

**Location:** `Kobold.Core.Components.Rendering.SpriteRenderer`

```csharp
public struct SpriteRenderer
{
    public ITexture Texture;
    public Rectangle? SourceRect;  // null = full texture
    public Vector2 Scale;
    public float Rotation;
    public Color Tint;
    public int Layer;
    public Vector2 Pivot;  // Rotation/scale origin (0-1)
}
```

**Static Factories:**
- `FullTexture(ITexture, scale, tint, layer)` - Use entire texture
- `FromSpriteSheet(ITexture, Rectangle sourceRect, ...)` - Single sprite
- `Background(ITexture, layer)` - Background layer
- `GameObject(ITexture, scale, layer)` - Game object layer
- `UI(ITexture, layer)` - UI layer

**Example:**
```csharp
var texture = Assets.LoadTexture("player.png");

// Full texture
World.Create(
    new Transform { Position = new Vector2(100, 100) },
    SpriteRenderer.FullTexture(texture, scale: 2.0f)
);

// From sprite sheet
var spriteSheet = Assets.LoadSpriteSheet("characters.json");
World.Create(
    new Transform { Position = new Vector2(200, 200) },
    SpriteRenderer.FromSpriteSheet(
        texture,
        spriteSheet.GetFrame(0),
        scale: 1.5f,
        tint: Color.Red
    )
);
```

---

### RectangleRenderer

Renders a colored rectangle.

**Location:** `Kobold.Core.Components.Rendering.RectangleRenderer`

```csharp
public struct RectangleRenderer
{
    public Vector2 Size;
    public Color Color;
    public int Layer;
}
```

**Example:**
```csharp
World.Create(
    new Transform { Position = new Vector2(300, 300) },
    new RectangleRenderer
    {
        Size = new Vector2(100, 50),
        Color = Color.Green,
        Layer = 100
    }
);
```

---

### TextRenderer

Renders text.

**Location:** `Kobold.Core.Components.Rendering.TextRenderer`

```csharp
public struct TextRenderer
{
    public string Text;
    public Color Color;
    public float FontSize;
    public int Layer;
}
```

**Example:**
```csharp
World.Create(
    new Transform { Position = new Vector2(10, 10) },
    new TextRenderer
    {
        Text = "Score: 0",
        Color = Color.White,
        FontSize = 24,
        Layer = RenderLayers.UI
    }
);
```

---

### TriangleRenderer

Renders a triangle.

**Location:** `Kobold.Core.Components.Rendering.TriangleRenderer`

```csharp
public struct TriangleRenderer
{
    public Vector2[] Points;  // 3 points
    public Color Color;
    public int Layer;
}
```

---

### Animation

Sprite animation state.

**Location:** `Kobold.Core.Components.Rendering.Animation`

```csharp
public struct Animation
{
    public Dictionary<string, AnimationClip> Clips;
    public string CurrentClip;
    public int CurrentFrame;
    public float TimeInCurrentFrame;
    public bool IsPlaying;
}
```

**Methods:**
- `Play(string clipName)` - Start playing animation
- `GetCurrentFrameRect()` - Get current frame's source rectangle
- `GetCurrentClip()` - Get current AnimationClip

**Example:**
```csharp
var spriteSheet = Assets.LoadSpriteSheet("player.json");

var animation = new Animation();
animation.Clips["walk"] = spriteSheet.CreateAnimationClip("walk");
animation.Clips["idle"] = spriteSheet.CreateAnimationClip("idle");
animation.Play("idle");

World.Create(
    new Transform { Position = new Vector2(400, 300) },
    SpriteRenderer.FromSpriteSheet(texture, spriteSheet.GetFrame(0)),
    animation
);
```

---

### AnimationClip

Animation definition (frames, timing, looping).

**Location:** `Kobold.Core.Components.Rendering.AnimationClip`

```csharp
public struct AnimationClip
{
    public string Name;
    public Rectangle[] Frames;
    public float FrameDuration;
    public bool Loop;
}
```

**Static Factories:**
- `FromSpriteSheet(SpriteSheet, string animationName)`
- `FromGrid(gridWidth, gridHeight, frameWidth, frameHeight, ...)`

**Example:**
```csharp
var walkClip = AnimationClip.FromGrid(
    gridWidth: 8,
    gridHeight: 1,
    frameWidth: 32,
    frameHeight: 32,
    startFrame: 0,
    frameCount: 8,
    frameDuration: 0.1f,
    loop: true
);
```

---

## Camera

Viewport camera for scrolling/following.

**Location:** `Kobold.Core.Components.Core.Camera`

```csharp
public struct Camera
{
    public Vector2 Position;
    public float ViewportWidth;
    public float ViewportHeight;
    public float SmoothSpeed;
    public Vector2? MinBounds;
    public Vector2? MaxBounds;
    public Entity? FollowTarget;
}
```

**Methods:**
- `WorldToScreen(Vector2)` - Convert world to screen coordinates
- `ScreenToWorld(Vector2)` - Convert screen to world coordinates
- `GetViewportBounds()` - Get camera bounds rectangle
- `SetBounds(Vector2 min, Vector2 max)` - Constrain camera

**Example:**
```csharp
// Camera that follows player
var camera = World.Create(
    new Camera
    {
        Position = Vector2.Zero,
        ViewportWidth = 800,
        ViewportHeight = 600,
        SmoothSpeed = 5f,
        FollowTarget = playerEntity,
        MinBounds = new Vector2(0, 0),
        MaxBounds = new Vector2(2000, 1500)
    }
);
```

---

## Gameplay Components

### Tags

String tags for categorizing entities.

**Location:** `Kobold.Core.Components.Core.Tags`

```csharp
public struct Tags
{
    public string[] Values;
}
```

**Example:**
```csharp
World.Create(
    new Transform { Position = new Vector2(100, 100) },
    new Tags { Values = new[] { "Enemy", "Flying", "Boss" } }
);

// Query entities with tag
var enemies = World.Query<Transform, Tags>()
    .Where(e =>
    {
        var tags = e.Get<Tags>();
        return tags.Values.Contains("Enemy");
    });
```

---

### Lifetime

Automatically destroys entity after time expires.

**Location:** `Kobold.Core.Components.Gameplay.Lifetime`

```csharp
public struct Lifetime
{
    public float TimeRemaining;  // Seconds
}
```

**Example:**
```csharp
// Particle that lives for 2 seconds
World.Create(
    new Transform { Position = new Vector2(300, 300) },
    new RectangleRenderer { Size = new Vector2(5, 5), Color = Color.Yellow },
    new Lifetime { TimeRemaining = 2.0f }
);
```

---

### PlayerControlled

Marker component for player-controlled entities.

**Location:** `Kobold.Core.Components.Gameplay.PlayerControlled`

```csharp
public struct PlayerControlled { }
```

**Example:**
```csharp
var player = World.Create(
    new Transform { Position = new Vector2(400, 300) },
    SpriteRenderer.FullTexture(texture),
    new PlayerControlled()
);

// Query only player entities
var players = World.Query<Transform, Velocity, PlayerControlled>();
```

---

### ScreenBounds

Defines screen boundaries for BoundarySystem.

**Location:** `Kobold.Core.Components.Gameplay.ScreenBounds`

```csharp
public struct ScreenBounds
{
    public float MinX, MaxX, MinY, MaxY;
}
```

**Example:**
```csharp
// Usually created once at game start
World.Create(new ScreenBounds
{
    MinX = 0,
    MaxX = 800,
    MinY = 0,
    MaxY = 600
});
```

---

### GameState

Global game state storage.

**Location:** `Kobold.Core.Components.Core.GameState`

```csharp
public struct GameState
{
    public GameStateType State;
    // Add your game-specific fields
}

public enum GameStateType
{
    Menu,
    Playing,
    Paused,
    GameOver
}
```

**Example:**
```csharp
// Create singleton game state entity
var gameState = World.Create(new GameState
{
    State = GameStateType.Playing
});

// Access from anywhere
var query = World.Query<GameState>();
ref var state = ref query.First().Get<GameState>();
state.State = GameStateType.Paused;
```

---

### PendingDestruction

Marks entity for removal at end of frame.

**Location:** `Kobold.Core.Components.Gameplay.PendingDestruction`

```csharp
public struct PendingDestruction { }
```

**Example:**
```csharp
// Mark for destruction (safer than immediate World.Destroy)
World.Add(entity, new PendingDestruction());
// DestructionSystem will remove it at the end of the frame
```

---

### CustomBoundaryBehavior

Custom screen boundary handling.

**Location:** `Kobold.Core.Components.Gameplay.CustomBoundaryBehavior`

```csharp
public struct CustomBoundaryBehavior
{
    public BoundaryBehavior Behavior;
}

public enum BoundaryBehavior
{
    None,     // Do nothing
    Wrap,     // Wrap to opposite side
    Clamp,    // Stop at edge
    Bounce,   // Bounce off edge
    Destroy   // Destroy entity
}
```

---

## Render Layers

Constants for layered rendering:

```csharp
public static class RenderLayers
{
    public const int Background = 0;
    public const int GameObjects = 100;
    public const int UI = 200;
    public const int Debug = 300;
}
```

Lower numbers render first (behind).

---

## Collision Layers

Enum for collision filtering:

```csharp
public enum CollisionLayer
{
    Default = 0,
    Player = 1,
    Enemy = 2,
    Projectile = 3,
    Environment = 4,
    PlayerProjectile = 5,
    EnemyProjectile = 6,
    Pickup = 7,
    Trigger = 8
}
```

Use with CollisionSystem and CollisionMatrix.

---

## See Also

- **[Systems](systems.md)** - Systems that use these components
- **[Architecture](architecture.md)** - Understanding ECS
- **[Code Snippets](../examples/code-snippets.md)** - Component usage examples

---

**Next:** Learn about [Systems](systems.md) that process these components →
