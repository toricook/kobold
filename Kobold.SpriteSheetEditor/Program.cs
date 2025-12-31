using System;
using SpriteSheetEditor;

class Program
{
    [STAThread]
    static void Main()
    {
        var editor = new SpriteSheetEditorGame();
        editor.Run();
    }
}
