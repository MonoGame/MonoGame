// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// <para>Collection of rules on how the content should be handled.</para>
/// <para>
/// The way this system works is by adding each call you make with include/exclude to a list of rules,
/// then during the <see cref="GetContentInfo"/> it will check if the filepath is valid for each one and return the last match it finds.
/// </para>
/// </summary>
public class ContentCollection : IContentCollection
{
    private const string DEFAULT_CONTENT_ROOT = "Content";

    private string _contentRoot = DEFAULT_CONTENT_ROOT;
    private readonly Dictionary<string, ContentCollectionRoot> _content = new()
    {
        [DEFAULT_CONTENT_ROOT] = new ContentCollectionRoot(DEFAULT_CONTENT_ROOT)
    };

    private ContentCollectionRoot CurrentContent
    {
        get
        {
            if (_content.TryGetValue(_contentRoot, out ContentCollectionRoot? val))
            {
                return val;
            }

            var collection = new ContentCollectionRoot(_contentRoot);
            _content[_contentRoot] = collection;
            return collection;
        }
    }

    /// <summary>
    /// Sets the content root path that will be used when generating the output paths during include methods.
    /// </summary>
    /// <param name="contentRoot">Relative path that will get prefixed to the output paths.</param>
    public ContentCollection SetContentRoot(string contentRoot)
    {
        _contentRoot = contentRoot;
        return this;
    }

    /// <summary>
    /// Marks the file at inputPath to be copied to the output directory.
    /// </summary>
    /// <param name="inputPath">Relative path to the content file.</param>
    /// <param name="outputPath">Relative path for the copy, if its not specified, the inputPath will be used.</param>
    public ContentCollection IncludeCopy(string inputPath, string? outputPath)
    {
        CurrentContent.IncludeCopy(inputPath, outputPath);
        return this;
    }

    /// <summary>
    /// Marks the file at inputPath to be built with specified settings.
    /// </summary>
    /// <param name="inputPath">Relative path to the content file.</param>
    /// <param name="contentImporter">
    /// An <see cref="IContentImporter"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate importer.
    /// </param>
    /// <param name="contentProcessor">
    /// An <see cref="IContentProcessor"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate processor.
    /// </param>
    /// <remarks>
    /// Do not use the same <see cref="IContentProcessor"/> or <see cref="IContentProcessor"/> instance if it's stateful and <see cref="ContentBuilderParams.MaxDegreeOfParallelism"/> is greater than 1.
    /// Assets being built concurrently using the same instance may cause issues.
    /// In that case, use transient instances for each.
    /// </remarks>
    public ContentCollection Include(string inputPath, IContentImporter? contentImporter = null, IContentProcessor? contentProcessor = null)
    {
        CurrentContent.Include(inputPath, contentImporter, contentProcessor);
        return this;
    }

    /// <summary>
    /// Marks the file at inputPath to be built with specified settings.
    /// </summary>
    /// <param name="inputPath">Relative path to the content file.</param>
    /// <param name="outputPath">Relative path for the output content, if its not specified, the inputPath will be used with .xnb extension.</param>
    /// <param name="contentImporter">
    /// An <see cref="IContentImporter"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate importer.
    /// </param>
    /// <param name="contentProcessor">
    /// An <see cref="IContentProcessor"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate processor.
    /// </param>
    /// <remarks>
    /// Do not use the same <see cref="IContentProcessor"/> or <see cref="IContentProcessor"/> instance if it's stateful and <see cref="ContentBuilderParams.MaxDegreeOfParallelism"/> is greater than 1.
    /// Assets being built concurrently using the same instance may cause issues.
    /// In that case, use transient instances for each.
    /// </remarks>
    public ContentCollection Include(string inputPath, string outputPath, IContentImporter? contentImporter = null, IContentProcessor? contentProcessor = null)
    {
        CurrentContent.Include(inputPath, outputPath, contentImporter, contentProcessor);
        return this;
    }

    /// <summary>
    /// Marks the file at excludePath to be excluded from the content handling.
    /// </summary>
    /// <param name="excludePath">Relative path to the content file.</param>
    public ContentCollection Exclude(string excludePath)
    {
        CurrentContent.Exclude(excludePath);
        return this;
    }

