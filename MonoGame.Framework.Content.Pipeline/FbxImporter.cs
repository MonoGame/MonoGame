// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reading AutoDesk (.fbx) files for use in the Content Pipeline.
    /// </summary>
    [ContentImporter(".fbx", DisplayName = "Fbx Importer - MonoGame", DefaultProcessor = "ModelProcessor")]
    public class FbxImporter : ContentImporter<NodeContent>
    {
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            var importer = new OpenAssetImporter
            {
                ImporterName = "FbxImporter",
            };

            return importer.Import(filename, context);
        }
    }
}
