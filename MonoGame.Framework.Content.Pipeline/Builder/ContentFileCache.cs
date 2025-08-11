// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// Contains cached information about a single source content file.
/// </summary>
public record ContentFileCache
{
    /// <summary>
    /// The ContentRoot that was used for the building of the content file,
    /// </summary>
    public string ContentRoot { get; init; } = "";

    /// <summary>
    /// <c>true</c> if the content was built, <c>false</c> if the content was copied.
    /// </summary>
    public bool ShouldBuild { get; init; } = false;

    /// <summary>
    /// An <see cref="IContentImporter"/> that was used for the building of the content file,
    /// </summary>
    public IContentImporter? Importer { get; init; } = null;

    /// <summary>
    /// An <see cref="IContentProcessor"/> that was used for the building of the content file,
    /// </summary>
    public IContentProcessor? Processor { get; init; } = null;

    /// <summary>
    /// Indicates if the content was compressed.
    /// </summary>
    public bool CompressContent { get; init; } = false;

    /// <summary>
    /// Indicates the <see cref="GraphicsProfile"/> that was used when the content was built.
    /// </summary>
    public GraphicsProfile GraphicsProfile { get; init; } = GraphicsProfile.HiDef;

    /// <summary>
    /// A dictionary of keys of dependency files that either the <see cref="IContentImporter"/> or <see cref="IContentProcessor"/> included
    /// and values of the last modified times for those files.
    /// </summary>
    public Dictionary<string, DateTime> Dependencies { get; init; } = [];

    /// <summary>
    /// A hashset of output files that the <see cref="IContentProcessor"/> included.
    /// </summary>
    public HashSet<string> Outputs { get; init; } = [];

    /// <summary>
    /// Adds the specified file as a dependency related to the current content file.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> the added depedency is related to.</param>
    /// <param name="dependencyPath">A relative or absolute path to the dependency file.</param>
    public void AddDependency(ContentBuilder builder, string dependencyPath)
    {
        string fullDependencyPath;
        string relativeDependencyPath;

        if (Path.IsPathRooted(dependencyPath))
        {
            fullDependencyPath = dependencyPath;
            relativeDependencyPath = Path.GetRelativePath(builder.Parameters.RootedSourceDirectory, dependencyPath);
        }
        else
        {
            fullDependencyPath = Path.Combine(builder.Parameters.RootedSourceDirectory, dependencyPath);
            relativeDependencyPath = dependencyPath;
        }

        if (!File.Exists(fullDependencyPath))
        {
            return;
        }

        var lastModifiedTime = File.GetLastWriteTimeUtc(fullDependencyPath);
        Dependencies[relativeDependencyPath] = lastModifiedTime;
    }

    /// <summary>
    /// Removes the specified file as a dependency related to the current content file.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> the added depedency is related to.</param>
    /// <param name="dependencyPath">A relative or absolute path to the dependency file.</param>
    public void RemoveDependency(ContentBuilder builder, string dependencyPath)
    {
        string relativeDependencyPath = Path.IsPathRooted(dependencyPath) ?
            Path.GetRelativePath(builder.Parameters.RootedSourceDirectory, dependencyPath) :
            dependencyPath;
        Dependencies.Remove(relativeDependencyPath);
    }

    /// <summary>
    /// Adds the specified file as an output file related to the current content file.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> the output file is related to.</param>
    /// <param name="outputPath">A relative or absolute path to the output file.</param>
    public void AddOutputFile(ContentBuilder builder, string outputPath)
    {
        var relativeOutputFile = Path.IsPathRooted(outputPath) ?
            Path.GetRelativePath(builder.Parameters.RootedOutputDirectory, outputPath) :
            outputPath;
        Outputs.Add(relativeOutputFile);
    }

    /// <summary>
    /// Removes the specified file as an output related to the current content file.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> the output file is related to.</param>
    /// <param name="outputPath">A relative or absolute path to the output file.</param>
    public void RemoveOutputFile(ContentBuilder builder, string outputPath)
    {
        var relativeOutputFile = Path.IsPathRooted(outputPath) ?
            Path.GetRelativePath(builder.Parameters.RootedOutputDirectory, outputPath) :
            outputPath;
        Outputs.Remove(relativeOutputFile);
    }
}