    /// <summary>
    /// Marks the files that match the includePattern to be copied to the output directory.
    /// </summary>
    /// <typeparam name="T">
    /// A <see cref="ContentRule"/> to be used for matching the includePattern. Some built in options are:
    /// <para><see cref="WildcardRule"/> - specifies that input pattern should be a wildcard.</para>
    /// <para><see cref="RegexRule"/> - specifies that input pattern should be a regex.</para>
    /// </typeparam>
    /// <param name="includePattern">A pattern to use for building of content files.</param>
    /// <param name="outputPath">Relative path for the output content, if its not specified, the relative filePath will be used with .xnb extension.</param>
    public ContentCollection IncludeCopy<T>(string includePattern, Func<string, string>? outputPath = null)
        where T : ContentRule, new()
    {
        CurrentContent.IncludeCopy<T>(includePattern, outputPath);
        return this;
    }

    /// <inheritdoc cref="Include{T}(string, Func{IContentImporter}?, Func{IContentProcessor}?)"/>
    public ContentCollection Include<T>(string includePattern)
        where T : ContentRule, new()
    {
        Include<T>(includePattern, (Func<IContentImporter>?)null, null);
        return this;
    }

    /// <summary>
    /// Marks the files that match the includePattern to be built with the specified settings.
    /// </summary>
    /// <typeparam name="T">
    /// A <see cref="ContentRule"/> to be used for matching the includePattern. Some built in options are:
    /// <para><see cref="WildcardRule"/> - specifies that input pattern should be a wildcard.</para>
    /// <para><see cref="RegexRule"/> - specifies that input pattern should be a regex.</para>
    /// </typeparam>
    /// <param name="includePattern">A pattern to use for building of content files.</param>
    /// <param name="contentImporter">
    /// An <see cref="IContentImporter"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate importer.
    /// </param>
    /// <param name="contentProcessor">
    /// An <see cref="IContentProcessor"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate processor.
    /// </param>
    /// <remarks>
    /// Do not use this overload if you will be passing in a stateful <see cref="IContentProcessor"/> or <see cref="IContentProcessor"/> if <see cref="ContentBuilderParams.MaxDegreeOfParallelism"/> is greater than 1.
    /// Assets being built concurrently using the same instance may cause issues.
    /// In that case, use the overload that accepts factory functions instead, and make sure the instances returned by those functions are transient.
    /// </remarks>
    [Obsolete("Use the overload that accepts factory functions instead.")]
    public ContentCollection Include<T>(string includePattern, IContentImporter? contentImporter = null, IContentProcessor? contentProcessor = null)
        where T : ContentRule, new()
    {
        Include<T>(includePattern,
            contentImporter != null ? () => contentImporter : null,
            contentProcessor != null ? () => contentProcessor : null);
        return this;
    }

    /// <summary>
    /// Marks the files that match the includePattern to be built with the specified settings.
    /// </summary>
    /// <typeparam name="T">
    /// A <see cref="ContentRule"/> to be used for matching the includePattern. Some built in options are:
    /// <para><see cref="WildcardRule"/> - specifies that input pattern should be a wildcard.</para>
    /// <para><see cref="RegexRule"/> - specifies that input pattern should be a regex.</para>
    /// </typeparam>
    /// <param name="includePattern">A pattern to use for building of content files.</param>
    /// <param name="contentImporterFactory">
    /// A factory that creates a fresh, identically configured <see cref="IContentImporter"/> per matched file. If not specified,
    /// the system will use reflection to try to figure out an apropriate importer.
    /// </param>
    /// <param name="contentProcessorFactory">
    /// A factory that creates a fresh, identically configured <see cref="IContentProcessor"/> per matched file. If not specified,
    /// the system will use reflection to try to figure out an apropriate processor.
    /// </param>
    /// <remarks>
    /// Each factory is invoked once per matched file and must return a new, identically configured instance on every call (never a captured shared instance).
    /// This ensures stateful processors or importers are not reused across multiple files, which could cause issues when building content in parallel.
    /// </remarks>
    public ContentCollection Include<T>(string includePattern,
        Func<IContentImporter>? contentImporterFactory = null,
        Func<IContentProcessor>? contentProcessorFactory = null)
        where T : ContentRule, new()
    {
        CurrentContent.Include<T>(includePattern, contentImporterFactory, contentProcessorFactory);
        return this;
    }

    /// <inheritdoc cref="Include{T}(string, Func{string, string}, Func{IContentImporter}?, Func{IContentProcessor}?)"/>
    public ContentCollection Include<T>(string includePattern, Func<string, string> outputPath) where T : ContentRule, new()
        => Include<T>(includePattern, outputPath, (Func<IContentImporter>?)null, null);

