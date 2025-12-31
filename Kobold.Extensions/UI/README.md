# Kobold UI Framework

A simple, wireframe-first UI system for the Kobold game framework. Build interactive menus, buttons, and panels quickly with basic shapes, then easily reskin them with sprites later.

## Features

- **Interactive Components**: Buttons with hover, pressed, and disabled states
- **Wireframe Mode**: Quick prototyping with colored rectangles
- **Event-Driven**: React to UI interactions via EventBus
- **Screen Anchoring**: Position UI relative to screen edges or center
- **Reskinnable**: Easy transition from wireframes to sprite-based UI
- **ECS-Based**: Fully integrated with Kobold's entity component system

## Quick Start

### 1. Setup Systems

Add the UI systems to your game's system manager:

```csharp
using Kobold.Extensions.UI.Systems;
using Kobold.Extensions.UI.Events;

// In your game initialization
var uiInputSystem = new UIInputSystem(world, inputManager, eventBus);
var uiButtonSystem = new UIButtonSystem(world, eventBus);
var uiAnchorSystem = new UIAnchorSystem(world, new Vector2(800, 600)); // Your screen size

systemManager.AddSystem(uiInputSystem);
systemManager.AddSystem(uiButtonSystem);
systemManager.AddSystem(uiAnchorSystem);
```

### 2. Create a Simple Button

```csharp
using Kobold.Extensions.UI;
using Kobold.Extensions.UI.Events;

// Create a button
var playButton = UIFactory.CreateButton(
    world,
    position: new Vector2(300, 200),
    size: new Vector2(200, 50),
    text: "Play Game",
    normalColor: UIColorScheme.Dark.ButtonNormal,
    buttonId: "play_button"
);

// Listen for button clicks
eventBus.Subscribe<UIButtonClickedEvent>(e =>
{
    if (e.ButtonId == "play_button")
    {
        Console.WriteLine("Play button clicked!");
        StartGame();
    }
});
```

### 3. Create Anchored UI

```csharp
// Button centered on screen
var settingsButton = UIFactory.CreateAnchoredButton(
    world,
    anchorPoint: AnchorPoint.Center,
    offset: new Vector2(0, 60),  // 60 pixels below center
    size: new Vector2(200, 50),
    text: "Settings",
    normalColor: UIColorScheme.Dark.ButtonNormal,
    buttonId: "settings"
);

// Panel in top-right corner
var scorePanel = UIFactory.CreateAnchoredPanel(
    world,
    anchorPoint: AnchorPoint.TopRight,
    offset: new Vector2(-10, 10),  // 10 pixels from corner
    size: new Vector2(200, 100),
    backgroundColor: UIColorScheme.Dark.Panel
);
```

## Components

### UIBounds
Defines the interactive hit area for UI elements.

```csharp
// Manual creation
world.Add(entity, new UIBounds(
    size: new Vector2(200, 50),
    offset: Vector2.Zero
));

// Centered bounds
world.Add(entity, UIBounds.Centered(new Vector2(200, 50)));
```

### UIInteractive
Tracks interaction state - hover, pressed, clicked.

```csharp
// Add to make an entity interactive
world.Add(entity, new UIInteractive(isEnabled: true));

// Check state in systems
if (interactive.IsHovered) { /* ... */ }
if (interactive.WasClicked) { /* ... */ }

// Disable temporarily
interactive.IsEnabled = false;
```

### UIButton
Defines button visual states and colors.

```csharp
// Create button with derived colors
var button = UIButton.WithDerivedColors("my_button", Color.Blue);

// Or specify all colors manually
var button = new UIButton(
    id: "my_button",
    normalColor: Color.Gray,
    hoverColor: Color.LightGray,
    pressedColor: Color.DarkGray,
    disabledColor: Color.DimGray
);
```

### UIPanel
Container or background element.

```csharp
// Simple background panel
world.Add(entity, UIPanel.Background(UIColorScheme.Dark.Panel));

// Modal panel that blocks clicks
world.Add(entity, UIPanel.Modal(
    Color.FromArgb(180, 0, 0, 0),  // Semi-transparent black
    order: 100
));
```

### UIAnchor
Positions UI relative to screen edges/center.

```csharp
// Anchor button to bottom-center
UIFactory.AddAnchor(world, buttonEntity, AnchorPoint.BottomCenter, new Vector2(0, -50));

// Update screen size (call when window resizes)
uiAnchorSystem.SetScreenSize(new Vector2(1920, 1080));
```

## Events

Subscribe to UI events via EventBus:

```csharp
// Generic UI click (any interactive element)
eventBus.Subscribe<UIClickEvent>(e =>
{
    Console.WriteLine($"Entity {e.Entity} clicked at {e.MousePosition}");
});

// Button-specific click (includes button ID)
eventBus.Subscribe<UIButtonClickedEvent>(e =>
{
    Console.WriteLine($"Button '{e.ButtonId}' clicked");
});

// Hover enter (useful for tooltips)
eventBus.Subscribe<UIHoverEnterEvent>(e =>
{
    ShowTooltip(e.Entity);
});

// Hover exit
eventBus.Subscribe<UIHoverExitEvent>(e =>
{
    HideTooltip(e.Entity);
});
```

## Color Schemes

Pre-defined color palettes for consistent styling:

