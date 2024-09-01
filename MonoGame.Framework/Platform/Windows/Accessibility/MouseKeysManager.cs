using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Windows.Accessibility
{
    internal class MouseKeysManager
    {
        internal static bool IsEnabled()
        {
            MouseKeys mouseKeys = new MouseKeys();
            mouseKeys.cbSize = (uint) Marshal.SizeOf(mouseKeys);

            NativeMethods.SystemParametersInfo(SPI.SPI_GETMOUSEKEYS,(uint) Marshal.SizeOf(mouseKeys), ref mouseKeys, SPIF.None);

            return mouseKeys.dwFlags.HasFlag(MouseKeysFlags.Available) && mouseKeys.dwFlags.HasFlag(MouseKeysFlags.MouseKeysOn);
        }
    }
}
