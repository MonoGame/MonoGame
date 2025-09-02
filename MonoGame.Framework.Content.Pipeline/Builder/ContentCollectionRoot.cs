// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
 
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder;

class ContentCollectionRoot(string contentRoot)
{
    private readonly string _contentRoot = contentRoot;
    private readonly Dictionary<string, ContentInfo?> _imputFiles = [];
    private readonly List<(ContentRule rule, ContentInfo? info)> _rules = [];

    public void IncludeCopy(string inputPath, string? outputPath)
        => _imputFiles[inputPath] = new(_contentRoot, false, null, null, string.IsNullOrWhiteSpace(outputPath) ? (s => s) : (_ => outputPath));

    public void Include(string inputPath, IContentImporter? contentImporter, IContentProcessor? contentProcessor)
        => _imputFiles[inputPath] = new(_contentRoot, true, contentImporter, contentProcessor);

    public void Include(string inputPath, string outputPath, IContentImporter? contentImporter, IContentProcessor? contentProcessor )
        => _imputFiles[inputPath] = new(_contentRoot, true, contentImporter, contentProcessor, _ => outputPath);

    public void Exclude(string excludePath) => _imputFiles[excludePath] = null;

    public void IncludeCopy<T>(string includePattern, Func<string, string>? outputPath)
        where T : ContentRule, new()
    {
        var rule = new T { Pattern = includePattern };
        RemoveFilesByRule(rule, false, outputPath ?? (s => s));
        _rules.Add((rule, new(_contentRoot, false, null, null, outputPath)));
    }

    public void Include<T>(string includePattern, IContentImporter? contentImporter, IContentProcessor? contentProcessor)
        where T : ContentRule, new()
    {
        var rule = new T { Pattern = includePattern };
        RemoveFilesByRule(rule, true, s => s);
        _rules.Add((rule, new(_contentRoot, true, contentImporter, contentProcessor)));
    }

    public void Include<T>(string includePattern, Func<string, string> outputPath, IContentImporter? contentImporter, IContentProcessor? contentProcessor)
        where T : ContentRule, new()
    {
        var rule = new T { Pattern = includePattern };
        RemoveFilesByRule(rule, true, outputPath);
        _rules.Add((rule, new(_contentRoot, true, contentImporter, contentProcessor, outputPath)));
    }

    public void Exclude<T>(string excludePattern)
        where T : ContentRule, new()
    {
        var rule = new T { Pattern = excludePattern };
        RemoveFilesByRule(rule, true, null);
        _rules.Add((rule, null));
    }

    public bool GetContentInfo(string filePath, out List<ContentInfo> contentInfos)
    {
        HashSet<string> usedOutputs = [];
        contentInfos = [];

        bool inputFiles = _imputFiles.TryGetValue(filePath, out ContentInfo? inputFilesInfo);
        bool? shouldBuild = null;

        if (inputFilesInfo != null)
        {
            usedOutputs.Add(filePath.GetDestinationPath(inputFilesInfo.ShouldBuild, inputFilesInfo.GetOutputPath));
            shouldBuild = inputFilesInfo.ShouldBuild;
        }
        else if (inputFiles)
        {
            usedOutputs.Add(filePath.GetDestinationPath(true));
            usedOutputs.Add(filePath.GetDestinationPath(false));
        }

        for (int i = _rules.Count - 1; i >= 0; i--)
        {
            (ContentRule rule, ContentInfo? info) rule = _rules[i];
            if (rule.rule.IsMatch(filePath))
            {
                if (rule.info == null)
                {
                    // Everything before this got discarded, we can exit here.
                    break;
                }

                var outputPath = filePath.GetDestinationPath(rule.info.ShouldBuild, rule.info.GetOutputPath);
                if (usedOutputs.Contains(outputPath))
                {
                    // Output already got used up later in list, nothing for us to do here.
                    continue;
                }

                if (shouldBuild != null && shouldBuild != rule.info.ShouldBuild)
                {
                    continue;
                }

                usedOutputs.Add(outputPath);
                contentInfos.Add(rule.info); // TODO: check for build action
                shouldBuild = rule.info?.ShouldBuild;
            }
        }

        if (inputFilesInfo != null)
        {
            contentInfos.Add(inputFilesInfo);
        }

        return contentInfos.Count > 0;
    }

    private void RemoveFilesByRule(ContentRule rule, bool build, Func<string, string>? outputPathRes)
    {
        List<string> elementsToRemove = [];

        foreach (var pair in _imputFiles)
        {
            if (!rule.IsMatch(pair.Key))
            {
                continue;
            }

            // we want to check the output path, but only when the build action does not change
            if (outputPathRes != null && pair.Value?.ShouldBuild == build)
            {
                var ruleOutput = pair.Key.GetDestinationPath(build, outputPathRes);
                var inputFilesOutput = pair.Key.GetDestinationPath(build, pair.Value.GetOutputPath);

                if (ruleOutput != inputFilesOutput)
                {
                    continue;
                }
            }

            elementsToRemove.Add(pair.Key);
        }

        foreach (var element in elementsToRemove)
        {
            _imputFiles.Remove(element);
        }
    }
}