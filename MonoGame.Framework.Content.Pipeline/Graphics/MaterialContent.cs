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

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for maintaining a collection of named texture references.
    /// </summary>
    /// <remarks>In addition to texture references, opaque data values are stored in the OpaqueData property of the base class.</remarks>
    public class MaterialContent : ContentItem
    {
        TextureReferenceDictionary textures;

        /// <summary>
        /// Gets the texture collection of the material.
        /// </summary>
        /// <value>Collection of textures used by the material.</value>
        public TextureReferenceDictionary Textures { get { return textures; } }

        /// <summary>
        /// Initializes a new instance of MaterialContent.
        /// </summary>
        public MaterialContent()
        {
            textures = new TextureReferenceDictionary();
        }

        /// <summary>
        /// Gets a reference type from the OpaqueDataDictionary collection.
        /// </summary>
        /// <typeparam name="T">Type of the related opaque data.</typeparam>
        /// <param name="key">Key of the property being retrieved.</param>
        /// <returns>The related opaque data.</returns>
        protected T GetReferenceTypeProperty<T>(string key) where T : class
        {
            object value;
            if (OpaqueData.TryGetValue(key, out value))
                return (T)value;
            return default(T);
        }

        /// <summary>
        /// Gets a value from the Textures collection.
        /// </summary>
        /// <param name="key">Key of the texture being retrieved.</param>
        /// <returns>Reference to a texture from the collection.</returns>
        protected ExternalReference<TextureContent> GetTexture(string key)
        {
            return textures[key];
        }

        /// <summary>
        /// Gets a value type from the OpaqueDataDictionary collection.
        /// </summary>
        /// <typeparam name="T">Type of the value being retrieved.</typeparam>
        /// <param name="key">Key of the value type being retrieved.</param>
        /// <returns>Index of the value type beng retrieved.</returns>
        protected Nullable<T> GetValueTypeProperty<T>(string key) where T : struct
        {
            object value;
            if (OpaqueData.TryGetValue(key, out value))
                return (T)value;
            return null;
        }

        /// <summary>
        /// Sets a value in the contained OpaqueDataDictionary object.
        /// If null is passed, the value is removed.
        /// </summary>
        /// <typeparam name="T">Type of the element being set.</typeparam>
        /// <param name="key">Name of the key being modified.</param>
        /// <param name="value">Value being set.</param>
        protected void SetProperty<T>(string key, T value)
        {
            if (value != null)
                OpaqueData[key] = value;
            else
                OpaqueData.Remove(key);
        }

        /// <summary>
        /// Sets a value in the contained TextureReferenceDictionary object.
        /// If null is passed, the value is removed.
        /// </summary>
        /// <param name="key">Name of the key being modified.</param>
        /// <param name="value">Value being set.</param>
        /// <remarks>The key value differs depending on the type of attached dictionary.
        /// If attached to a BasicMaterialContent dictionary (which becomes a BasicEffect object at run time), the value for the Texture key is used as the texture for the BasicEffect runtime object. Other keys are ignored.
        /// If attached to a EffectMaterialContent dictionary, key names are the texture names used by the effect. These names are dependent upon the author of the effect object.</remarks>
        protected void SetTexture(string key, ExternalReference<TextureContent> value)
        {
            if (value != null)
                textures[key] = value;
            else
                textures.Remove(key);
        }
    }
}
