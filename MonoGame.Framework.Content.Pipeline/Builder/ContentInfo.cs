// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// Describes how the <see cref="ContentBuilder"/> should handle the content.
/// </summary>
/// <param name="shouldBuild">Indicates if the content should be built or copied.</param>
/// <param name="contentRoot">The desired content root that will get prefixed to the output path.</param>
/// <param name="importer">The desired <see cref="IContentImporter"/> to be used for the content building.</param>
/// <param name="processor">The desired <see cref="IContentProcessor"/> to be used for the content building.</param>
/// <param name="outputPath">The desired output path to be setup based on the input path to the content.</param>
public class ContentInfo(string contentRoot = "", bool shouldBuild = true, IContentImporter? importer = null, IContentProcessor? processor = null, Func<string, string>? outputPath = null)
{
    private readonly Func<string, string> _outputPath = outputPath ?? (shouldBuild ? GetDefaultOutputPath : GetDefaultCopyPath);

    /// <summary>
    /// A relative path to be used as a prefix to the output path.
    /// </summary>
    public string ContentRoot { get; init; } = contentRoot;

    /// <summary>
    /// <c>true</c> if the content should be built, <c>false</c> if the content should be copied.
    /// </summary>
    public bool ShouldBuild { get; init; } = shouldBuild;

    /// <summary>
    /// An <see cref="IContentImporter"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate importer.
    /// </summary>
    public IContentImporter? Importer { get; init; } = importer;

    /// <summary>
    /// An <see cref="IContentProcessor"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate processor.
    /// </summary>
    public IContentProcessor? Processor { get; init; } = processor;

    /// <summary>
    /// Gets the desired output path for the current <see cref="ShouldBuild"/> operation.
    /// </summary>
    /// <param name="filePath">A relative path to the content file.</param>
    /// <returns>Desired relative path for the output content.</returns>
    public string GetOutputPath(string filePath) => _outputPath(filePath);

    /// <summary>
    /// Gets the default relative output filepath when building content. By default only the extension gets replaced with .xnb extension.
    /// </summary>
    /// <param name="filePath">A relative path to the content file.</param>
    /// <returns>Desired relative path for the built content.</returns>
    public static string GetDefaultOutputPath(string filePath)
    {
        var extLength = Path.GetExtension(filePath).Length;
        if (extLength > 0)
        {
            filePath = filePath[..^extLength];
        }

        return filePath + ".xnb";
    }

    /// <summary>
    /// Gets the default relative output filepath when copying content. By default input and output relative paths match.
    /// </summary>
    /// <param name="filePath">A relative path to the content file.</param>
    /// <returns>Desired relative path for the coppied content.</returns>
    public static string GetDefaultCopyPath(string filePath) => filePath;
}
