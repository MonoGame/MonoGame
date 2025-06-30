// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Content.Pipeline.Builder.Server;

/// <summary>
/// An ttribute used to specify that a property should be displaying as a cli argument.
/// </summary>
/// <param name="name">Name of the parameter to be shown in the cli</param>
/// <param name="description">Description for the parameter to be shown in the cli</param>
[AttributeUsage(AttributeTargets.Property)]
public class ContentServerParameterAttribute(string name, string description) : Attribute
{
    /// <summary>
    /// Name of the parameter to be shown in the cli.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// Description for the parameter to be shown in the cli.
    /// </summary>
    public string Description { get; set; } = description;
}
