using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Narrative;
using Kobold.Extensions.Narrative.Events;
using Kobold.Extensions.Narrative.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kobold.Extensions.Narrative.Systems
{
    /// <summary>
    /// System responsible for managing dialogue conversations.
    /// Handles dialogue flow, choice selection, condition evaluation, and flag management.
    /// Use OnDialogueStarted and OnDialogueEnded callbacks to integrate with your game state management.
    /// </summary>
    public class DialogueSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly Dictionary<string, DialogueData> _loadedDialogues;

        /// <summary>
        /// Optional callback invoked when dialogue starts.
        /// Use this to pause the game, show UI, etc.
        /// </summary>
        public Action? OnDialogueStarted { get; set; }

        /// <summary>
        /// Optional callback invoked when dialogue ends.
        /// Use this to resume the game, hide UI, etc.
        /// </summary>
        public Action? OnDialogueEnded { get; set; }

        public DialogueSystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;
            _loadedDialogues = new Dictionary<string, DialogueData>();

            // Subscribe to choice selection events
            _eventBus.Subscribe<DialogueChoiceSelectedEvent>(OnChoiceSelected);
        }

        /// <summary>
        /// Register a loaded dialogue data for use by the system.
        /// Typically called after loading dialogue JSON files.
        /// </summary>
        /// <param name="dialogueData">The dialogue data to register</param>
        public void RegisterDialogue(DialogueData dialogueData)
        {
            _loadedDialogues[dialogueData.Id] = dialogueData;
        }

        /// <summary>
        /// Start a dialogue conversation by ID.
        /// </summary>
        /// <param name="dialogueId">The ID of the dialogue to start</param>
        /// <param name="speaker">Optional entity that is speaking (e.g., NPC)</param>
        public void StartDialogue(string dialogueId, Entity speaker = default)
        {
            if (!_loadedDialogues.TryGetValue(dialogueId, out var dialogueData))
            {
                // TODO: Log error - dialogue not loaded
                return;
            }

            // Check if dialogue is already active
            var existingDialogueQuery = new QueryDescription().WithAll<DialogueState>();
            if (_world.CountEntities(in existingDialogueQuery) > 0)
            {
                // TODO: Log warning - dialogue already active
                return;
            }

            // Create dialogue state entity
            var dialogueEntity = _world.Create<DialogueState>();
            _world.Set(dialogueEntity, new DialogueState(
                dialogueId,
                "start",  // Default starting node
                dialogueData,
                speaker
            ));

            // Invoke callback (e.g., to pause the game)
            OnDialogueStarted?.Invoke();

            // Publish dialogue started event
            _eventBus.Publish(new DialogueStartedEvent(dialogueId, speaker));

            // Show the first node
            ShowCurrentNode(dialogueEntity);
        }

        public void Update(float deltaTime)
        {
            // DialogueSystem is event-driven and doesn't need per-frame updates
            // All logic happens in response to events (choice selections)
        }

        /// <summary>
        /// Display the current dialogue node and publish an event for UI to show it.
        /// </summary>
        private void ShowCurrentNode(Entity dialogueEntity)
        {
            ref var state = ref _world.Get<DialogueState>(dialogueEntity);

            // Get the current node
            if (!state.Data.Nodes.TryGetValue(state.CurrentNodeId, out var node))
            {
                // Node not found - end dialogue
                EndDialogue(dialogueEntity);
                return;
            }

            // Get player's story flags for condition evaluation
            var flags = GetPlayerStoryFlags();

            // Filter choices based on conditions
            var availableChoices = node.Choices
                .Where(choice => EvaluateCondition(choice.Condition, flags))
                .ToList();

            // If no choices and no text, end dialogue
            if (availableChoices.Count == 0 && string.IsNullOrEmpty(node.Text))
            {
                EndDialogue(dialogueEntity);
                return;
            }

            // Publish event for UI to display
            _eventBus.Publish(new DialogueNodeEvent(
                node.Speaker,
                node.Text,
                availableChoices
            ));
        }

        /// <summary>
        /// Handle player choice selection.
        /// </summary>
        private void OnChoiceSelected(DialogueChoiceSelectedEvent evt)
        {
            // Find the active dialogue
            var dialogueQuery = new QueryDescription().WithAll<DialogueState>();
            Entity dialogueEntity = Entity.Null;

            _world.Query(in dialogueQuery, (Entity entity) =>
            {
                dialogueEntity = entity;
            });

            if (dialogueEntity == Entity.Null)
            {
                // No active dialogue
                return;
            }

            ref var state = ref _world.Get<DialogueState>(dialogueEntity);

            // Get the current node
            if (!state.Data.Nodes.TryGetValue(state.CurrentNodeId, out var node))
            {
                EndDialogue(dialogueEntity);
                return;
            }

            // Get player flags for condition evaluation
            var flags = GetPlayerStoryFlags();

            // Filter choices to get available ones (same logic as ShowCurrentNode)
            var availableChoices = node.Choices
                .Where(choice => EvaluateCondition(choice.Condition, flags))
                .ToList();

            // Validate choice index
            if (evt.ChoiceIndex < 0 || evt.ChoiceIndex >= availableChoices.Count)
            {
                // Invalid choice index
                return;
            }

            var selectedChoice = availableChoices[evt.ChoiceIndex];

            // Apply flag changes from the choice
            if (selectedChoice.SetFlags != null && selectedChoice.SetFlags.Count > 0)
            {
                ApplyFlagChanges(selectedChoice.SetFlags);
            }

            // Navigate to next node or end dialogue
            if (string.IsNullOrEmpty(selectedChoice.Next) || selectedChoice.Next == "end")
            {
                EndDialogue(dialogueEntity);
            }
            else
            {
                // Update current node
                state.CurrentNodeId = selectedChoice.Next;
                ShowCurrentNode(dialogueEntity);
            }
        }

        /// <summary>
        /// End the current dialogue conversation.
        /// </summary>
        private void EndDialogue(Entity dialogueEntity)
        {
            if (dialogueEntity == Entity.Null)
                return;

            ref var state = ref _world.Get<DialogueState>(dialogueEntity);
            var dialogueId = state.DialogueId;

            // Publish dialogue ended event
            _eventBus.Publish(new DialogueEndedEvent(dialogueId));

            // Invoke callback (e.g., to resume the game)
            OnDialogueEnded?.Invoke();

            // Destroy the dialogue state entity
            _world.Destroy(dialogueEntity);
        }

        /// <summary>
        /// Evaluate a dialogue condition against the player's story flags.
        /// </summary>
        /// <param name="condition">The condition to evaluate (null means always true)</param>
        /// <param name="flags">The player's story flags</param>
        /// <returns>True if the condition is met</returns>
        private bool EvaluateCondition(DialogueCondition? condition, Dictionary<string, object> flags)
        {
            // No condition means always available
            if (condition == null)
                return true;

            // Check if flag exists
            if (!flags.TryGetValue(condition.Flag, out var flagValue))
                return false;

            // Compare values
            return Equals(flagValue, condition.Value);
        }

        /// <summary>
        /// Get the player entity's story flags.
        /// Creates a StoryFlags component if the player doesn't have one.
        /// </summary>
        /// <returns>Dictionary of story flags</returns>
        private Dictionary<string, object> GetPlayerStoryFlags()
        {
            var playerQuery = new QueryDescription().WithAll<Player>();
            Entity playerEntity = Entity.Null;

            _world.Query(in playerQuery, (Entity entity) =>
            {
                playerEntity = entity;
            });

            if (playerEntity == Entity.Null)
            {
                // No player entity - return empty flags
                return new Dictionary<string, object>();
            }

            // Check if player has StoryFlags component
            if (!_world.Has<StoryFlags>(playerEntity))
            {
                // Add StoryFlags component to player
                _world.Add(playerEntity, new StoryFlags());
            }

            ref var storyFlags = ref _world.Get<StoryFlags>(playerEntity);

            // Ensure flags dictionary is initialized
            if (storyFlags.Flags == null)
            {
                storyFlags.Flags = new Dictionary<string, object>();
            }

            return storyFlags.Flags;
        }

        /// <summary>
        /// Apply flag changes to the player's story flags.
        /// </summary>
        /// <param name="flagChanges">Dictionary of flags to set</param>
        private void ApplyFlagChanges(Dictionary<string, object> flagChanges)
        {
            var flags = GetPlayerStoryFlags();

            foreach (var kvp in flagChanges)
            {
                flags[kvp.Key] = kvp.Value;
            }
        }
    }
}
