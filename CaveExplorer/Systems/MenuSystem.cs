using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Extensions.SaveSystem;
using Kobold.Extensions.UI;
using Kobold.Extensions.UI.Components;
using Kobold.Extensions.UI.Events;
using System.Drawing;
using System.Numerics;

namespace CaveExplorer.Systems
{
    /// <summary>
    /// System that manages the game's menu UI.
    /// Creates menu when entering Menu state, handles button clicks,
    /// and manages save/load functionality.
    /// </summary>
    public class MenuSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly IInputManager _inputManager;
        private readonly SaveManager _saveManager;
        private readonly Entity _gameStateEntity;

        private bool _menuCreated = false;
        private List<Entity> _menuEntities = new List<Entity>();
        private bool _escWasPressed = false;

        public MenuSystem(World world, EventBus eventBus, IInputManager inputManager, SaveManager saveManager, Entity gameStateEntity)
        {
            _world = world;
            _eventBus = eventBus;
            _inputManager = inputManager;
            _saveManager = saveManager;
            _gameStateEntity = gameStateEntity;

            // Subscribe to button clicks
            _eventBus.Subscribe<UIButtonClickedEvent>(OnButtonClicked);
        }

        public void Update(float deltaTime)
        {
            // Handle ESC key to toggle menu
            HandleEscapeKey();

            // Check if we're in menu state
            ref var gameState = ref _world.Get<CoreGameState>(_gameStateEntity);
            bool inMenu = gameState.IsInMenu || gameState.IsPaused;

            if (inMenu && !_menuCreated)
            {
                CreateMenu();
                _menuCreated = true;
            }
            else if (!inMenu && _menuCreated)
            {
                DestroyMenu();
                _menuCreated = false;
            }
        }

        private void HandleEscapeKey()
        {
            bool escPressed = _inputManager.IsKeyPressed(KeyCode.Escape);

            // Detect key press (not held)
            if (escPressed && !_escWasPressed)
            {
                ref var gameState = ref _world.Get<CoreGameState>(_gameStateEntity);

                if (gameState.IsPlaying)
                {
                    // Open pause menu
                    _world.Set(_gameStateEntity, new CoreGameState(StandardGameState.Paused));
                }
                else if (gameState.IsPaused)
                {
                    // Resume game
                    _world.Set(_gameStateEntity, new CoreGameState(StandardGameState.Playing));
                }
                // If in Menu state (main menu), ESC does nothing
            }

            _escWasPressed = escPressed;
        }

        private void CreateMenu()
        {
            // Dark semi-transparent background panel
            var background = UIFactory.CreateAnchoredPanel(
                _world,
                AnchorPoint.Center,
                Vector2.Zero,
                new Vector2(600, 500),
                Color.FromArgb(220, 30, 30, 40),
                blocksClicks: true,
                order: 100
            );
            _menuEntities.Add(background);

            // Title
            // Estimate text bounds for centering (roughly 20px per character at size 36)
            string titleText = "Cave Explorer";
            float titleWidth = titleText.Length * 20f;
            float titleHeight = 36f;

            var titleEntity = _world.Create(
                new Transform(Vector2.Zero),
                new UIBounds(new Vector2(titleWidth, titleHeight), Vector2.Zero),
                TextRenderer.UIText(titleText, UIColorScheme.Dark.Text, 36f)
            );
            UIFactory.AddAnchor(_world, titleEntity, AnchorPoint.Center, new Vector2(0, -180));
            _menuEntities.Add(titleEntity);

            // Start/Continue Game button
            ref var gameState = ref _world.Get<CoreGameState>(_gameStateEntity);
            string startButtonText = gameState.IsInMenu ? "Start Game" : "Resume";

            var startButton = UIFactory.CreateAnchoredButton(
                _world,
                AnchorPoint.Center,
                new Vector2(0, -80),
                new Vector2(300, 50),
                startButtonText,
                UIColorScheme.Dark.ButtonNormal,
                buttonId: "start_game"
            );
            _menuEntities.Add(startButton);

            // Save Game button
            var saveButton = UIFactory.CreateAnchoredButton(
                _world,
                AnchorPoint.Center,
                new Vector2(0, -20),
                new Vector2(300, 50),
                "Save Game",
                UIColorScheme.Dark.Success,
                buttonId: "save_game"
            );
            _menuEntities.Add(saveButton);

            // Load Game button
            var loadButton = UIFactory.CreateAnchoredButton(
                _world,
                AnchorPoint.Center,
                new Vector2(0, 40),
                new Vector2(300, 50),
                "Load Game",
                UIColorScheme.Dark.Accent,
                buttonId: "load_game"
            );
            _menuEntities.Add(loadButton);

            // New Game button
            var newGameButton = UIFactory.CreateAnchoredButton(
                _world,
                AnchorPoint.Center,
                new Vector2(0, 100),
                new Vector2(300, 50),
                "New Game",
                UIColorScheme.Dark.ButtonNormal,
                buttonId: "new_game"
            );
            _menuEntities.Add(newGameButton);

            // Quit button
            var quitButton = UIFactory.CreateAnchoredButton(
                _world,
                AnchorPoint.Center,
                new Vector2(0, 160),
                new Vector2(300, 50),
                "Quit",
                UIColorScheme.Dark.Error,
                buttonId: "quit"
            );
            _menuEntities.Add(quitButton);

            // Status text (for showing save/load messages)
            var statusText = _world.Create(
                new Transform(Vector2.Zero),
                TextRenderer.UIText("", UIColorScheme.Dark.TextSecondary, 14f),
                new MenuStatusText()
            );
            UIFactory.AddAnchor(_world, statusText, AnchorPoint.Center, new Vector2(0, 210));
            _menuEntities.Add(statusText);
        }

