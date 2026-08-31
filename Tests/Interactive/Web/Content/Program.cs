using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;
using Content;

Builder builder = new Builder();

if (args is { Length: > 0 })
{
    builder.Run(args);
}
else
{
    ContentBuilderParams builderParams = new ContentBuilderParams()
    {
        Mode = ContentBuilderMode.Builder,
        WorkingDirectory = $"{AppContext.BaseDirectory}../../",
        SourceDirectory = "Assets",
        Platform = TargetPlatform.WebGL2,
    };

    builder.Run(builderParams);
}

return builder.FailedToBuild > 0 ? -1 : 0;
