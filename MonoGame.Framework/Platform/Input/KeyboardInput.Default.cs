using System;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class KeyboardInput
    {
        private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            throw new NotImplementedException("KeyboardInput is not implemented on this platform.");
        }

        private static void PlatformCancel(string result)
        {
            throw new NotImplementedException("KeyboardInput is not implemented on this platform.");
        }
    }
}
