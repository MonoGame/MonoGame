// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    public sealed class ContentCompiler
    {
        // Note: Should be called from ContentTypeWriter.Initialize() method.
        public ContentTypeWriter GetTypeWriter(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
