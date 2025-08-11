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
    private readonly Dictionary<string, ContentInfo?> _imputFiles = [];
    private readonly List<(ContentRule rule, ContentInfo? info)> _rules = [];
    private string _contentRoot = "Content";

    /// <summary>
    /// Sets the content root path that will be used when generating the output paths during include methods.
    /// </summary>
    /// <param name="contentRoot">Relative path that will get prefixed to the output paths.</param>
    public void SetContentRoot(string contentRoot) => _contentRoot = contentRoot;

    /// <summary>
    /// Marks the file at inputPath to be copied to the output directory.
    /// </summary>
    /// <param name="inputPath">Relative path to the content file.</param>
    /// <param name="outputPath">Relative path for the copy, if its not specified, the inputPath will be used.</param>
    public void IncludeCopy(string inputPath, string? outputPath)
        => _imputFiles[inputPath] = new(_contentRoot, false, null, null, string.IsNullOrWhiteSpace(outputPath) ? ContentInfo.GetDefaultOutputPath : s => outputPath);

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
    public void Include(string inputPath, IContentImporter? contentImporter = null, IContentProcessor? contentProcessor = null)
        => _imputFiles[inputPath] = new(_contentRoot, true, contentImporter, contentProcessor);

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
    public void Include(string inputPath, string outputPath, IContentImporter? contentImporter = null, IContentProcessor? contentProcessor = null)
        => _imputFiles[inputPath] = new(_contentRoot, true, contentImporter, contentProcessor, (s) => outputPath);

    /// <summary>
    /// Marks the file at excludePath to be excluded from the content handling.
    /// </summary>
    /// <param name="excludePath">Relative path to the content file.</param>
    public void Exclude(string excludePath)
        => _imputFiles[excludePath] = null;

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
    public void IncludeCopy<T>(string includePattern, Func<string, string>? outputPath = null)
        where T : ContentRule, new()
    {
        var rule = new T { Pattern = includePattern };
        RemoveFilesByRule(rule);
        _rules.Add((rule, new(_contentRoot, false, null, null, outputPath)));
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
    public void Include<T>(string includePattern, IContentImporter? contentImporter = null, IContentProcessor? contentProcessor = null)
        where T : ContentRule, new()
    {
        var rule = new T { Pattern = includePattern };
        RemoveFilesByRule(rule);
        _rules.Add((rule, new(_contentRoot, true, contentImporter, contentProcessor)));
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
    /// <param name="contentImporter">
    /// An <see cref="IContentImporter"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate importer.
    /// </param>
    /// <param name="contentProcessor">
    /// An <see cref="IContentProcessor"/> to be used for the building, if its not specified, 
    /// the system will use reflection to try to figure out an apropriate processor.
    /// </param>
    public void Include<T>(string includePattern, Func<string, string> outputPath, IContentImporter? contentImporter = null, IContentProcessor? contentProcessor = null)
        where T : ContentRule, new()
    {
        var rule = new T { Pattern = includePattern };
        RemoveFilesByRule(rule);
        _rules.Add((rule, new(_contentRoot, true, contentImporter, contentProcessor, outputPath)));
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
    public void Exclude<T>(string excludePattern)
        where T : ContentRule, new()
    {
        var rule = new T { Pattern = excludePattern };
        RemoveFilesByRule(rule);
        _rules.Add((rule, null));
    }

    /// <summary>
    /// Gets information about how the ocntent should be handled for the passed relative filepath.
    /// </summary>
    /// <param name="filePath">Relative path to the content file.</param>
    /// <param name="contentInfo"><see cref="ContentInfo"/> describing the desired content handling.</param>
    /// <returns><c>true</c> if the content should be handled, <c>false</c> otherwise.</returns>
    public bool GetContentInfo(string filePath, ref ContentInfo? contentInfo)
    {
        if (_imputFiles.TryGetValue(filePath, out var info))
        {
            if (info == null)
                return false;

            contentInfo = info;
            return true;
        }

        for (int i = _rules.Count - 1; i >= 0; i--)
        {
            (ContentRule rule, ContentInfo? info) rule = _rules[i];
            if (rule.rule.IsMatch(filePath))
            {
                contentInfo = rule.info;
                return rule.info != null;
            }
        }

        return false;
    }

    private void RemoveFilesByRule(ContentRule rule)
    {
        List<string> elementsToRemove = [];

        foreach (var pair in _imputFiles)
        {
            if (rule.IsMatch(pair.Key))
            {
                elementsToRemove.Add(pair.Key);
            }
        }

        foreach (var element in elementsToRemove)
        {
            _imputFiles.Remove(element);
        }
    }
}
