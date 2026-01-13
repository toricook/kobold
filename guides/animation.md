# Animation System

Kobold's animation system supports sprite sheet animations with flexible layouts and automatic directional control.

## Architecture

The animation system has two parts:

**AnimationSystem** - Frame-by-frame playback engine. Advances frames, handles looping, updates sprite renderer.

**Controller Systems** - Decide which animation to play based on game state:
- `DirectionalAnimationControllerSystem` - Switches animations based on movement direction
- Create custom controllers for combat, cutscenes, state machines, etc.

## Quick Start

### 1. Prepare Your Sprite Sheet

Create a uniform grid sprite sheet (PNG) and place it in your `Assets` folder.

**Example:** `player.png` - 640×32 pixels (20 columns × 1 row of 32×32 sprites)

### 2. Create Configuration File

Create `player.json` alongside your sprite sheet:

```json
{
  "spriteWidth": 32,
  "spriteHeight": 32,
  "columns": 20,
  "rows": 1,
  "spacing": 0,
  "margin": 0,
  "pivot": { "x": 0.5, "y": 1.0 },
  "animations": {
    "walk_up": { "startFrame": 0, "frameCount": 5, "fps": 10, "loop": true },
    "walk_down": { "startFrame": 5, "frameCount": 5, "fps": 10, "loop": true },
    "walk_right": { "startFrame": 10, "frameCount": 5, "fps": 10, "loop": true },
    "walk_left": { "startFrame": 15, "frameCount": 5, "fps": 10, "loop": true }
  }
}
```

**Frame indexing:** Frames are numbered left-to-right, top-to-bottom starting at 0.

**Pivot:** (0.5, 1.0) = bottom-center, good for top-down characters where feet touch ground.

### 3. Register Systems

```csharp
public override void Initialize()
{
    // Required: Frame playback
    SystemManager.AddSystem(new AnimationSystem(World), SystemUpdateOrder.GAME_LOGIC);

    // Optional: Automatic directional animation
    SystemManager.AddSystem(
        new DirectionalAnimationControllerSystem(World),
        SystemUpdateOrder.GAME_LOGIC
    );
}
```

### 4. Create Animated Entity

```csharp
// Load sprite sheet
var playerSheet = Assets.LoadSpriteSheet("player");

// Build animation component from config
var animation = AnimationBuilder.Create()
    .WithSpriteSheet(playerSheet)
    .AddAllAnimationsFromConfig()
    .Build();

animation.Play("idle");

// Create sprite renderer
var initialFrame = playerSheet.GetFrame(0);
var sprite = SpriteRenderer.FromSpriteSheet(
    playerSheet.Texture,
    initialFrame,
    Vector2.One,
    ySort: true
);

// Create entity
var player = World.Create(
    new Transform { Position = new Vector2(100, 100) },
    new Velocity(Vector2.Zero),
    sprite,
    animation,
    DirectionalAnimation.FourWay(
        up: "walk_up",
        down: "walk_down",
        left: "walk_left",
        right: "walk_right",
        idle: "idle",
        minimumSpeed: 10f
    )
);
```

The player will now animate automatically when moving!

## Sprite Sheet Layouts

### Layout 1: Single Row (All Animations)

Best for simple characters or when you have many short animations.

```
[walk0][walk1][walk2][walk3][jump0][jump1][jump2][attack0][attack1]...
```

```json
{
  "columns": 20,
  "rows": 1,
  "animations": {
    "walk": { "startFrame": 0, "frameCount": 4, "fps": 10 },
    "jump": { "startFrame": 4, "frameCount": 3, "fps": 12 },
    "attack": { "startFrame": 7, "frameCount": 5, "fps": 15 }
  }
}
```

### Layout 2: Row Per Animation

Best for organized sheets with directional animations.

```
Row 0: [up0][up1][up2][up3]
Row 1: [down0][down1][down2][down3]
Row 2: [left0][left1][left2][left3]
Row 3: [right0][right1][right2][right3]
```

```json
{
  "columns": 4,
  "rows": 4,
  "animations": {
    "walk_up": { "row": 0, "startColumn": 0, "frameCount": 4, "fps": 10 },
    "walk_down": { "row": 1, "startColumn": 0, "frameCount": 4, "fps": 10 },
    "walk_left": { "row": 2, "startColumn": 0, "frameCount": 4, "fps": 10 },
    "walk_right": { "row": 3, "startColumn": 0, "frameCount": 4, "fps": 10 }
  }
}
```

### Layout 3: Mixed Layout

Multiple animations per row, different frame counts.

```
Row 0: [idle0][idle1][walk0][walk1][walk2][walk3][walk4][walk5]...
```

```json
{
  "animations": {
    "idle": { "startFrame": 0, "frameCount": 2, "fps": 5 },
    "walk": { "startFrame": 2, "frameCount": 6, "fps": 10 }
  }
}
```

### Layout 4: Non-Contiguous Frames

Cherry-pick specific frames for complex animations.

```json
{
  "animations": {
    "blink": {
      "frameIndices": [0, 0, 0, 1, 0],
      "fps": 8,
      "loop": true
    }
  }
}
```

## Animation Builder

The `AnimationBuilder` provides a fluent API for creating animations programmatically.

