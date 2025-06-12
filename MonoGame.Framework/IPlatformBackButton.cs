using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Allows for platform specific handling of the Back button. 
    /// </summary>
    public interface IPlatformBackButton
    {
        /// <summary>
        /// Return true if your game has handled the back button event
        /// return false if you want the operating system to handle it.
        /// </summary>
        bool Handled();
    }
}
