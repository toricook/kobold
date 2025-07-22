using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using Kobold.Core.Abstractions;

namespace Kobold.Monogame
{
    public static class PixelFont
    {
        private static readonly Dictionary<char, string[]> CharacterPatterns = new()
        {
            ['0'] = new[]
            {
                " ████ ",
                "██  ██",
                "██  ██",
                "██  ██",
                "██  ██",
                " ████ "
            },

            ['1'] = new[]
            {
                "  ██  ",
                " ███  ",
                "  ██  ",
                "  ██  ",
                "  ██  ",
                "██████"
            },

            ['2'] = new[]
            {
                " ████ ",
                "██  ██",
                "   ██ ",
                "  ██  ",
                " ██   ",
                "██████"
            },

            ['3'] = new[]
            {
                " ████ ",
                "    ██",
                " ████ ",
                "    ██",
                "██  ██",
                " ████ "
            },

            ['4'] = new[]
            {
                "██  ██",
                "██  ██",
                "██  ██",
                "██████",
                "    ██",
                "    ██"
            },

            ['5'] = new[]
            {
                "██████",
                "██    ",
                "██████",
                "    ██",
                "██  ██",
                " ████ "
            },

            ['6'] = new[]
            {
                " ████ ",
                "██    ",
                "██████",
                "██  ██",
                "██  ██",
                " ████ "
            },

            ['7'] = new[]
            {
                "██████",
                "    ██",
                "   ██ ",
                "  ██  ",
                " ██   ",
                "██    "
            },

            ['8'] = new[]
            {
                " ████ ",
                "██  ██",
                " ████ ",
                "██  ██",
                "██  ██",
                " ████ "
            },

            ['9'] = new[]
            {
                " ████ ",
                "██  ██",
                "██  ██",
                " █████",
                "    ██",
                " ████ "
            },

            ['-'] = new[]
            {
                "      ",
                "      ",
                "██████",
                "      ",
                "      ",
                "      "
            },
            // Letters A-Z
            ['A'] = new[]
            {
                " ████ ",
                "██  ██",
                "██  ██",
                "██████",
                "██  ██",
                "██  ██"
            },

            ['B'] = new[]
            {
                "█████ ",
                "██  ██",
                "█████ ",
                "██  ██",
                "██  ██",
                "█████ "
            },

            ['C'] = new[]
            {
                " ████ ",
                "██  ██",
                "██    ",
                "██    ",
                "██  ██",
                " ████ "
            },

            ['D'] = new[]
            {
                "█████ ",
                "██  ██",
                "██  ██",
                "██  ██",
                "██  ██",
                "█████ "
            },

            ['E'] = new[]
            {
                "██████",
                "██    ",
                "█████ ",
                "██    ",
                "██    ",
                "██████"
            },

            ['F'] = new[]
            {
                "██████",
                "██    ",
                "█████ ",
                "██    ",
                "██    ",
                "██    "
            },

            ['G'] = new[]
            {
                " ████ ",
                "██  ██",
                "██    ",
                "██ ███",
                "██  ██",
                " ████ "
            },

            ['H'] = new[]
            {
                "██  ██",
                "██  ██",
                "██████",
                "██  ██",
                "██  ██",
                "██  ██"
            },

            ['I'] = new[]
            {
                "██████",
                "  ██  ",
                "  ██  ",
                "  ██  ",
                "  ██  ",
                "██████"
            },

            ['J'] = new[]
            {
                "██████",
                "    ██",
                "    ██",
                "    ██",
                "██  ██",
                " ████ "
            },

            ['K'] = new[]
            {
                "██  ██",
                "██ ██ ",
                "████  ",
                "████  ",
                "██ ██ ",
                "██  ██"
            },

            ['L'] = new[]
            {
                "██    ",
                "██    ",
                "██    ",
                "██    ",
                "██    ",
                "██████"
            },

            ['M'] = new[]
            {
                "██  ██",
                "██████",
                "██████",
                "██  ██",
                "██  ██",
                "██  ██"
            },

            ['N'] = new[]
            {
                "██  ██",
                "███ ██",
                "██████",
                "██ ███",
                "██  ██",
                "██  ██"
            },

            ['O'] = new[]
            {
                " ████ ",
                "██  ██",
                "██  ██",
                "██  ██",
                "██  ██",
                " ████ "
            },

            ['P'] = new[]
            {
                "█████ ",
                "██  ██",
                "██  ██",
                "█████ ",
                "██    ",
                "██    "
            },

            ['Q'] = new[]
            {
                " ████ ",
                "██  ██",
                "██  ██",
                "██ ███",
                "██  ██",
                " █████"
            },

            ['R'] = new[]
            {
                "█████ ",
                "██  ██",
                "██  ██",
                "█████ ",
                "██ ██ ",
                "██  ██"
            },

            ['S'] = new[]
            {
                " ████ ",
                "██  ██",
                " ██   ",
                "   ██ ",
                "██  ██",
                " ████ "
            },

            ['T'] = new[]
            {
                "██████",
                "  ██  ",
                "  ██  ",
                "  ██  ",
                "  ██  ",
                "  ██  "
            },

            ['U'] = new[]
            {
                "██  ██",
                "██  ██",
                "██  ██",
                "██  ██",
                "██  ██",
                " ████ "
            },

            ['V'] = new[]
            {
                "██  ██",
                "██  ██",
                "██  ██",
                "██  ██",
                " ████ ",
                "  ██  "
            },

            ['W'] = new[]
            {
                "██  ██",
                "██  ██",
                "██  ██",
                "██████",
                "██████",
                "██  ██"
            },

            ['X'] = new[]
            {
                "██  ██",
                " ████ ",
                "  ██  ",
                "  ██  ",
                " ████ ",
                "██  ██"
            },

            ['Y'] = new[]
            {
                "██  ██",
                "██  ██",
                " ████ ",
                "  ██  ",
                "  ██  ",
                "  ██  "
            },

            ['Z'] = new[]
            {
                "██████",
                "    ██",
                "   ██ ",
                "  ██  ",
                " ██   ",
                "██████"
            },

            [' '] = new[]
            {
                "      ",
                "      ",
                "      ",
                "      ",
                "      ",
                "      "
            },
            ['?'] = new[]
            {
               " ████ ",
               "██  ██",
               "   ██ ",
               "  ██  ",
               "      ",
               "  ██  "
            },
            ['!'] = new[]
            {
               "  ██  ",
               "  ██  ",
               "  ██  ",
               "  ██  ",
               "      ",
               "  ██  "
            },
            [':'] = new[]
            {
               "      ",
               "  ██  ",
               "      ",
               "      ",
               "  ██  ",
               "      "
            },
            ['/'] = new[]
            {
               "    ██",
               "    ██",
               "   ██ ",
               "  ██  ",
               " ██   ",
               "██    "
            },
        };

