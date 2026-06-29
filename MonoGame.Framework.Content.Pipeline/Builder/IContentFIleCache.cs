// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// An interface for storing information about the compiled content.
/// </summary>
public interface IContentFileCache
{
    /// <summary>
    /// Adds the specified file as a dependency related to the current content file.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> the added depedency is related to.</param>
    /// <param name="dependencyPath">A relative or absolute path to the dependency file.</param>
    void AddDependency(ContentBuilder builder, string dependencyPath);

    /// <summary>
    /// Adds the specified file cache as a dependency related to the current content file.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> the added depedency is related to.</param>
    /// <param name="fileCache">A dependent file cache.</param>
    void AddDependency(ContentBuilder builder, IContentFileCache fileCache);

    /// <summary>
    /// Adds the specified file as an output file related to the current content file.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> the output file is related to.</param>
    /// <param name="outputPath">A relative or absolute path to the output file.</param>
    void AddOutputFile(ContentBuilder builder, string outputPath);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> the added depedency is related to.</param>
    /// <param name="info">A <see cref="ContentInfo"/> for which to use to check the validity of the cached asset.</param>
    /// <returns><c>true</c> if the file cache is still valid, <c>false</c> otherwise.</returns>
    bool IsValid(ContentBuilder builder, ContentInfo info);
}
