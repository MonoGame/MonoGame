// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder;

public class PipelineImporterContext(PipelineManager manager, PipelineBuildEvent pipelineEvent) : ContentImporterContext
{
    public override string IntermediateDirectory => manager.IntermediateDirectory;

    public override string OutputDirectory => manager.OutputDirectory;

    public override ContentBuildLogger Logger => manager.Logger;

    public override void AddDependency(string filename) => pipelineEvent.Dependencies.AddUnique(filename);
}
