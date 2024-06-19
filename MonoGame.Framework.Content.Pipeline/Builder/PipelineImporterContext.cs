// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    /// <inheritdoc/>
    public class PipelineImporterContext : ContentImporterContext
    {
        private readonly PipelineManager _manager;

        /// <summary>
        /// Initializes a new instance of PipelineImporterContext.
        /// </summary>
        /// <param name="manager">The pipeline manager to associate with this instance.</param>
        public PipelineImporterContext(PipelineManager manager)
        {
            _manager = manager;
        }

        /// <inheritdoc/>
        public override string IntermediateDirectory { get { return _manager.IntermediateDirectory; } }

        /// <inheritdoc/>
        public override string OutputDirectory { get { return _manager.OutputDirectory; } }

        /// <inheritdoc/>
        public override ContentBuildLogger Logger { get { return _manager.Logger; } }

        /// <inheritdoc/>
        public override void AddDependency(string filename)
        {
        }
    }
}
