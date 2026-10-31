// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline;

/// <summary>
/// Provides properties describing the origin of the game asset, such as the original source file and creation tool. This information is used for error reporting, and by processors that need to determine from what directory the asset was originally loaded.
/// </summary>
/// <remarks>
/// Initializes a new instance of ContentIdentity with the specified values.
/// </remarks>
/// <param name="sourceFilename">The absolute path to the file name of the asset source.</param>
/// <param name="sourceTool">The name of the digital content creation (DCC) tool that created the asset.</param>
/// <param name="fragmentIdentifier">Specific location of the content item within the larger source file. For example, this could be a line number in the file.</param>
[Serializable]
public class ContentIdentity(string sourceFilename = "", string? sourceTool = "", string? fragmentIdentifier = null)
{
    /// <summary>
    /// Gets or sets the specific location of the content item within the larger source file.
    /// </summary>
    public string? FragmentIdentifier { get; set; } = fragmentIdentifier;

    /// <summary>
    /// Gets or sets the file name of the asset source.
    /// </summary>
    public string SourceFilename { get; set; } = sourceFilename;

    /// <summary>
    /// Gets or sets the creation tool of the asset.
    /// </summary>
    public string? SourceTool { get; set; } = sourceTool;
}
