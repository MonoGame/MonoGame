using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Windows.Accessibility
{
    internal class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref MouseKeys pvParam, SPIF fWinIni);
    }
}
