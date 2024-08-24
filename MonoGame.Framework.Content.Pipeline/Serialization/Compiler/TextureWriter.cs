// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    internal class TextureWriter : BuiltInContentWriter<TextureContent>
    {
        protected internal override void Write(ContentWriter output, TextureContent value)
        {
            // Do nothing.
            // The TextureWriter is not used to write anything, but it is used by
            // the ExternalReferenceWriter when an ExternalReference<TextureContent>
            // is written! (See ExternalReferenceWriter implementation.)
        }
    }
}
