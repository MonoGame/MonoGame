// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    public class PipelineImporterContext : ContentImporterContext
    {
        private readonly PipelineManager _manager;

        public PipelineImporterContext(PipelineManager manager)
        {
            _manager = manager;
        }

        public override string IntermediateDirectory { get { return _manager.IntermediateDirectory; } }
        public override string OutputDirectory { get { return _manager.OutputDirectory; } }
        public override ContentBuildLogger Logger { get { return _manager.Logger; } }

        public override void AddDependency(string filename)
        {            
        }
    }
}