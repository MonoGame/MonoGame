// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class Image2DReader : ContentTypeReader<Image2D>
    {
        private Image2DArrayReader _arrayReader;
        private Image2DArray _existingArray;

        public Image2DReader()
        {
            _arrayReader = new Image2DArrayReader();
        }

        protected internal override Image2D Read(ContentReader reader, Image2D existingInstance)
        {
            _existingArray = _arrayReader.Read(reader, _existingArray);
            return _existingArray[0];
        }
    }
}
