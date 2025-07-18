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
        public float ScreenWidth { get; set; } = 800f;
        public float ScreenHeight { get; set; } = 600f;
        public Color BackgroundColor { get; set; } = Color.Black;
        public string Title { get; set; } = "Kobold Game";
        public bool VSync { get; set; } = true;
    }
}
