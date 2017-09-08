using System;

namespace MonoGame.OpenGL
{
    internal class WindowInfo : IWindowInfo
    {
        public IntPtr Handle { get; private set; }

        public WindowInfo(IntPtr handle)
        {
            Handle = handle;
        }
    }
}
