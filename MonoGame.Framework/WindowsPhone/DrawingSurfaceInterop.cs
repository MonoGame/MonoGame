using System;
using Microsoft.Xna.Framework;
using SharpDX;
using SharpDX.Direct3D11;

namespace MonoGame.Framework.WindowsPhone
{
    internal static class DrawingSurfaceState
    {
        public static Device Device;
        public static DeviceContext Context;
        public static RenderTargetView RenderTargetView;
    }
}
