using System;

namespace $safeprojectname$
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
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
