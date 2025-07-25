namespace Example;

using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

public static partial class Program
{
    readonly static Game1 game = new();

    [STAThread, SupportedOSPlatform("linux")]
    static void Main()
    {
        // game.Run();
    }

    static void RunFrame()
    {
        try
        {
            game.RunOneFrame();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    static bool run = false;

    [JSExport()]
    public static void MainLoop()
    {
        if (!run)
        {
            RunFrame();
            run = true;
        }
    }
}
