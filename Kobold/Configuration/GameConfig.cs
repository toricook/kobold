using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Configuration
{
    public class GameConfig
    {
        public GameConfig() { }
        public GameConfig(int screenWidth, int screenHeight, Color backgroundColor)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            BackgroundColor = backgroundColor;
        }

        public int ScreenWidth { get; set; } = 800;
        public int ScreenHeight { get; set; } = 600;
        public Color BackgroundColor { get; set; } = Color.Black;
    }
}
