# Animation System Setup

Quick guide for setting up sprite sheet animations in Kobold.

## Sprite Sheet Requirements

Your sprite sheet must be a uniform grid:

```
Assets/
├── character.png     # Sprite sheet texture
└── character.json    # Animation configuration
```

## Configuration File

Create a JSON config alongside your sprite sheet:

```json
{
  "spriteWidth": 32,
  "spriteHeight": 32,
  "columns": 10,
  "rows": 4,
  "spacing": 0,
  "margin": 0,
  "pivot": {
    "x": 0.5,
    "y": 1.0
  },
  "animations": {
    "walk_up": {
      "startFrame": 0,
      "frameCount": 4,
      "fps": 10,
      "loop": true
    },
    "walk_down": {
      "startFrame": 4,
      "frameCount": 4,
      "fps": 10,
      "loop": true
    }
  }
}
```

**Frame indexing:** Left-to-right, top-to-bottom. Frame 0 = top-left corner.

**Pivot point:** (0.5, 1.0) = bottom-center, good for top-down characters.

## Load and Setup

```csharp
// Load sprite sheet
var sheet = Assets.LoadSpriteSheet("character");

// Build animation component
var animation = AnimationBuilder.Create()
    .WithSpriteSheet(sheet)
    .AddAllAnimationsFromConfig()  // Load all from JSON
    .Build();

// Or add animations manually
var animation = AnimationBuilder.Create()
    .WithSpriteSheet(sheet)
    .AddAnimation("walk", startFrame: 0, frameCount: 4, fps: 10)
    .AddAnimation("jump", frameIndices: new[] { 10, 11, 12 }, fps: 15)
    .Build();

// Create sprite renderer
var frame = sheet.GetFrame(0);
var sprite = SpriteRenderer.FromSpriteSheet(sheet.Texture, frame, Vector2.One);

// Create entity
world.Create(
    new Transform { Position = new Vector2(100, 100) },
    sprite,
    animation
);
```

## Directional Animations

For automatic animation switching based on movement:

```csharp
world.Create(
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

**Requires:** `DirectionalAnimationControllerSystem` registered in your game.

## Systems Required

```csharp
// In your game's Initialize():
SystemManager.AddSystem(new AnimationSystem(World), SystemUpdateOrder.GAME_LOGIC);

// Optional: For directional animation
SystemManager.AddSystem(new DirectionalAnimationControllerSystem(World), SystemUpdateOrder.GAME_LOGIC);
```

**AnimationSystem** - Plays animations (frame-by-frame playback)
**DirectionalAnimationControllerSystem** - Switches animations based on velocity

## Sprite Sheet Layouts

### Single Row
```json
{
  "columns": 20,
  "rows": 1,
  "animations": {
    "walk": { "startFrame": 0, "frameCount": 4 },
    "jump": { "startFrame": 4, "frameCount": 3 },
    "attack": { "startFrame": 7, "frameCount": 5 }
  }
}
```

### Row Per Animation
```json
{
  "columns": 4,
  "rows": 4,
  "animations": {
    "walk_up": { "row": 0, "startColumn": 0, "frameCount": 4 },
    "walk_down": { "row": 1, "startColumn": 0, "frameCount": 4 },
    "walk_left": { "row": 2, "startColumn": 0, "frameCount": 4 },
    "walk_right": { "row": 3, "startColumn": 0, "frameCount": 4 }
  }
}
```

### Custom Frame Sequence
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

## Builder Methods

```csharp
// Linear frame range
.AddAnimation("walk", frames: 0..7, fps: 10)
.AddAnimation("walk", startFrame: 0, frameCount: 8, fps: 10)

// Arbitrary frames
.AddAnimation("blink", frameIndices: new[] { 0, 0, 1, 0 }, fps: 8)

// Row-based
.AddAnimationFromRow("walk_up", row: 0, startCol: 0, frameCount: 4, fps: 10)

// Helper for 4-way movement
.AddFourWayWalking(
    upRow: 0, downRow: 1, leftRow: 2, rightRow: 3,
    framesPerDirection: 4,
    fps: 10
)

// From config
.AddAnimationFromConfig("walk_up")
.AddAllAnimationsFromConfig()
```

## Playing Animations

```csharp
// Manual control
animation.Play("walk");
animation.Play("jump", restart: true);  // Force restart

// Query current state
var clip = animation.GetCurrentClip();
var frame = animation.GetCurrentFrameRect();
bool playing = animation.IsPlaying;
```

## Tips

- Use bottom-center pivot (0.5, 1.0) for top-down characters
- Use center pivot (0.5, 0.5) for platformer characters
- Frame count starts at 1, frame indices start at 0
- DirectionalAnimation requires Velocity component
- Multiple animations per row: use `startFrame` and count frames sequentially
