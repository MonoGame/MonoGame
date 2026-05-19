// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// An interface for storing information about the compiled content.
/// </summary>
public interface IContentCache
{
    /// <summary>
    /// Loads the content cache for the specified builder.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> that the cache file is related to.</param>
    void LoadCache(ContentBuilder builder);

    /// <summary>
    /// Saves any pending content cache information to disk for the specified builder.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> that the cache file is related to.</param>
    void FlushCache(ContentBuilder builder);

    /// <summary>
    /// Creates a new insteance of <see cref="IContentFileCache" /> that the current implementation of <see cref="IContentCache"/> uses.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> that the cache file is related to.</param>
    /// <param name="info">A <see cref="ContentInfo"/> for which to create the <see cref="IContentFileCache"/> for.</param>
    /// <returns></returns>
    IContentFileCache CreateContentFileCache(ContentBuilder builder, ContentInfo info);

    /// <summary>
    /// Reads (from memory or disk) cached information about the content file and returns it if its valid.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> that the cache file is related to.</param>
    /// <param name="relativeDstPath">A relative path to the content file.</param>
    /// <returns>An instance of <see cref="ContentFileCache"/> if cached information about the content is found, <c>null</c> otherwise.</returns>
    IContentFileCache? ReadContentFileCache(ContentBuilder builder, string relativeDstPath);

    /// <summary>
    /// Writes down (to memory or disk) information about the content file.
    /// 
    /// Do note that <see cref="FlushCache"/> will always be called before exiting the content builder.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> that the cache file is related to.</param>
    /// <param name="relativeDstPath">A relative path to the content file.</param>
    /// <param name="fileCache">An insteance of <see cref="IContentFileCache"/> that contains information about the built content.</param>
    void WriteContentFileCache(ContentBuilder builder, string relativeDstPath, IContentFileCache? fileCache);

    /// <summary>
    /// Marks the specified <see cref="IContentFileCache"/> as being used by the <see cref="ContentBuilder" /> and
    /// that is should not get cleaned up by the <see cref="CleanCache"/>
    /// </summary>
    /// <param name="fileCache">An insteance of <see cref="IContentFileCache"/> that contains information about the built content.</param>
    void MarkUsed(IContentFileCache fileCache);

    /// <summary>
    /// Clears out any unused content files from the cache and the disk that were not marked by <see cref="MarkUsed" />.
    /// </summary>
    /// <param name="builder">A <see cref="ContentBuilder"/> that the cache file is related to.</param>
    void CleanCache(ContentBuilder builder);
}
