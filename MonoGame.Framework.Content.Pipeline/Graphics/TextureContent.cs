// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides a base class for all texture objects.
    /// </summary>
    public abstract class TextureContent : ContentItem
    {
        MipmapChainCollection faces;

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
            if (newBitmapType == null)
                throw new ArgumentNullException("newBitmapType");

            if (!newBitmapType.IsSubclassOf(typeof (BitmapContent)))
                throw new ArgumentException(string.Format("Type '{0}' is not a subclass of BitmapContent.", newBitmapType));

            if (newBitmapType.IsAbstract)
                throw new ArgumentException(string.Format("Type '{0}' is abstract and cannot be allocated.", newBitmapType));

            if (newBitmapType.ContainsGenericParameters)
                throw new ArgumentException(string.Format("Type '{0}' contains generic parameters and cannot be allocated.", newBitmapType));

            if (newBitmapType.GetConstructor(new Type[2] {typeof (int), typeof (int)}) == null)
                throw new ArgumentException(string.Format("Type '{0} does not have a constructor with signature (int, int) and cannot be allocated.",
                                                          newBitmapType));

            foreach (var mipChain in faces)
            {
                for (var i = 0; i < mipChain.Count; i++)
                {
                    var src = mipChain[i];
                    if (src.GetType() != newBitmapType)
                    {
                        var dst = (BitmapContent)Activator.CreateInstance(newBitmapType, new object[] { src.Width,src.Height });
                        BitmapContent.Copy(src, dst);
                        mipChain[i] = dst;
                    }
                }
            }
        }        

        /// <summary>
        /// Generates a full set of mipmaps for the texture.
        /// </summary>
        /// <param name="overwriteExistingMipmaps">true if the existing mipmap set is replaced with the new set; false otherwise.</param>
        public virtual void GenerateMipmaps(bool overwriteExistingMipmaps)
        {
            // If we already have mipmaps and we're not supposed to overwrite
            // them then return without any generation.
            if (!overwriteExistingMipmaps && faces.Any(f => f.Count > 1))
                return;

            // Generate the mips for each face.
            foreach (var face in faces)
            {
                // Remove any existing mipmaps.
                var faceBitmap = face[0];
                face.Clear();
                face.Add(faceBitmap);
                var faceType = faceBitmap.GetType();
                int width = faceBitmap.Width;
                int height = faceBitmap.Height;
                while (width > 1 && height > 1)
                {
                    width /= 2;
                    height /= 2;

                    var mip = (BitmapContent)Activator.CreateInstance(faceType, new object[] { width, height });
                    BitmapContent.Copy(faceBitmap, mip);
                    face.Add(mip);
                }
            }
        }

        /// <summary>
        /// Verifies that all contents of this texture are present, correct and match the capabilities of the device.
        /// </summary>
        /// <param name="targetProfile">The profile identifier that defines the capabilities of the device.</param>
        public abstract void Validate(GraphicsProfile? targetProfile);
    }
}
