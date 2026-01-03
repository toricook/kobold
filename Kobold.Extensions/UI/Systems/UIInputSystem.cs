using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Extensions.UI.Components;
using Kobold.Extensions.UI.Events;
using System.Numerics;

namespace Kobold.Extensions.UI.Systems
{
    /// <summary>
    /// System that handles UI input and interaction.
    /// Performs hit detection using mouse position and UIBounds,
    /// updates UIInteractive state, and publishes UI interaction events.
    ///
    /// This system should be updated before game logic systems so that
    /// UI interactions are processed first.
    /// </summary>
    public class UIInputSystem : ISystem
    {
        private readonly World _world;
        private readonly IInputManager _inputManager;
        private readonly EventBus _eventBus;

        private Entity? _lastHoveredEntity;
        private bool _wasMouseDownLastFrame;

        /// <summary>
        /// Creates a new UIInputSystem.
        /// </summary>
        /// <param name="world">The ECS world</param>
        /// <param name="inputManager">Input manager for mouse input</param>
        /// <param name="eventBus">Event bus for publishing UI events</param>
        public UIInputSystem(World world, IInputManager inputManager, EventBus eventBus)
        {
            _world = world;
            _inputManager = inputManager;
            _eventBus = eventBus;
            _lastHoveredEntity = null;
            _wasMouseDownLastFrame = false;
        }

        public void Update(float deltaTime)
        {
            var mousePosition = _inputManager.GetMousePosition();
            var isMouseDown = _inputManager.IsMouseButtonPressed(MouseButton.Left);

            // Reset WasClicked flag for all interactive elements
            ResetClickedFlags();

            // Find the topmost UI element under the mouse
            Entity? hoveredEntity = FindTopmostUIElement(mousePosition);

            // Handle hover state changes
            HandleHoverState(hoveredEntity);

            // Update interaction state for all UI elements
            UpdateInteractionState(hoveredEntity, mousePosition, isMouseDown);

            // Track mouse state for next frame
            _wasMouseDownLastFrame = isMouseDown;
        }

        /// <summary>
        /// Resets the WasClicked flag for all interactive UI elements.
        /// This ensures clicks are only detected for one frame.
        /// </summary>
        private void ResetClickedFlags()
        {
            var query = new QueryDescription().WithAll<UIInteractive>();

            _world.Query(in query, (ref UIInteractive interactive) =>
            {
                interactive.WasClicked = false;
            });
        }

        /// <summary>
        /// Finds the topmost UI element at the given position.
        /// Returns null if no UI element contains the point.
        /// UI elements are checked in render layer order (highest layer first).
        /// </summary>
        private Entity? FindTopmostUIElement(Vector2 mousePosition)
        {
            Entity? topmostEntity = null;
            int highestLayer = int.MinValue;

            var query = new QueryDescription().WithAll<Transform, UIBounds, UIInteractive>();

            _world.Query(in query, (Entity entity, ref Transform transform, ref UIBounds bounds, ref UIInteractive interactive) =>
            {
                // Skip disabled elements
                if (!interactive.IsEnabled)
                    return;

                // Check if mouse is within bounds
                if (!bounds.Contains(transform.Position, mousePosition))
                    return;

                // Get the render layer (if present)
                int layer = 0;
                if (_world.Has<SpriteRenderer>(entity))
                {
                    ref var sprite = ref _world.Get<SpriteRenderer>(entity);
                    layer = sprite.Layer;
                }
                else if (_world.Has<RectangleRenderer>(entity))
                {
                    ref var rect = ref _world.Get<RectangleRenderer>(entity);
                    layer = rect.Layer;
                }
                else if (_world.Has<TextRenderer>(entity))
                {
                    ref var text = ref _world.Get<TextRenderer>(entity);
                    layer = text.Layer;
                }

                // Keep the entity with the highest layer
                if (layer > highestLayer)
                {
                    highestLayer = layer;
                    topmostEntity = entity;
                }
            });

            return topmostEntity;
        }

        /// <summary>
        /// Handles hover enter and exit events.
        /// </summary>
        private void HandleHoverState(Entity? hoveredEntity)
        {
            // Check if hover state changed
            if (hoveredEntity != _lastHoveredEntity)
            {
                // Publish hover exit event for previously hovered entity
                if (_lastHoveredEntity.HasValue)
                {
                    _eventBus.Publish(new UIHoverExitEvent { Entity = _lastHoveredEntity.Value });
                }

                // Publish hover enter event for newly hovered entity
                if (hoveredEntity.HasValue)
                {
                    _eventBus.Publish(new UIHoverEnterEvent { Entity = hoveredEntity.Value });
                }

                _lastHoveredEntity = hoveredEntity;
            }
        }

        /// <summary>
        /// Updates the interaction state for all UI elements.
        /// </summary>
        private void UpdateInteractionState(Entity? hoveredEntity, Vector2 mousePosition, bool isMouseDown)
        {
            var query = new QueryDescription().WithAll<UIInteractive>();

            _world.Query(in query, (Entity entity, ref UIInteractive interactive) =>
            {
                // Skip disabled elements
                if (!interactive.IsEnabled)
                {
                    interactive.IsHovered = false;
                    interactive.IsPressed = false;
                    interactive.WasClicked = false;
                    interactive.WasPressedLastFrame = false;
                    return;
                }

                bool isHovered = hoveredEntity.HasValue && hoveredEntity.Value == entity;

                // Update hover state
                interactive.IsHovered = isHovered;

                if (isHovered)
                {
                    // Update pressed state
                    interactive.IsPressed = isMouseDown;

                    // Detect click (mouse was down last frame, now released while hovering)
                    if (interactive.WasPressedLastFrame && !isMouseDown)
                    {
                        interactive.WasClicked = true;

                        // Publish click event
                        _eventBus.Publish(new UIClickEvent
                        {
                            Entity = entity,
                            MousePosition = mousePosition
                        });
                    }

                    // Update last frame state
                    interactive.WasPressedLastFrame = isMouseDown;
                }
                else
                {
                    // Not hovered - reset interaction state
                    interactive.IsPressed = false;
                    interactive.WasPressedLastFrame = false;
                }
            });
        }
    }
}
