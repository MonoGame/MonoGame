// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading AutoDesk (.fbx) files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(".fbx", DisplayName = "Fbx Importer - MonoGame", DefaultProcessor = "ModelProcessor")]
    public class FbxImporter : OpenAssetImporter
    {
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            return base.Import(filename, context);
        }
    }
}
