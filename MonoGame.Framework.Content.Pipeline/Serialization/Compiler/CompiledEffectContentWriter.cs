﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class CompiledEffectContentWriter : BuiltInContentWriter<CompiledEffectContent>
    {
        protected internal override void Write(ContentWriter output, CompiledEffectContent value)
        {
            var code = value.GetEffectCode();
            output.Write(code.Length);
            output.Write(code);
        }
    }
}