```csharp
// Dark theme
UIColorScheme.Dark.ButtonNormal
UIColorScheme.Dark.ButtonHover
UIColorScheme.Dark.Panel
UIColorScheme.Dark.Text
UIColorScheme.Dark.Accent
UIColorScheme.Dark.Success
UIColorScheme.Dark.Warning
UIColorScheme.Dark.Error

// Light theme
UIColorScheme.Light.ButtonNormal
// ... etc

// Blue theme
UIColorScheme.Blue.ButtonNormal
// ... etc
```

## Complete Example: Main Menu

```csharp
public class MainMenuSystem : ISystem
{
    private readonly World _world;
    private readonly EventBus _eventBus;

    public MainMenuSystem(World world, EventBus eventBus)
    {
        _world = world;
        _eventBus = eventBus;

        CreateMainMenu();
        SubscribeToEvents();
    }

    private void CreateMainMenu()
    {
        // Background panel
        var background = UIFactory.CreateAnchoredPanel(
            _world,
            AnchorPoint.Center,
            Vector2.Zero,
            new Vector2(400, 500),
            UIColorScheme.Dark.Panel
        );

        // Title (using TextRenderer directly)
        var title = _world.Create(
            new Transform(Vector2.Zero),
            TextRenderer.UIText("My Game", UIColorScheme.Dark.Text, 32f)
        );
        UIFactory.AddAnchor(_world, title, AnchorPoint.Center, new Vector2(0, -180));

        // Play button
        var playButton = UIFactory.CreateAnchoredButton(
            _world,
            AnchorPoint.Center,
            new Vector2(0, -60),
            new Vector2(250, 50),
            "Play",
            UIColorScheme.Dark.ButtonNormal,
            buttonId: "play"
        );

        // Settings button
        var settingsButton = UIFactory.CreateAnchoredButton(
            _world,
            AnchorPoint.Center,
            new Vector2(0, 0),
            new Vector2(250, 50),
            "Settings",
            UIColorScheme.Dark.ButtonNormal,
            buttonId: "settings"
        );

        // Quit button (using error color for destructive action)
        var quitButton = UIFactory.CreateAnchoredButton(
            _world,
            AnchorPoint.Center,
            new Vector2(0, 60),
            new Vector2(250, 50),
            "Quit",
            UIColorScheme.Dark.Error,
            buttonId: "quit"
        );
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<UIButtonClickedEvent>(OnButtonClicked);
    }

    private void OnButtonClicked(UIButtonClickedEvent e)
    {
        switch (e.ButtonId)
        {
            case "play":
                StartGame();
                break;
            case "settings":
                OpenSettings();
                break;
            case "quit":
                QuitGame();
                break;
        }
    }

    public void Update(float deltaTime)
    {
        // Menu doesn't need per-frame updates
        // UI systems handle interaction automatically
    }
}
```

## Polling vs Events

You can use either event-driven or polling approaches:

### Event-Driven (Recommended)
```csharp
eventBus.Subscribe<UIButtonClickedEvent>(e =>
{
    if (e.ButtonId == "my_button")
        DoSomething();
});
```

### Polling
```csharp
// In your system's Update method
if (_world.Has<UIInteractive>(buttonEntity))
{
    ref var interactive = ref _world.Get<UIInteractive>(buttonEntity);
    if (interactive.WasClicked)
    {
        DoSomething();
    }
}
```

## Reskinning: Wireframes to Sprites

To replace wireframe buttons with sprites later:

1. Remove the `RectangleRenderer` component
2. Add a `SpriteRenderer` component
3. UIButton colors will work as tint multipliers (if implemented)

```csharp
// Wireframe button
var button = UIFactory.CreateButton(world, pos, size, "Click", Color.Blue);

// Later: Convert to sprite
world.Remove<RectangleRenderer>(button);
world.Add(button, new SpriteRenderer(
    texture: buttonTexture,
    sourceRect: new Rectangle(0, 0, 200, 50),
    layer: RenderLayers.UI
));
```

## Advanced: Manual Entity Creation

For full control, create UI entities manually:

```csharp
var customButton = world.Create(
    new Transform(new Vector2(100, 100)),
    new UIBounds(new Vector2(150, 40), Vector2.Zero),
    new UIInteractive { IsEnabled = true },
    new UIButton(
        id: "custom",
        normalColor: Color.Purple,
        hoverColor: Color.Violet,
        pressedColor: Color.DarkViolet,
        disabledColor: Color.Gray
    ),
    RectangleRenderer.UI(new Vector2(150, 40), Color.Purple),
    TextRenderer.UIText("Custom", Color.White, 14f)
);
```

## Tips

1. **Update Order**: Add `UIInputSystem` before `UIButtonSystem` in your system manager
2. **Screen Resize**: Call `uiAnchorSystem.SetScreenSize()` when window resizes
3. **Hit Testing**: UIInputSystem respects render layers - higher layers are checked first
4. **Modal Dialogs**: Use `UIPanel.Modal()` with `blocksClicks: true` to prevent interaction with UI behind it
5. **Performance**: Hit testing is O(n) where n = interactive elements. Keep under ~100 for best performance.

## Future Extensions

Planned features for future versions:
- Layout containers (Horizontal/Vertical stacks)
- Text input fields
- Sliders and checkboxes
- Scroll views
- Nine-slice sprite panels
- Animation/tweening system
