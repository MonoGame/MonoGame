using System;

namespace TextureSquish
{
    abstract class ColourFit
    {
        public ColourFit(ColourSet colours, CompressionMode flags)
        {
            m_colours = colours;
            m_flags = flags;
        }

        public void Compress(BlockWindow block)
        {
            bool isDxt1 = ((m_flags & CompressionMode.Dxt1) != 0);

            if (isDxt1)
            {
                if (m_colours.IsTransparent) Compress3(block);
                else                         Compress4(block);
            }
            else Compress4(block);
        }

        protected abstract void Compress3(BlockWindow block);
        protected abstract void Compress4(BlockWindow block);

        protected ColourSet m_colours;
        protected CompressionMode m_flags;
    }

}


