using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class TextureCollection
    {
        private Dictionary<int, Texture> _textures = new Dictionary<int, Texture>();

        public Texture this[int index]
        {
            get { return _textures[index]; }
            set { _textures[index] = value; }
        }

        internal void Clear()
        {
            _textures.Clear();
        }
    }
}
