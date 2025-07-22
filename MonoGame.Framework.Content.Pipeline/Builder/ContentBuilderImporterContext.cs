// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder;

class ContentBuilderImporterContext(ContentBuilder builder, ContentFileCache contentFileCache) : ContentImporterContext
{
    private readonly ContentBuilder _builder = builder;

    private readonly ContentFileCache _contentFileCache = contentFileCache;

    public override string IntermediateDirectory => _builder.Parameters.RootedIntermediateDirectory;

    public override ContentBuildLogger Logger => _builder.Logger;

    public override string OutputDirectory => _builder.Parameters.RootedOutputDirectory;

    public override void AddDependency(string filename) => _contentFileCache.AddDependency(_builder, filename);
}
