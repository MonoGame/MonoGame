#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
 All rights reserved.
 
 This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
 accept the license, do not use the software.
 
 1. Definitions
 The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
 U.S. copyright law.
 
 A "contribution" is the original software, or any additions or changes to the software.
 A "contributor" is any person that distributes its contribution under this license.
 "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 
 2. Grant of Rights
 (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 
 3. Conditions and Limitations
 (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 your patent license from such contributor to the software ends automatically.
 (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 notices that are present in the software.
 (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
 code form, you may only do so under a license that complies with this license.
 (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
 or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
 permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 purpose and non-infringement.
 */
#endregion License

using System;
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
    }
}
