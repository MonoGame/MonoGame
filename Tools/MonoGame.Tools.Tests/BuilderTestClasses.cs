using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MonoGame.Tests.ContentPipeline;

public class SharedNode
{
    public string Label { get; set; } = string.Empty;
}

public class TestContent
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<int> Values { get; set; } = [];

    [ContentSerializer(SharedResource = true)]
    public SharedNode SharedChild { get; set; }
}

internal sealed class TempWorkspace : IDisposable
{
    private readonly string _originalWorkingDirectory;
    public string RootDirectory { get; }
    public string SourceDirectory { get; }
    public string OutputDirectory { get; }
    public string IntermediateDirectory { get; }

    public TempWorkspace()
    {
        _originalWorkingDirectory = Directory.GetCurrentDirectory();

        RootDirectory = Path.Combine(Path.GetTempPath(), $"mg_builder_test_{Guid.NewGuid():N}");
        SourceDirectory = Path.Combine(RootDirectory, "Content");
        OutputDirectory = Path.Combine(RootDirectory, "bin", "Content");
        IntermediateDirectory = Path.Combine(RootDirectory, "obj", "Content");

        Directory.CreateDirectory(SourceDirectory);
        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(IntermediateDirectory);
    }

    public string WriteSourceFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(SourceDirectory, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);

        return fullPath;
    }

    public bool RunBuilder(ContentBuilder builder, int maxDegreeOfParallelism, bool rebuild = false)
    {
        try
        {
            var parameters = new ContentBuilderParams
            {
                Mode = ContentBuilderMode.Builder,
                WorkingDirectory = RootDirectory,
                SourceDirectory = "Content",
                OutputDirectory = "bin/Content",
                IntermediateDirectory = "obj/Content",
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                Rebuild = rebuild,
                LogLevel = LogLevel.Error,
            };

            return builder.Run(parameters);
        }
        finally
        {
            Directory.SetCurrentDirectory(_originalWorkingDirectory);
        }
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalWorkingDirectory);

        try
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore transient cleanup errors in temp folder.
        }
    }
}

internal sealed class TestContentBuilder(Func<IContentCollection> factory) : ContentBuilder
{
    public override IContentCollection GetContentCollection() => factory();
}

public sealed class TextContent(string text, ContentIdentity identity) : ContentItem
{
    public string Text { get; } = text;

    public ContentIdentity ContentIdentity { get; } = identity;
}

[ContentImporter([".txt"], DisplayName = "Test Text Importer")]
public class TextImporter : ContentImporter<TextContent>
{
    public override TextContent Import(string filename, ContentImporterContext context)
    {
        var text = File.ReadAllText(filename);

        return new TextContent(text, new ContentIdentity(filename));
    }
}

[ContentProcessor(DisplayName = "Test Text Processor")]
public class TextProcessor : ContentProcessor<TextContent, string>
{
    private static int _processCount;
    public static int ProcessCount => Volatile.Read(ref _processCount);

    public static void Reset() => Interlocked.Exchange(ref _processCount, 0);

    public override string Process(TextContent input, ContentProcessorContext context)
    {
        Thread.Sleep(30);
        Interlocked.Increment(ref _processCount);

        return $"PROCESSED: {input.Text}";
    }
}

[ContentProcessor(DisplayName = "Parent Processor")]
public class ParentProcessor : ContentProcessor<TextContent, string>
{
    private static int _processCount;
    public static int ProcessCount => Volatile.Read(ref _processCount);

    public static void Reset() => Interlocked.Exchange(ref _processCount, 0);

    public string ChildRelativePath { get; set; } = string.Empty;
    public string AssetName { get; set; }

    public ParentProcessor()
    {
    }

    public ParentProcessor(string childRelativePath, string assetName)
    {
        ChildRelativePath = childRelativePath;
        AssetName = assetName;
    }

    public override string Process(TextContent input, ContentProcessorContext context)
    {
        Interlocked.Increment(ref _processCount);
        var fullChildPath = Path.Combine(context.ProjectDirectory, ChildRelativePath);
        var extRef = new ExternalReference<TextContent>(fullChildPath, input.ContentIdentity);
        var builtRef = context.BuildAsset<TextContent, string>(
            extRef,
            new TextImporter(),
            new TextProcessor(),
            AssetName);

        return $"{input.Text}=>{builtRef.Filename}";
    }
}

[ContentProcessor(DisplayName = "Failing Processor")]
public class FailingProcessor : ContentProcessor<TextContent, string>
{
    private static int _attemptCount;
    public static int AttemptCount => Volatile.Read(ref _attemptCount);

    public static void Reset() => Interlocked.Exchange(ref _attemptCount, 0);

    public override string Process(TextContent input, ContentProcessorContext context)
    {
        Thread.Sleep(25);
        Interlocked.Increment(ref _attemptCount);

        throw new InvalidContentException("Simulated build failure.");
    }
}

