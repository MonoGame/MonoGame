// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests
{
    internal static class AssetTestUtility
    {
        public static Microsoft.Xna.Framework.Graphics.Effect LoadEffect(ContentManager content, string name)
        {
            return content.Load<Microsoft.Xna.Framework.Graphics.Effect>(Paths.CompiledEffect(name));
        }
    }
}
