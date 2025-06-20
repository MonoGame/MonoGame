#pragma warning disable 1591

namespace MonoGame.Framework.Content.Pipeline.Builder.Server;

public class ContentServerParameterAttribute(string name, string description) : Attribute
{
    public string Name { get; set; } = name;

    public string Description { get; set; } = description;
}