    /// <summary>
    /// Marks the files that match the includePattern to be built with the specified settings.
    /// </summary>
    /// <typeparam name="T">
    /// A <see cref="ContentRule"/> to be used for matching the includePattern. Some built in options are:
    /// <para><see cref="WildcardRule"/> - specifies that input pattern should be a wildcard.</para>
    /// <para><see cref="RegexRule"/> - specifies that input pattern should be a regex.</para>
    /// </typeparam>
    /// <param name="includePattern">A pattern to use for building of content files.</param>
    /// <param name="outputPath">A function that gives a relative output filepath for the specified relative content filepath.</param>
    /// <param name="contentImporter">
    /// An <see cref="IContentImporter"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate importer.
    /// </param>
    /// <param name="contentProcessor">
    /// An <see cref="IContentProcessor"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate processor.
    /// </param>
    /// <remarks>
    /// Do not use this overload if you will be passing in a stateful <see cref="IContentProcessor"/> or <see cref="IContentProcessor"/> if <see cref="ContentBuilderParams.MaxDegreeOfParallelism"/> is greater than 1.
    /// Assets being built concurrently using the same instance may cause issues.
    /// In that case, use the overload that accepts factory functions instead, and make sure the instances returned by those functions are transient.
    /// </remarks>
    [Obsolete("Use the overload that accepts factory functions instead to ensure the same instance isn't reused.")]
    public ContentCollection Include<T>(string includePattern,
        Func<string, string> outputPath,
        IContentImporter? contentImporter = null,
        IContentProcessor? contentProcessor = null)
        where T : ContentRule, new()
    {
        Include<T>(includePattern,
            outputPath,
            contentImporter != null ? () => contentImporter : null,
            contentProcessor != null ? () => contentProcessor : null);
        return this;
    }

    /// <summary>
    /// Marks the files that match the includePattern to be built with the specified settings.
    /// </summary>
    /// <typeparam name="T">
    /// A <see cref="ContentRule"/> to be used for matching the includePattern. Some built in options are:
    /// <para><see cref="WildcardRule"/> - specifies that input pattern should be a wildcard.</para>
    /// <para><see cref="RegexRule"/> - specifies that input pattern should be a regex.</para>
    /// </typeparam>
    /// <param name="includePattern">A pattern to use for building of content files.</param>
    /// <param name="outputPath">A function that gives a relative output filepath for the specified relative content filepath.</param>
    /// <param name="contentImporterFactory">
    /// A factory that creates a fresh, identically configured <see cref="IContentImporter"/> per matched file. If not specified,
    /// the system will use reflection to try to figure out an apropriate importer.
    /// </param>
    /// <param name="contentProcessorFactory">
    /// A factory that creates a fresh, identically configured <see cref="IContentProcessor"/> per matched file. If not specified,
    /// the system will use reflection to try to figure out an apropriate processor.
    /// </param>
    /// <remarks>
    /// Each factory is invoked once per matched file and must return a new, identically configured instance on every call (never a captured shared instance).
    /// This ensures stateful processors or importers are not reused across multiple files, which could cause issues when building content in parallel.
    /// </remarks>
    public ContentCollection Include<T>(string includePattern,
        Func<string, string> outputPath,
        Func<IContentImporter>? contentImporterFactory = null,
        Func<IContentProcessor>? contentProcessorFactory = null)
        where T : ContentRule, new()
    {
        CurrentContent.Include<T>(includePattern, outputPath, contentImporterFactory, contentProcessorFactory);
        return this;
    }

    /// <summary>
    /// Marks the files that match the excludePattern to be excluded from the content handling.
    /// </summary>
    /// <typeparam name="T">
    /// A <see cref="ContentRule"/> to be used for matching the includePattern. Some built in options are:
    /// <para><see cref="WildcardRule"/> - specifies that input pattern should be a wildcard.</para>
    /// <para><see cref="RegexRule"/> - specifies that input pattern should be a regex.</para>
    /// </typeparam>
    /// <param name="excludePattern">A pattern to use for exclusion of content files.</param>
    public ContentCollection Exclude<T>(string excludePattern) where T : ContentRule, new()
    {
        CurrentContent.Exclude<T>(excludePattern);
        return this;
    }

    /// <summary>
    /// Gets information about how the ocntent should be handled for the passed relative filepath.
    /// </summary>
    /// <param name="filePath">Relative path to the content file.</param>
    /// <param name="contentInfos">A list of <see cref="ContentInfo"/> describing the desired content handling.</param>
    /// <returns><c>true</c> if the content should be handled, <c>false</c> otherwise.</returns>
    public bool GetContentInfo(string filePath, out List<ContentInfo> contentInfos)
    {
        contentInfos = [];

        foreach (var pair in _content)
        {
            if (!pair.Value.GetContentInfo(filePath, out List<ContentInfo> currentContent))
            {
                continue;
            }

            foreach (var info in currentContent)
            {
                contentInfos.Add(info);
            }
        }

        return contentInfos.Count > 0;
    }
}
