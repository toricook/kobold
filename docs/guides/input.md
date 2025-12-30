# Input Handling Guide

Keyboard and mouse input patterns for player controls.

## Basic Input

```csharp
protected override void Update(float deltaTime)
{
    base.Update(deltaTime);

    // Key pressed (once per press)
    if (InputManager.IsKeyPressed(KeyCode.Space))
    {
        FireWeapon();
    }

    // Key held (continuous)
    if (InputManager.IsKeyDown(KeyCode.W))
    {
        MoveForward();
    }
}
```

## Player Movement Patterns

### 8-Direction Movement (Top-Down)

```csharp
var players = World.Query<Transform, Velocity, PlayerControlled>();
foreach (var player in players)
{
    ref var velocity = ref player.Get<Velocity>();

    var moveDir = Vector2.Zero;
    if (InputManager.IsKeyDown(KeyCode.W)) moveDir.Y -= 1;
    if (InputManager.IsKeyDown(KeyCode.S)) moveDir.Y += 1;
    if (InputManager.IsKeyDown(KeyCode.A)) moveDir.X -= 1;
    if (InputManager.IsKeyDown(KeyCode.D)) moveDir.X += 1;

    if (moveDir != Vector2.Zero)
    {
        velocity.Value = Vector2.Normalize(moveDir) * 200f;
    }
    else
    {
        velocity.Value = Vector2.Zero;
    }
}
```

### Platformer Controls

```csharp
var moveSpeed = 200f;
var jumpForce = -400f;

if (InputManager.IsKeyDown(KeyCode.A))
    velocity.Value = new Vector2(-moveSpeed, velocity.Value.Y);
else if (InputManager.IsKeyDown(KeyCode.D))
    velocity.Value = new Vector2(moveSpeed, velocity.Value.Y);
else
    velocity.Value = new Vector2(0, velocity.Value.Y);  // Stop horizontal

if (InputManager.IsKeyPressed(KeyCode.Space) && isGrounded)
{
    velocity.Value = new Vector2(velocity.Value.X, jumpForce);
}
```

### Tank Controls (Rotation + Thrust)

```csharp
ref var transform = ref player.Get<Transform>();
ref var velocity = ref player.Get<Velocity>();

var rotSpeed = 3f;
var thrustForce = 300f;

// Rotate
if (InputManager.IsKeyDown(KeyCode.A))
    transform.Rotation -= rotSpeed * deltaTime;
if (InputManager.IsKeyDown(KeyCode.D))
    transform.Rotation += rotSpeed * deltaTime;

// Thrust forward
if (InputManager.IsKeyDown(KeyCode.W))
{
    velocity.Value += transform.Forward * thrustForce * deltaTime;
}
```

## Mouse Input

### Click to Spawn

```csharp
if (InputManager.IsMouseButtonPressed(MouseButton.Left))
{
    var mousePos = InputManager.GetMousePosition();
    SpawnEntityAt(mousePos);
}
```

### Aim at Mouse

```csharp
var mousePos = InputManager.GetMousePosition();
var playerPos = transform.Position;

var direction = Vector2.Normalize(mousePos - playerPos);
transform.Rotation = MathF.Atan2(direction.Y, direction.X);
```

## See Also

- **[InputManager](../core/game-engine.md#platform-implementations)** - API reference
- **[Input System](../core/systems.md#inputsystem)** - Custom input systems

---

**Back:** [Guides](index.md) ←
