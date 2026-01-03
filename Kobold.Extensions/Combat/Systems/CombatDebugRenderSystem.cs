using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Extensions.Combat.Components;
using Kobold.Extensions.Combat.Events;

namespace Kobold.Extensions.Combat.Systems
{
    /// <summary>
    /// Debug render system that visualizes attack ranges, damage areas, and health states.
    /// Only renders when debug mode is enabled.
    /// </summary>
    public class CombatDebugRenderSystem : IRenderSystem
    {
        private readonly World _world;
        private readonly IRenderer _renderer;
        private readonly EventBus _eventBus;
        private bool _showAttackRadius = true;
        private bool _showHealthBars = true;

        // Track recent attacks for visualization
        private List<DebugAttackVisualization> _recentAttacks = new List<DebugAttackVisualization>();

        public bool DebugEnabled { get; set; } = true;

        public CombatDebugRenderSystem(World world, IRenderer renderer, EventBus eventBus)
        {
            _world = world;
            _renderer = renderer;
            _eventBus = eventBus;

            // Subscribe to attack events
            _eventBus.Subscribe<AttackPerformedEvent>(OnAttackPerformed);
        }

        public void Render()
        {
            if (!DebugEnabled)
                return;

            // Get camera for world-to-screen conversion
            Camera? camera = GetCamera();

            // Draw attack radius for entities with melee weapons
            if (_showAttackRadius)
            {
                DrawAttackRadii(camera);
            }

            // Draw recent attack animations
            DrawRecentAttacks(camera);

            // Draw health bars
            if (_showHealthBars)
            {
                DrawHealthBars(camera);
            }
        }

        private Camera? GetCamera()
        {
            var cameraQuery = new QueryDescription().WithAll<Camera>();
            Camera? camera = null;
            _world.Query(in cameraQuery, (ref Camera cam) =>
            {
                camera = cam;
            });
            return camera;
        }

        private void DrawAttackRadii(Camera? camera)
        {
            var weaponQuery = new QueryDescription()
                .WithAll<Transform, MeleeWeaponComponent, Player>();

            _world.Query(in weaponQuery, (Entity entity, ref Transform transform, ref MeleeWeaponComponent weapon) =>
            {
                // Draw circle representing attack radius
                Color radiusColor = _world.Has<AttackCooldownComponent>(entity)
                    ? Color.FromArgb(100, 255, 0, 0)  // Red if on cooldown
                    : Color.FromArgb(100, 0, 255, 0); // Green if ready

                // Convert world position to screen position
                Vector2 screenPosition = camera.HasValue
                    ? camera.Value.WorldToScreen(transform.Position)
                    : transform.Position;

                DrawCircle(screenPosition, weapon.AttackRadius, radiusColor, 2f);
            });
        }

        private void DrawRecentAttacks(Camera? camera)
        {
            // Update and draw attack flash effects
            for (int i = _recentAttacks.Count - 1; i >= 0; i--)
            {
                var attack = _recentAttacks[i];
                attack.Duration -= 0.016f; // Approximate frame time

                if (attack.Duration <= 0f)
                {
                    _recentAttacks.RemoveAt(i);
                    continue;
                }

                // Flash effect: brighter at start, fades out
                float alpha = attack.Duration / 0.3f; // 0.3s total duration
                Color flashColor = Color.FromArgb((int)(alpha * 150), 255, 255, 0);

                // Convert world position to screen position
                Vector2 screenPosition = camera.HasValue
                    ? camera.Value.WorldToScreen(attack.Position)
                    : attack.Position;

                DrawCircle(screenPosition, attack.Radius, flashColor, 3f);

                // Update the attack visualization
                _recentAttacks[i] = attack;
            }
        }

        private void DrawHealthBars(Camera? camera)
        {
            var healthQuery = new QueryDescription()
                .WithAll<Transform, HealthComponent>();

            _world.Query(in healthQuery, (Entity entity, ref Transform transform, ref HealthComponent health) =>
            {
                float healthPercent = (float)health.CurrentHealth / health.MaxHealth;

                // Convert world position to screen position
                Vector2 screenPosition = camera.HasValue
                    ? camera.Value.WorldToScreen(transform.Position)
                    : transform.Position;

                Vector2 barPosition = screenPosition + new Vector2(-20, -30);
                Vector2 barSize = new Vector2(40, 4);

                // Background (black)
                _renderer.DrawRectangle(barPosition, barSize, Color.Black);

                // Health bar (green to red based on health)
                Color healthColor = healthPercent > 0.5f
                    ? Color.FromArgb(255, 0, 255, 0)
                    : Color.FromArgb(255, 255, 0, 0);

                Vector2 healthSize = new Vector2(barSize.X * healthPercent, barSize.Y);
                _renderer.DrawRectangle(barPosition, healthSize, healthColor);

                // Invulnerability indicator
                if (health.InvulnerabilityTimer > 0f)
                {
                    DrawCircle(screenPosition, 25f, Color.FromArgb(50, 255, 255, 255), 1f);
                }
            });
        }

        private void DrawCircle(Vector2 center, float radius, Color color, float thickness = 2f)
        {
            // Draw circle using line segments
            int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)i / segments * MathF.PI * 2;
                float angle2 = (float)(i + 1) / segments * MathF.PI * 2;

                Vector2 p1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius;
                Vector2 p2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius;

                _renderer.DrawLine(p1, p2, color, thickness);
            }
        }

        private void OnAttackPerformed(AttackPerformedEvent evt)
        {
            // Add attack visualization
            _recentAttacks.Add(new DebugAttackVisualization
            {
                Position = evt.AttackPosition,
                Radius = evt.AttackRadius,
                Duration = 0.3f // 300ms flash
            });
        }

        private struct DebugAttackVisualization
        {
            public Vector2 Position;
            public float Radius;
            public float Duration;
        }
    }
}
