// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for maintaining a collection of named texture references.
    /// </summary>
    /// <remarks>In addition to texture references, opaque data values are stored in the OpaqueData property of the base class.</remarks>
    public class MaterialContent : ContentItem
    {
        readonly TextureReferenceDictionary _textures;

        /// <summary>
        /// Gets the texture collection of the material.
        /// </summary>
        /// <value>Collection of textures used by the material.</value>
        public TextureReferenceDictionary Textures { get { return _textures; } }

        /// <summary>
        /// Initializes a new instance of MaterialContent.
        /// </summary>
        public MaterialContent()
        {
            _textures = new TextureReferenceDictionary();
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
            ExternalReference<TextureContent> texture;
            _textures.TryGetValue(key, out texture);
            return texture;
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
                _textures[key] = value;
            else
                _textures.Remove(key);
        }

        /// <summary>
        /// Helper method to make a copy of a material.
        /// </summary>
        /// <returns>A clone of the material.</returns>
        public MaterialContent Clone()
        {
            // Construct it via reflection.
            var clone = (MaterialContent)Activator.CreateInstance(GetType());

            // Give it the same identity as the original material.
            clone.Name = Name;
            clone.Identity = Identity;

            // Just copy the opaque data and textures which should
            // result in the same properties being set if the material
            // is implemented correctly.
            foreach (var pair in Textures)
                clone.Textures.Add(pair.Key, pair.Value);            
            foreach (var pair in OpaqueData)
                clone.OpaqueData.Add(pair.Key, pair.Value);

            return clone;
        }
    }
}