        private void DestroyMenu()
        {
            foreach (var entity in _menuEntities)
            {
                if (_world.IsAlive(entity))
                {
                    _world.Destroy(entity);
                }
            }
            _menuEntities.Clear();
        }

        private void OnButtonClicked(UIButtonClickedEvent e)
        {
            switch (e.ButtonId)
            {
                case "start_game":
                    StartGame();
                    break;
                case "save_game":
                    SaveGame();
                    break;
                case "load_game":
                    LoadGame();
                    break;
                case "new_game":
                    NewGame();
                    break;
                case "quit":
                    QuitGame();
                    break;
            }
        }


        private void StartGame()
        {
            _world.Set(_gameStateEntity, new CoreGameState(StandardGameState.Playing));
        }

        private void SaveGame()
        {
            try
            {
                bool success = _saveManager.Save("slot1");
                if (success)
                {
                    ShowStatusMessage("Game saved successfully!", UIColorScheme.Dark.Success);
                    Console.WriteLine("Game saved to slot1");
                }
                else
                {
                    ShowStatusMessage("Save failed!", UIColorScheme.Dark.Error);
                    Console.WriteLine("Save failed");
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Save failed: {ex.Message}", UIColorScheme.Dark.Error);
                Console.WriteLine($"Save failed: {ex}");
            }
        }

        private void LoadGame()
        {
            try
            {
                bool success = _saveManager.Load("slot1");
                if (success)
                {
                    ShowStatusMessage("Game loaded successfully!", UIColorScheme.Dark.Success);
                    Console.WriteLine("Game loaded from slot1");

                    // Return to gameplay
                    _world.Set(_gameStateEntity, new CoreGameState(StandardGameState.Playing));
                }
                else
                {
                    ShowStatusMessage("Load failed!", UIColorScheme.Dark.Error);
                    Console.WriteLine("Load failed");
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Load failed: {ex.Message}", UIColorScheme.Dark.Error);
                Console.WriteLine($"Load failed: {ex}");
            }
        }

        private void NewGame()
        {
            ShowStatusMessage("New Game - Not yet implemented", UIColorScheme.Dark.Warning);
            Console.WriteLine("New Game clicked (placeholder)");
        }

        private void QuitGame()
        {
            ShowStatusMessage("Quit - Not yet implemented", UIColorScheme.Dark.Warning);
            Console.WriteLine("Quit clicked (placeholder)");
            // In a real game, you'd call Environment.Exit(0) or close the window
        }

        private void ShowStatusMessage(string message, Color color)
        {
            // Find the status text entity
            var query = new QueryDescription().WithAll<MenuStatusText, TextRenderer>();
            _world.Query(in query, (ref TextRenderer text) =>
            {
                text.Text = message;
                text.Color = color;
            });
        }
    }

    /// <summary>
    /// Tag component to identify the menu status text entity.
    /// </summary>
    public struct MenuStatusText
    {
    }
}
