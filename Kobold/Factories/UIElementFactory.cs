using Kobold.Core.Components;
using Kobold.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Factories
{
    public static class UIElementFactory
    {
        public static UIElementConfig CreateText(System.Numerics.Vector2 position, string text, System.Drawing.Color color, float fontSize = 16f)
        {
            return new UIElementConfig
            {
                Components = new List<object>
                {
                    new Transform(position),
                    new TextRenderer(text, color, fontSize)
                }
            };
        }

        public static UIElementConfig CreateButton(System.Numerics.Vector2 position, System.Numerics.Vector2 size, System.Drawing.Color color)
        {
            return new UIElementConfig
            {
                Components = new List<object>
                {
                    new Transform(position),
                    new RectangleRenderer(size, color)
                }
            };
        }
    }
}
