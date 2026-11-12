// MonoGame - Copyright (C) MonoGame Foundation, Inc
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
        /// <summary>
        /// If true, the custom axis up direction of the FBX model will be ignored.
        /// <defaultValue>true</defaultValue>
        /// </summary>
        public bool IgnoreFbxUpDirection { get; set; } = true;

        /// <summary>
        /// Gets or Sets if the Pivots for an FBX file are preserved or not.
        /// </summary>
        public bool PreservePivots { get; set; } = true; 

        /// <summary>
        /// Gets or Sets if we enable or disable primitive area checks.
        /// Disable this is your model is small and you are finding that 
        /// faces are being removed.
        /// </summary>
        public bool RemoveDegeneratePrimitivesCheckArea { get; set; } = true;

        /// <inheritdoc/>
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            ArgumentNullException.ThrowIfNull(filename);
            ArgumentNullException.ThrowIfNull(context);

            var importer = new OpenAssetImporter("FbxImporter", true)
            {
                PreservePivots = this.PreservePivots,
                IgnoreFbxUpDirection = this.IgnoreFbxUpDirection,
                RemoveDegeneratePrimitivesCheckArea = this.RemoveDegeneratePrimitivesCheckArea,
            };
            return importer.Import(filename, context);
        }
    }
}
