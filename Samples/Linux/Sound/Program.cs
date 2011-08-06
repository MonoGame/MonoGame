using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Xna.Samples.Sound
{
    static class Program
    {
        private static Game1 game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            game = new Game1();
            game.Run();
        }
    }
}
