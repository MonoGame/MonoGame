using System;

using Vec3 = Microsoft.Xna.Framework.Vector3;
using Vec4 = Microsoft.Xna.Framework.Vector4;

namespace TextureSquish
{
    /// <summary>
    /// Represents a set of block colours
    /// </summary>
    class ColourSet
    {
        public ColourSet(Byte[] rgba, int mask, CompressionMode flags)
        {
            // check the compression mode for dxt1
            bool isDxt1 = ((flags & CompressionMode.Dxt1) != 0);
            bool weightByAlpha = ((flags & CompressionMode.WeightColourByAlpha) != 0);

            // create the minimal set
            for (int i = 0; i < 16; ++i)
            {
                // check this pixel is enabled
                int bit = 1 << i;
                if ((mask & bit) == 0)
                {
                    m_remap[i] = -1;
                    continue;
                }

                // check for transparent pixels when using dxt1
                if (isDxt1 && rgba[4 * i + 3] < 128)
                {
                    m_remap[i] = -1;
                    m_transparent = true;
                    continue;
                }

                // loop over previous points for a match
                for (int j = 0; ; ++j)
                {
                    // allocate a new point
                    if (j == i)
                    {
                        // normalise coordinates to [0,1]
                        float x = (float)rgba[4 * i + 0] / 255.0f;
                        float y = (float)rgba[4 * i + 1] / 255.0f;
                        float z = (float)rgba[4 * i + 2] / 255.0f;

                        // ensure there is always non-zero weight even for zero alpha
                        float w = (float)(rgba[4 * i + 3] + 1) / 256.0f;

                        // add the point
                        m_points[m_count] = new Vec3(x, y, z);
                        m_weights[m_count] = (weightByAlpha ? w : 1.0f);
                        m_remap[i] = m_count;

                        // advance
                        ++m_count;
                        break;
                    }

                    // check for a match
                    int oldbit = 1 << j;
                    bool match = ((mask & oldbit) != 0)
                        && (rgba[4 * i + 0] == rgba[4 * j + 0])
                        && (rgba[4 * i + 1] == rgba[4 * j + 1])
                        && (rgba[4 * i + 2] == rgba[4 * j + 2])
                        && (rgba[4 * j + 3] >= 128 || !isDxt1);
                    if (match)
                    {
                        // get the index of the match
                        int index = m_remap[j];

                        // ensure there is always non-zero weight even for zero alpha
                        float w = (float)(rgba[4 * i + 3] + 1) / 256.0f;

                        // map to this point and increase the weight
                        m_weights[index] += (weightByAlpha ? w : 1.0f);
                        m_remap[i] = index;
                        break;
                    }
                }
            }

            // square root the weights
            for (int i = 0; i < m_count; ++i)
                m_weights[i] = (float)Math.Sqrt(m_weights[i]);
        }

        public int Count { get { return m_count; } }
        public Vec3[] Points { get { return m_points; } }
        public float[] Weights { get { return m_weights; } }

        public bool IsTransparent { get { return m_transparent; } }

        public void RemapIndices(Byte[] source, Byte[] target)
        {
            for (int i = 0; i < 16; ++i)
            {
                int j = m_remap[i];
                if (j == -1) target[i] = 3;
                else         target[i] = source[j];
            }
        }

        int m_count;
        private readonly Vec3[] m_points = new Vec3[16];
        private readonly float[] m_weights = new float[16];
        private readonly int[] m_remap = new int[16];
        bool m_transparent;
    };

}


