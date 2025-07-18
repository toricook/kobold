# Building Pong with Kobold ECS

A step-by-step walkthrough of how this Pong game was created using the Kobold ECS core engine and MonoGame rendering backend.

## Step 1: Define Game-Specific Components

The core library provides some common components. This file defines additional components specific to Pong.

```csharp
// Pong/Components.cs
public struct Paddle
{
    public float Speed;
    public bool IsPlayer; // true for player, false for AI
}

public struct Ball
{
    public float Speed;
}

public struct Score
{
    public int PlayerScore;
    public int AIScore;
}
```

## Step 2: Create Game-Specific Events

Similar to the components, we define Pong-specific events.

```csharp
// Pong/Events.cs
public class PlayerScoredEvent : BaseEvent
{
    public int PlayerId { get; }
    public int NewScore { get; }
}

public class BallResetEvent : BaseEvent
{
    public float InitialSpeedX { get; }
    public float InitialSpeedY { get; }
}

public class GameOverEvent : BaseEvent
{
    public int WinningPlayerId { get; }
    public int WinnerScore { get; }
    public int LoserScore { get; }
}

public class GameRestartEvent : BaseEvent { }
```

## Step 3: Set Up the Main Game Class

The `PongGame` class inherits from the `GameEngineBase`. The base class contains all of the things that a game needs to work -- renderer, input manager, system manager, etc.

```csharp
// Pong/PongGame.cs
public class PongGame : GameEngineBase
{
    private const float SCREEN_WIDTH = 800f;
    private const float SCREEN_HEIGHT = 600f;
    // ... other constants

    private Entity _playerPaddle;
    private Entity _aiPaddle;
    private Entity _ball;
    private Entity _scoreDisplay;
    private Entity _gameStateEntity;
}
```

## Step 4: Create Game Entities

The game class `Initialize()` method has to create all of the entities that the game needs:

```csharp
// Create player paddle entity
_playerPaddle = World.Create(
    new Transform(new Vector2(50f, SCREEN_HEIGHT / 2 - PADDLE_HEIGHT / 2)),
    new Velocity(Vector2.Zero),
    new Paddle(PADDLE_SPEED, isPlayer: true),
    PlayerControlled.CreateVerticalOnly(PADDLE_SPEED), // Core component
    new BoxCollider(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT)),
    RectangleRenderer.GameObject(new Vector2(PADDLE_WIDTH, PADDLE_HEIGHT), Color.White)
);

// Create ball entity  
_ball = World.Create(
    new Transform(centerPosition),
    new Velocity(initialVelocity),
    new Ball(BALL_SPEED),
    new BoxCollider(new Vector2(BALL_SIZE, BALL_SIZE)),
    RectangleRenderer.GameObject(new Vector2(BALL_SIZE, BALL_SIZE), Color.White)
);
```


## Step 5: Extend the Core Game State System

The Pong-specific game state system extends the core system and handles configuring all of the states and their transitions.

```csharp
// Pong/Systems/PongGameStateSystem.cs
public class PongGameStateSystem : GameStateSystem<GameState>, IEventHandler<GameOverEvent>
{
    public PongGameStateSystem(World world, EventBus eventBus, IInputManager inputManager)
        : base(world, eventBus, inputManager)
    {
        ConfigureGameStates();
        eventBus.Subscribe<GameOverEvent>(this);
    }

    private void ConfigureGameStates()
    {
        // Configure state transitions and input mappings
        ConfigureState(new GameState(GameStateType.GameOver), new StateConfig
        {
            InputTransitions = new List<InputTransition>
            {
                new InputTransition
                {
                    Key = KeyCode.Space,
                    NextState = new GameState(GameStateType.Playing),
                    OnTransition = RestartGame
                }
            },
            OnStateEnter = OnGameOverEnter
        });
    }
}
```


## Step 6: Implement Pong-Specific Systems

### AI System
The system that controls the AI paddle is Pong specific.

