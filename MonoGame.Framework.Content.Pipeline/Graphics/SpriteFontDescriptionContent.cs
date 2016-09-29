// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides a base class for all texture objects.
    /// </summary>
    public abstract class SpriteFontDescriptionContent : ContentItem, IDisposable
    {
        MipmapChainCollection faces;
        internal Bitmap _bitmap;

        /// <summary>
        /// Collection of image faces that hold a single mipmap chain for a regular 2D texture, six chains for a cube map, or an arbitrary number for volume and array textures.
        /// </summary>
        public MipmapChainCollection Faces
        {
            get
            {
                return faces;
            }
        }

        /// <summary>
        /// Initializes a new instance of TextureContent with the specified face collection.
        /// </summary>
        /// <param name="faces">Mipmap chain containing the face collection.</param>
        protected TextureContent(MipmapChainCollection faces)
        {
            this.faces = faces;
        }

        /// <summary>
        /// Converts all bitmaps for this texture to a different format.
        /// </summary>
        /// <param name="newBitmapType">Type being converted to. The new type must be a subclass of BitmapContent, such as PixelBitmapContent or DxtBitmapContent.</param>
        public void ConvertBitmapType(Type newBitmapType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates a full set of mipmaps for the texture.
        /// </summary>
        /// <param name="overwriteExistingMipmaps">true if the existing mipmap set is replaced with the new set; false otherwise.</param>
        public virtual void GenerateMipmaps(bool overwriteExistingMipmaps)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Verifies that all contents of this texture are present, correct and match the capabilities of the device.
        /// </summary>
        /// <param name="targetProfile">The profile identifier that defines the capabilities of the device.</param>
        public abstract void Validate(Nullable<GraphicsProfile> targetProfile);

        public virtual void Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }
    }
}
