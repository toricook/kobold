---
title: GameEngineBase
nav_order: 1
parent: Core
---

# GameEngineBase

`GameEngineBase` is the abstract base class that every Kobold game extends. 

It's an implementation of the `IGameEngine`, which defines the basic definition of what makes a game. A game is initialized once at the beginning and shut down once at the end, and in between, the game runs. While it runs, there is a loop. In every iteration of the loop, the state of everything that exists in the game (the "entities") is updated and then the current state of the game is rendered. The update loop requires knowledge of how much time has passed since the last update was called (the "delta time") since update loops may run at different rates on different hardware.

In other words, IGameEngine is

```csharp
public interface IGameEngine
{
    void Initialize();
    void Update(float deltaTime);
    void Render();
    void Shutdown();
}
```

The base class contains singletons that all games must have. These are:

- **World** - The ECS 
- **IRenderer** - Dependency injection for the thing responsible for drawing stuff to the screen
- **IInputManager** - Dependency injection for the thing responsible for reading inputs from attached hardware
- **IContentLoader** - Dependency injection for the thing responsible for getting game content (textures, sounds, etc.)
- **AssetManager** - Singleton that keeps track of all assets and handles loading and caching them
- **EventBus** - Singleton that allows publishing and subscribing to messages 
- **SystemManager** - Singleton that keeps track of all systems and exposes a method to update all of them