```csharp
// Pong/Systems/AISystem.cs  
public class AISystem : ISystem
{
    public void Update(float deltaTime)
    {
        // Find ball position
        Vector2 ballPosition = GetBallPosition();
        
        // Move AI paddle towards ball
        var paddleQuery = new QueryDescription().WithAll<Paddle, Transform, Velocity>();
        World.Query(in paddleQuery, (ref Paddle paddle, ref Transform transform, ref Velocity velocity) =>
        {
            if (!paddle.IsPlayer) // Only move AI paddle
            {
                float difference = ballPosition.Y - transform.Position.Y;
                velocity.Value = new Vector2(0, Math.Sign(difference) * paddle.Speed * 0.8f);
            }
        });
    }
}
```

### Collision Handler
The Pong-specific collision handler listens to collision events between the ball and the paddle, which are raised by the base CollisionSystem, but it also has some Pong-specific collision logic like how to handle ball-wall collisions.

```csharp
// Pong/Systems/PongCollisionHandler.cs
public class PongCollisionHandler : ISystem, IEventHandler<CollisionEvent>
{
    public void Handle(CollisionEvent eventData)
    {
        // Check if ball hit paddle
        if (IsBallPaddleCollision(eventData.Entity1, eventData.Entity2, out var ball, out var paddle))
        {
            // Reverse ball direction
            var ballVelocity = World.Get<Velocity>(ball);
            ballVelocity.Value = new Vector2(-ballVelocity.Value.X, ballVelocity.Value.Y);
        }
    }
    
    private void HandleBallWallCollisions()
    {
        // Detect scoring when ball goes off screen
        if (ballPosition.X <= 0)
        {
            EventBus.Publish(new PlayerScoredEvent(playerId: 2, newScore: 0));
        }
    }
}
```

### Score System
There is also a dedicated system for managing scoring, since that is game specific.

```csharp
// Pong/Systems/ScoreSystem.cs
public class ScoreSystem : ISystem, IEventHandler<PlayerScoredEvent>, IEventHandler<GameRestartEvent>
{
    public void Handle(PlayerScoredEvent eventData)
    {
        // Update score
        UpdateScore(eventData.PlayerId);
        
        // Check for win condition
        if (PlayerScore >= WinningScore)
        {
            EventBus.Publish(new GameOverEvent(1, PlayerScore, AIScore));
        }
    }
}
```

## Step 7: Create Event-Driven UI System

The Pong UI system is set up to listen for game state changed events and display the proper UI elements.

```csharp
// Pong/Systems/PongUISystem.cs
public class PongUISystem : ISystem, IEventHandler<GameStateChangedEvent<GameState>>
{
    public void Handle(GameStateChangedEvent<GameState> eventData)
    {
        ClearUI();
        
        switch (eventData.NewState.State)
        {
            case GameStateType.GameOver:
                CreateGameOverUI(eventData.NewState);
                break;
            case GameStateType.Paused:
                CreatePausedUI();
                break;
        }
    }
    
    private void CreateGameOverUI(GameState gameState)
    {
        // Create UI entities with high render layer
        var titleEntity = World.Create(
            new Transform(new Vector2(250f, 250f)),
            TextRenderer.UIText("GAME OVER!", Color.Red, 32f)
        );
        _uiEntities.Add(titleEntity);
    }
}
```

## Step 8: Configure System Dependencies

```csharp
// In PongGame.InitializeSystems()
private void InitializeSystems()
{
    // Systems that only run during "Playing" state
    SystemManager.AddSystem(inputSystem, SystemUpdateOrder.INPUT, requiresGameplayState: true);
    SystemManager.AddSystem(physicsSystem, SystemUpdateOrder.PHYSICS, requiresGameplayState: true);
    SystemManager.AddSystem(aiSystem, SystemUpdateOrder.AI, requiresGameplayState: true);
    
    // Systems that always run
    SystemManager.AddSystem(gameStateSystem, SystemUpdateOrder.UI, requiresGameplayState: false);
    SystemManager.AddSystem(pongUISystem, SystemUpdateOrder.UI + 1, requiresGameplayState: false);
    
    // Render system always runs
    SystemManager.AddRenderSystem(renderSystem);
}
```

This creates the pause behavior automatically - when the game state isn't "Playing", gameplay systems stop updating but UI and state management continue.

## Step 9: Create the entry point
The program entry point needs to create the game engine and pass it into the MonoGame host. This is where the IRenderer and IInputManager become actualized and handled by MonoGame.

```csharp
public static void Main()
{
    var pongGame = new PongGame(); 
    using var host = new MonoGameHost(pongGame);
    host.Run();
}
```