[ContentProcessor(DisplayName = "Failing Parent Processor")]
public class FailingParentProcessor : ContentProcessor<TextContent, string>
{
    public string ChildRelativePath { get; set; } = string.Empty;

    public FailingParentProcessor()
    {
    }

    public FailingParentProcessor(string childRelativePath)
    {
        ChildRelativePath = childRelativePath;
    }

    public override string Process(TextContent input, ContentProcessorContext context)
    {
        var fullChildPath = Path.Combine(context.ProjectDirectory, ChildRelativePath);
        var extRef = new ExternalReference<TextContent>(fullChildPath, input.ContentIdentity);
        context.BuildAsset<TextContent, string>(
            extRef,
            new TextImporter(),
            new FailingProcessor());

        return input.Text;
    }
}

[ContentProcessor(DisplayName = "Loading Parent Processor")]
public class LoadingParentProcessor : ContentProcessor<TextContent, string>
{
    public static string LastLoadedChildValue;
    public string ChildRelativePath { get; set; } = string.Empty;

    public LoadingParentProcessor()
    {
    }

    public LoadingParentProcessor(string childRelativePath)
    {
        ChildRelativePath = childRelativePath;
    }

    public override string Process(TextContent input, ContentProcessorContext context)
    {
        var fullChildPath = Path.Combine(context.ProjectDirectory, ChildRelativePath);
        var externalReference = new ExternalReference<TextContent>(fullChildPath, input.ContentIdentity);
        var loaded = context.BuildAndLoadAsset<TextContent, string>(
            externalReference,
            new TextImporter(),
            new TextProcessor());

        LastLoadedChildValue = loaded;

        return $"{input.Text}:{loaded}";
    }
}

[ContentProcessor(DisplayName = "Self-Cycle Processor")]
public class SelfCycleProcessor : ContentProcessor<TextContent, string>
{
    public override string Process(TextContent input, ContentProcessorContext context)
    {
        var targetFile = Path.Combine(context.ProjectDirectory, input.Text.Trim());
        var extRef = new ExternalReference<TextContent>(targetFile, input.ContentIdentity);
        context.BuildAsset<TextContent, string>(
            extRef,
            new TextImporter(),
            this,
            assetName: Path.GetFileNameWithoutExtension(targetFile));

        return input.Text;
    }
}

[ContentProcessor(DisplayName = "Cross-Thread Cycle Processor")]
public class CrossThreadCycleProcessor : ContentProcessor<TextContent, string>
{
    public override string Process(TextContent input, ContentProcessorContext context)
    {
        Thread.Sleep(40);
        var targetFile = Path.Combine(context.ProjectDirectory, input.Text.Trim());
        var extRef = new ExternalReference<TextContent>(targetFile, input.ContentIdentity);
        context.BuildAsset<TextContent, string>(
            extRef,
            new TextImporter(),
            new CrossThreadCycleProcessor(),
            assetName: Path.GetFileNameWithoutExtension(targetFile));

        return input.Text;
    }
}

[ContentImporter([".txt"], DisplayName = "Stateful Test Importer")]
public class StatefulTestImporter : ContentImporter<TextContent>
{
    public static readonly IReadOnlyList<string> TagsToUse = [ "tag1", "tag2" ];
    public static int MismatchCount;
    private string _transientFileMarker;

    public string Prefix { get; set; } = string.Empty;

    public List<string> Tags { get; internal set; } = [];

    public override TextContent Import(string filename, ContentImporterContext context)
    {
        _transientFileMarker = filename;

        Thread.Sleep(16);

        if (_transientFileMarker != filename
            || Tags.Count != 2
            || Tags[0] != TagsToUse[0]
            || Tags[1] != TagsToUse[1])
        {
            Interlocked.Increment(ref MismatchCount);
        }

        var text = File.ReadAllText(filename);

        return new TextContent($"{Prefix}:{text}", new ContentIdentity(filename));
    }
}

[ContentProcessor(DisplayName = "Stateful Test Processor")]
public class StatefulTestProcessor : ContentProcessor<TextContent, string>
{
    public static readonly string KeyToUse = "KeyA";
    public static readonly string ValueToUse = "ValA";
    private static int _processCount;
    public static int ProcessCount => Volatile.Read(ref _processCount);

    public static void ResetCount() => Interlocked.Exchange(ref _processCount, 0);

    public static int MismatchCount;
    private string _transientPayloadMarker;

    public string Suffix { get; set; } = string.Empty;

    public Dictionary<string, string> Metadata { get; } = [];

    public override string Process(TextContent input, ContentProcessorContext context)
    {
        Interlocked.Increment(ref _processCount);
        _transientPayloadMarker = input.Text;

        Thread.Sleep(15);

        if (_transientPayloadMarker != input.Text
            || !Metadata.TryGetValue(KeyToUse, out var val)
            || val != ValueToUse)
        {
            Interlocked.Increment(ref MismatchCount);
        }

        return $"{input.Text}:{Suffix}";
    }
}
