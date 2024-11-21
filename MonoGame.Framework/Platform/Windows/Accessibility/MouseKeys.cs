using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Windows.Accessibility
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseKeys
    {
        public uint cbSize;
        public MouseKeysFlags dwFlags;
        public int iMaxSpeed;
        public int iTimeToMaxSpeed;
        public int iCtrlSpeed;
        public int dwReserved1;
        public int dwReserved2;
    }
}