```csharp
var animation = AnimationBuilder.Create()
    .WithSpriteSheet(sheet)

    // Add animations in various ways
    .AddAnimation("walk", startFrame: 0, frameCount: 4, fps: 10)
    .AddAnimation("jump", frames: 4..6, fps: 12)  // Range syntax
    .AddAnimation("blink", frameIndices: new[] { 0, 0, 1, 0 }, fps: 8)
    .AddAnimationFromRow("walk_up", row: 0, startCol: 0, frameCount: 4, fps: 10)

    // Helper for common patterns
    .AddFourWayWalking(
        upRow: 0, downRow: 1, leftRow: 2, rightRow: 3,
        framesPerDirection: 4,
        fps: 10
    )

    // Load from config
    .AddAnimationFromConfig("walk")
    .AddAllAnimationsFromConfig()

    .Build();
```

## Directional Animation

The `DirectionalAnimationControllerSystem` automatically switches animations based on movement direction.

### Four-Way Movement

```csharp
var directional = DirectionalAnimation.FourWay(
    up: "walk_up",
    down: "walk_down",
    left: "walk_left",
    right: "walk_right",
    idle: "idle",
    minimumSpeed: 10f,
    priority: DirectionPriority.Horizontal  // Prefer horizontal over vertical
);
```

**Requires:**
- `Velocity` component (magnitude determines if moving)
- `Animation` component (contains animation clips)
- `DirectionalAnimation` component (configuration)

When velocity magnitude < `minimumSpeed`, plays idle animation.

When moving, plays the animation matching the dominant direction.

### Eight-Way Movement

```csharp
var directional = DirectionalAnimation.EightWay(
    up: "walk_up",
    down: "walk_down",
    left: "walk_left",
    right: "walk_right",
    upLeft: "walk_up_left",
    upRight: "walk_up_right",
    downLeft: "walk_down_left",
    downRight: "walk_down_right",
    idle: "idle",
    minimumSpeed: 10f
);
```

Falls back to 4-way animations if 8-way not provided (e.g., `upLeft` → `left` → `up`).

## Manual Animation Control

For combat, cutscenes, or complex state machines, control animations manually:

```csharp
// Play animation
animation.Play("attack");
animation.Play("jump", restart: true);  // Force restart even if already playing

// Query state
var currentClip = animation.GetCurrentClip();
var currentFrame = animation.GetCurrentFrameRect();
bool isPlaying = animation.IsPlaying;
int frameIndex = animation.CurrentFrame;

// Pause/resume
animation.IsPlaying = false;
animation.IsPlaying = true;
```

## Creating Custom Controllers

For game-specific animation logic, create custom controller systems:

```csharp
public class CombatAnimationControllerSystem : ISystem
{
    private readonly World _world;

    public CombatAnimationControllerSystem(World world)
    {
        _world = world;
    }

    public void Update(float deltaTime)
    {
        var query = new QueryDescription()
            .WithAll<Animation, CombatState>();

        _world.Query(in query, (ref Animation anim, ref CombatState combat) =>
        {
            // Decide which animation to play based on combat state
            if (combat.IsAttacking)
                anim.Play("attack");
            else if (combat.IsBlocking)
                anim.Play("block");
            else if (combat.IsHurt)
                anim.Play("hurt");
        });
    }
}
```

**Best practices:**
- One controller per game system (combat, movement, cutscenes)
- Controllers can be enabled/disabled independently
- Higher priority controllers may override lower priority ones
- Use `restart: false` to avoid resetting animations unnecessarily

## Configuration Reference

### SpriteSheetConfig Properties

| Property | Type | Description |
|----------|------|-------------|
| `spriteWidth` | int | Width of each sprite in pixels |
| `spriteHeight` | int | Height of each sprite in pixels |
| `columns` | int | Number of columns in the sprite sheet |
| `rows` | int | Number of rows in the sprite sheet |
| `spacing` | int | Space between sprites in pixels (default: 0) |
| `margin` | int | Margin around entire sheet in pixels (default: 0) |
| `pivot` | Vector2 | Pivot point for sprites (0,0=top-left, 0.5,0.5=center, 1,1=bottom-right) |

### AnimationData Properties

| Property | Type | Description |
|----------|------|-------------|
| `startFrame` | int | Starting frame index (linear counting) |
| `frameCount` | int | Number of frames in animation |
| `fps` | float | Frames per second playback speed |
| `loop` | bool | Whether animation loops (default: true) |
| `row` | int? | Alternative: specify row instead of startFrame |
| `startColumn` | int? | Used with `row` to specify starting column |
| `frameIndices` | int[]? | Alternative: custom frame sequence (overrides startFrame) |

## Tips

- **Pivot point (0.5, 1.0)** - Bottom-center, good for top-down characters (feet at bottom)
- **Pivot point (0.5, 0.5)** - Center, good for platformer characters or centered rotation
- **Frame indices** - Always start at 0, counted left-to-right, top-to-bottom
- **FPS values** - 10 fps is common for pixel art walking, 15+ fps for smooth actions
- **Minimum speed** - Set to ~10-20 pixels/second to avoid jittery idle/walk transitions
- **Y-sorting** - Set `ySort: true` on sprites in top-down games for correct depth
- **Controller order** - DirectionalAnimationControllerSystem should run before AnimationSystem
