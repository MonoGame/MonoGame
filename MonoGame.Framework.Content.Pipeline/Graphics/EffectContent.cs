// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Contains the source code for a DirectX Effect, loaded from a .fx file.
    /// </summary>
    public class EffectContent : ContentItem
    {
        /// <summary>
        /// Initializes a new instance of EffectContent.
        /// </summary>
        public EffectContent()
        {

        }

        /// <summary>
        /// Gets or sets the effect program source code.
        /// </summary>
        public string EffectCode { get; set; }
    }
}
