# Camera Systems Guide

Camera following, bounds, smooth movement, and viewport management.

## Basic Camera

```csharp
var camera = World.Create(
    new Camera
    {
        Position = Vector2.Zero,
        ViewportWidth = 800,
        ViewportHeight = 600
    }
);
```

## Follow Player

```csharp
var camera = World.Create(
    new Camera
    {
        Position = Vector2.Zero,
        ViewportWidth = 800,
        ViewportHeight = 600,
        FollowTarget = playerEntity,  // Entity to follow
        SmoothSpeed = 5f              // Higher = faster following
    }
);
```

## Camera with Bounds

```csharp
var camera = World.Create(
    new Camera
    {
        Position = Vector2.Zero,
        ViewportWidth = 800,
        ViewportHeight = 600,
        FollowTarget = playerEntity,
        MinBounds = new Vector2(0, 0),
        MaxBounds = new Vector2(2000, 1500)  // Map size
    }
);
```

## Manual Camera Control

```csharp
var query = World.Query<Camera>();
ref var camera = ref query.First().Get<Camera>();

// Move camera
camera.Position += new Vector2(10, 0) * deltaTime;

// Center on position
camera.Position = targetPosition - new Vector2(camera.ViewportWidth / 2, camera.ViewportHeight / 2);
```

## Coordinate Conversion

```csharp
// World to screen
var screenPos = camera.WorldToScreen(worldPosition);

// Screen to world (e.g., for mouse clicks)
var worldPos = camera.ScreenToWorld(mousePosition);
```

## See Also

- **[Camera Component](../core/components.md#camera)** - Full API reference
- **[RenderSystem](../core/systems.md#rendersystem)** - Camera integration

---

**Back:** [Guides](index.md) ←
