// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides a collection of named references to texture files.
    /// </summary>
    public sealed class TextureReferenceDictionary : NamedValueDictionary<ExternalReference<TextureContent>>
    {
        /// <summary>
        /// Initializes a new instance of TextureReferenceDictionary.
        /// </summary>
        public TextureReferenceDictionary()
        {
        }
    }
}