        public static void DrawText(IRenderer renderer, string text, Vector2 position, Color color, float fontSize = 16f)
        {
            float charWidth = fontSize * 0.8f;

            for (int i = 0; i < text.Length; i++)
            {
                char c = char.ToUpper(text[i]);
                var charPos = new Vector2(position.X + i * charWidth, position.Y);
                DrawCharacter(renderer, c, charPos, fontSize, color);
            }
        }

        private static void DrawCharacter(IRenderer renderer, char c, Vector2 position, float fontSize, Color color)
        {
            if (!CharacterPatterns.TryGetValue(c, out var pattern))
            {
                // Unknown character
                pattern = new[]
                {
                    "██████",
                    "██████",
                    "██████",
                    "██████",
                    "██████",
                    "██████"
                };
            }

            float pixelSize = fontSize / 8f;
            DrawPixels(renderer, position, pixelSize, color, pattern);
        }

        private static void DrawPixels(IRenderer renderer, Vector2 position, float pixelSize, Color color, string[] pattern)
        {
            for (int y = 0; y < pattern.Length; y++)
            {
                for (int x = 0; x < pattern[y].Length; x++)
                {
                    if (pattern[y][x] == '█')
                    {
                        var pixelPos = new Vector2(
                            position.X + x * pixelSize,
                            position.Y + y * pixelSize
                        );
                        renderer.DrawRectangle(pixelPos, new Vector2(pixelSize, pixelSize), color);
                    }
                }
            }
        }

        public static Vector2 MeasureText(string text, float fontSize = 16f)
        {
            float charWidth = fontSize * 0.8f;
            return new Vector2(text.Length * charWidth, fontSize);
        }
    }
}