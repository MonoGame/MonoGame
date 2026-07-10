// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
	static class XactHelpers
	{
        static internal readonly Random Random = new Random();

        public static float ParseDecibels(byte decibles)
        {
            //lazy 4-param fitting:
            //0xff 6.0
            //0xca 2.0
            //0xbf 1.0
            //0xb4 0.0
            //0x8f -4.0
            //0x5a -12.0
            //0x14 -38.0
            //0x00 -96.0
            const double a = -96.0;
            const double b = 0.432254984608615;
            const double c = 80.1748600297963;
            const double d = 67.7385212334047;
            var dB = (float)(((a - d) / (1 + (Math.Pow(decibles / c, b)))) + d);

            return dB;
        }

        public static float ParseVolumeFromDecibels(byte decibles)
        {
            //lazy 4-param fitting:
            //0xff 6.0
            //0xca 2.0
            //0xbf 1.0
            //0xb4 0.0
            //0x8f -4.0
            //0x5a -12.0
            //0x14 -38.0
            //0x00 -96.0
            const double a = -96.0;
            const double b = 0.432254984608615;
            const double c = 80.1748600297963;
            const double d = 67.7385212334047;
            var dB = (float)(((a - d) / (1 + (Math.Pow(decibles / c, b)))) + d);

            return ParseVolumeFromDecibels(dB);
        }

        public static float ParseVolumeFromDecibels(float decibles)
        {
            // Convert from decibles to linear volume.
            return (float)Math.Pow(10.0, decibles / 20.0);
        }
	}
}

