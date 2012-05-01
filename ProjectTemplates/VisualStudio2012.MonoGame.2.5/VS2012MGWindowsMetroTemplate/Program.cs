#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace $safeprojectname$
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private static Game1 game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var factory = new MonoGame.Framework.GameFrameworkViewSource<Game1>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }
}
