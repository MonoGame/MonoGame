using System;
using System.Collections.Generic;
using System.Linq;

namespace Platformer
{
    static class Program
    {
        private static PlatformerGame game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            game = new PlatformerGame();
            game.Run();
        }
    }
}
