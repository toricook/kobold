using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Scenes;
using Kobold.Core.Services;
using Kobold.Core.Narrative;
using Kobold.Extensions.Narrative.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Kobold.Extensions.Narrative.Scenes
{
    /// <summary>
    /// Simple dialogue UI scene that displays dialogue text and choices.
    /// This is a basic implementation using TextRenderer and RectangleRenderer.
    /// For more advanced UI, you can create a custom implementation with sprites, animations, etc.
    /// </summary>
    public class DialogueUIScene : BaseScene
    {
        public override string SceneName => "DialogueUI";
        public override bool IsOverlay => true;
        public override SystemGroup ActiveSystemGroup => SystemGroup.Paused;

        private SceneContext? _context;
        private readonly List<Entity> _uiEntities = new List<Entity>();
        private List<DialogueChoice> _currentChoices = new List<DialogueChoice>();
        private int _selectedChoiceIndex = 0;
        private float _lastInputTime = 0;
        private const float INPUT_COOLDOWN = 0.2f; // Prevent rapid input

        public override void Load(SceneContext context)
        {
            _context = context;

            // Subscribe to dialogue events using lambda expressions
            context.EventBus.Subscribe<DialogueStartedEvent>(evt => OnDialogueStarted(evt));
            context.EventBus.Subscribe<DialogueNodeEvent>(evt => OnDialogueNode(evt));
            context.EventBus.Subscribe<DialogueEndedEvent>(evt => OnDialogueEnded(evt));
        }

        public override void Unload(SceneContext context)
        {
            // Cleanup UI entities
            ClearUI();

            // Note: EventBus doesn't support unsubscribing lambda handlers directly
            // In a production system, you might want to store handler references

            _context = null;
        }

        public override void Update(float deltaTime)
        {
            // TODO: Implement input handling for choice selection
            // This requires knowledge of the IInputManager interface and key codes
            // For now, choices can be selected programmatically or via external input handling
        }

        private void OnDialogueStarted(DialogueStartedEvent evt)
        {
            // Create background panel
            CreateDialogueBackground();
        }

        private void OnDialogueNode(DialogueNodeEvent evt)
        {
            // Clear previous UI
            ClearUI();

            if (_context == null)
                return;

            _currentChoices = evt.AvailableChoices;
            _selectedChoiceIndex = 0;

            // Use fixed screen dimensions for now
            // TODO: Get actual screen dimensions from a game configuration or window manager
            float screenWidth = 800f;
            float screenHeight = 600f;

            // Create background panel
            var panelWidth = screenWidth * 0.8f;
            var panelHeight = screenHeight * 0.3f;
            var panelX = screenWidth * 0.1f;
            var panelY = screenHeight - panelHeight - 20;

            var backgroundEntity = _context.World.Create<Transform, RectangleRenderer>();
            _context.World.Set(backgroundEntity, new Transform(new Vector2(panelX, panelY)));
            _context.World.Set(backgroundEntity, RectangleRenderer.UI(
                new Vector2(panelWidth, panelHeight),
                Color.FromArgb(200, 30, 30, 40)  // Semi-transparent dark background
            ));
            _uiEntities.Add(backgroundEntity);

            // Display speaker name if present
            float currentY = panelY + 10;
            if (!string.IsNullOrEmpty(evt.Speaker))
            {
                var speakerEntity = _context.World.Create<Transform, TextRenderer>();
                _context.World.Set(speakerEntity, new Transform(new Vector2(panelX + 15, currentY)));
                _context.World.Set(speakerEntity, TextRenderer.UIText(
                    evt.Speaker + ":",
                    Color.Yellow,
                    20f
                ));
                _uiEntities.Add(speakerEntity);
                currentY += 25;
            }

            // Display dialogue text
            var textEntity = _context.World.Create<Transform, TextRenderer>();
            _context.World.Set(textEntity, new Transform(new Vector2(panelX + 15, currentY)));
            _context.World.Set(textEntity, TextRenderer.UIText(
                WrapText(evt.Text, (int)(panelWidth - 30)),
                Color.White,
                16f
            ));
            _uiEntities.Add(textEntity);
            currentY += 50; // Space for text (simplified - doesn't account for wrapped lines)

            // Display choices
            if (evt.AvailableChoices.Count > 0)
            {
                currentY += 20;

                for (int i = 0; i < evt.AvailableChoices.Count; i++)
                {
                    var choice = evt.AvailableChoices[i];
                    var isSelected = i == _selectedChoiceIndex;

                    // Choice background (highlight if selected)
                    var choiceBackgroundEntity = _context.World.Create<Transform, RectangleRenderer>();
                    _context.World.Set(choiceBackgroundEntity, new Transform(new Vector2(panelX + 20, currentY - 2)));
                    _context.World.Set(choiceBackgroundEntity, RectangleRenderer.UI(
                        new Vector2(panelWidth - 40, 25),
                        isSelected ? Color.FromArgb(150, 100, 100, 120) : Color.FromArgb(80, 50, 50, 60)
                    ));
                    _uiEntities.Add(choiceBackgroundEntity);

                    // Choice text
                    var choiceEntity = _context.World.Create<Transform, TextRenderer>();
                    _context.World.Set(choiceEntity, new Transform(new Vector2(panelX + 30, currentY)));
                    _context.World.Set(choiceEntity, TextRenderer.UIText(
                        (isSelected ? "> " : "  ") + choice.Text,
                        isSelected ? Color.Cyan : Color.LightGray,
                        14f
                    ));
                    _uiEntities.Add(choiceEntity);

                    currentY += 30;
                }

                // Add instruction text
                var instructionEntity = _context.World.Create<Transform, TextRenderer>();
                _context.World.Set(instructionEntity, new Transform(new Vector2(panelX + 15, screenHeight - 30)));
                _context.World.Set(instructionEntity, TextRenderer.UIText(
                    "Use W/S or Up/Down to select, Enter/Space to confirm",
                    Color.Gray,
                    12f
                ));
                _uiEntities.Add(instructionEntity);
            }
        }

        private void OnDialogueEnded(DialogueEndedEvent evt)
        {
            // Clear all UI
            ClearUI();
            _currentChoices.Clear();
            _selectedChoiceIndex = 0;
        }

        private void CreateDialogueBackground()
        {
            // This is called when dialogue starts
            // The actual UI is created in OnDialogueNode
        }

        private void ClearUI()
        {
            if (_context == null)
                return;

            foreach (var entity in _uiEntities)
            {
                if (_context.World.IsAlive(entity))
                {
                    _context.World.Destroy(entity);
                }
            }
            _uiEntities.Clear();
        }

        private void UpdateChoiceHighlight()
        {
            // For this simple implementation, we just recreate the UI
            // A more optimized approach would update entities in place
            if (_context != null)
            {
                // The highlighting is handled by recreating the UI with the new selected index
                // in a production system, you'd want to update the existing entities instead
            }
        }

        private string WrapText(string text, int maxWidth)
        {
            // Simple text wrapping - in production you'd want to measure actual text width
            // This is a placeholder that doesn't do actual wrapping
            // For a real implementation, you'd need to measure text with the renderer
            return text;
        }
    }
}
