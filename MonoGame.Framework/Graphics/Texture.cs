// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
	public abstract partial class Texture : GraphicsResource
	{
		internal SurfaceFormat _format;
		internal int _levelCount;

		public SurfaceFormat Format
		{
			get { return _format; }
		}
		
		public int LevelCount
		{
			get { return _levelCount; }
		}

        internal static int CalculateMipLevels(int width, int height = 0, int depth = 0)
        {
            int levels = 1;
            int size = Math.Max(Math.Max(width, height), depth);
            while (size > 1)
            {
                size = size / 2;
                levels++;
            }
            return levels;
        }

        internal int GetPitch(int width)
        {
            Debug.Assert(width > 0, "The width is negative!");

            int pitch;

            switch (_format)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1a:
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                case SurfaceFormat.RgbEtc1:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.RgbPvrtc4Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:                    
                    pitch = ((width + 3) / 4) * _format.GetSize();
                    break;

                default:
                    pitch = width * _format.GetSize();
                    break;
            };

            return pitch;
        }

        internal protected override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }
	}
}

