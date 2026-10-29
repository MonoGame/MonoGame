// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MonoGame.Framework.Content.Pipeline.Builder;
using NUnit.Framework;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MonoGame.Tests.ContentPipeline
{
    [TestFixture]
    public class BuilderTest
    {
        string MakePath(string path, string append = null)
        {
            if (append != null)
                path = Path.Combine(path, append);

            return FileHelper.NormalizeDirectorySeparators(path);
        }

        [Test]
        public void CommandLineParserTests()
        {
            var args = ContentBuilderParams.Parse(null);
            Assert.AreEqual(ContentBuilderMode.None, args.Mode);

            args = ContentBuilderParams.Parse("");
            Assert.AreEqual(ContentBuilderMode.None, args.Mode);

            args = ContentBuilderParams.Parse("build");
            Assert.AreEqual(ContentBuilderMode.Builder, args.Mode);
            Assert.AreEqual(TargetPlatform.DesktopGL, args.Platform);
            Assert.AreEqual(false, args.CompressContent);
            Assert.AreEqual(false, args.SkipClean);
            Assert.IsTrue(Path.IsPathRooted(args.WorkingDirectory));
            Assert.AreEqual(Directory.GetCurrentDirectory(), args.WorkingDirectory);
            Assert.AreEqual("Content", args.SourceDirectory);
            Assert.AreEqual(MakePath(Directory.GetCurrentDirectory(), "Content"), args.RootedSourceDirectory);
            Assert.AreEqual(MakePath("bin/Content"), args.OutputDirectory);
            Assert.AreEqual(MakePath(Directory.GetCurrentDirectory(), "bin\\Content"), args.RootedOutputDirectory);
            Assert.AreEqual(MakePath("obj/Content"), args.IntermediateDirectory);
            Assert.AreEqual(MakePath(Directory.GetCurrentDirectory(), "obj\\Content"), args.RootedIntermediateDirectory);
            Assert.AreEqual(Environment.ProcessorCount, args.MaxDegreeOfParallelism);

            args = ContentBuilderParams.Parse("build", "-s", "C:/This/Does/Not/Exist");
            Assert.AreEqual(MakePath("C:/This/Does/Not/Exist"), args.SourceDirectory);

            args = ContentBuilderParams.Parse(
                "build",
                "-s", MakePath(Directory.GetCurrentDirectory(), "../Some/Folder"),
                "-o", MakePath(Directory.GetCurrentDirectory(), "Other/Folder"),
                "-i", MakePath(Directory.GetCurrentDirectory(), "Folder")
            );
            Assert.AreEqual(MakePath("../Some/Folder"), args.SourceDirectory);
            Assert.AreEqual(MakePath("Other/Folder"), args.OutputDirectory);
            Assert.AreEqual("Folder", args.IntermediateDirectory);

            args = ContentBuilderParams.Parse("build", "-m", "77");
            Assert.AreEqual(77, args.MaxDegreeOfParallelism);

            args = ContentBuilderParams.Parse("build", "--max-degree-of-parallelism", "666");
            Assert.AreEqual(666, args.MaxDegreeOfParallelism);

            args = ContentBuilderParams.Parse("build", "-m", "0");
            Assert.AreEqual(Environment.ProcessorCount, args.MaxDegreeOfParallelism);

            args = ContentBuilderParams.Parse("build", "--max-degree-of-parallelism", "-16");
            Assert.AreEqual(Environment.ProcessorCount, args.MaxDegreeOfParallelism);

            var normalizedParams = new ContentBuilderParams
            {
                MaxDegreeOfParallelism = 0
            };
            Assert.AreEqual(Environment.ProcessorCount, normalizedParams.MaxDegreeOfParallelism);

            args = ContentBuilderParams.Parse("server");
            Assert.AreEqual(ContentBuilderMode.Server, args.Mode);
        }

        [Test]
        public void ParallelBuild_SharedDependencyContention_BuildsChildOnceAndPropagatesCache()
        {
            using var workspace = new TempWorkspace();
            uint parentCount = 20;
            uint expectedSuccessCount = parentCount * 2;

            workspace.WriteSourceFile("shared.txt", "shared-payload");
            for (var i = 0; i < parentCount; i++)
            {
                workspace.WriteSourceFile($"parent_{i:D2}.txt", $"parent-{i:D2}->shared.txt");
            }

            TextProcessor.Reset();

            var builder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include<RegexRule>(
                    "^parent_.*\\.txt$",
                    () => new TextImporter(),
                    () => new ParentProcessor("shared.txt", assetName: null));

                return collection;
            });

            var succeeded = workspace.RunBuilder(builder, maxDegreeOfParallelism: 8);

            Assert.IsTrue(succeeded);
            Assert.AreEqual(0u, builder.FailedToBuild);
            Assert.AreEqual(expectedSuccessCount, builder.SucceededToBuild);
            Assert.AreEqual(1, TextProcessor.ProcessCount);

            var childContentInfo = new ContentInfo(
                "Content",
                true,
                new TextImporter(),
                new TextProcessor());

            var expectedChildRelDest = $"Content/shared.{childContentInfo.MakeBuildHash():x}.xnb";
            var expectedChildOutputFile = Path.Combine(workspace.OutputDirectory, expectedChildRelDest).Sanitize();

            Assert.IsTrue(File.Exists(expectedChildOutputFile));

            for (var i = 0; i < parentCount; i++)
            {
                var parentRelDest = $"Content/parent_{i:D2}.xnb";
                var parentCache = builder.ContentCache.ReadContentFileCache(builder, parentRelDest) as ContentFileCache;

                Assert.IsNotNull(parentCache);
                Assert.IsTrue(parentCache!.Dependencies.ContainsKey("shared.txt"), $"Parent {i} missing shared.txt dependency");
                Assert.IsTrue(parentCache.Outputs.ContainsKey(expectedChildRelDest), $"Parent {i} missing shared child output file");
            }
        }

        [Test]
        public void ParallelBuild_SharedDependencyFailure_FailsAllWaitingParentsAndSkipsCleanCache()
        {
            using var workspace = new TempWorkspace();
            uint parentCount = 16;

            workspace.WriteSourceFile("keep_me.txt", "keep-payload");

            var seedBuilder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include("keep_me.txt", new TextImporter(), new TextProcessor());
                return collection;
            });

            Assert.IsTrue(workspace.RunBuilder(seedBuilder, maxDegreeOfParallelism: 1));

            var keepMeOutput = Path.Combine(workspace.OutputDirectory, "Content/keep_me.xnb").Sanitize();

            Assert.IsTrue(File.Exists(keepMeOutput));

            workspace.WriteSourceFile("broken_shared.txt", "broken-payload");
            for (var i = 0; i < parentCount; i++)
            {
                workspace.WriteSourceFile($"fail_parent_{i:D2}.txt", $"fail-{i}");
            }

            FailingProcessor.Reset();

            var failingBuilder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include<RegexRule>(
                    "^fail_parent_.*\\.txt$",
                    () => new TextImporter(),
                    () => new FailingParentProcessor("broken_shared.txt"));

                return collection;
            });

            // Use maxDegreeOfParallelism = 4 across 16 parents to verify later batches reuse the current batch failure.
            var succeeded = workspace.RunBuilder(failingBuilder, maxDegreeOfParallelism: 4);

            Assert.IsFalse(succeeded);
            Assert.AreEqual(1, FailingProcessor.AttemptCount);
            Assert.GreaterOrEqual(failingBuilder.FailedToBuild, parentCount);

            // Because FailedToBuild > 0, CleanCache should be skipped so previously built keep_me.xnb remains.
            Assert.IsTrue(File.Exists(keepMeOutput));
        }

        [Test]
        public void ParallelBuild_CopyOnlyAssets_CopiesAllFilesAndRecordsCache()
        {
            using var workspace = new TempWorkspace();
            uint fileCount = 16;

            for (var i = 0; i < fileCount; i++)
            {
                workspace.WriteSourceFile($"raw_{i:D2}.dat", $"raw-test-content-{i:D4}");
            }

            var builder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.IncludeCopy<WildcardRule>("*.dat");

                return collection;
            });

            var succeeded = workspace.RunBuilder(builder, maxDegreeOfParallelism: 8);

            Assert.IsTrue(succeeded);
            Assert.AreEqual(0u, builder.FailedToBuild);
            Assert.AreEqual(fileCount, builder.SucceededToBuild);

            for (var i = 0; i < fileCount; i++)
            {
                var copiedFile = Path.Combine(workspace.OutputDirectory, $"Content/raw_{i:D2}.dat").Sanitize();

                Assert.IsTrue(File.Exists(copiedFile));
                Assert.AreEqual($"raw-test-content-{i:D4}", File.ReadAllText(copiedFile));
            }
        }

        [Test]
        public void ParallelBuild_CustomAssetNameWithoutXnbExtension_AppendsXnb()
        {
            using var workspace = new TempWorkspace();
            workspace.WriteSourceFile("child_custom.txt", "custom-child");
            workspace.WriteSourceFile("parent_custom.txt", "custom-parent");

            TextProcessor.Reset();

            var builder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include(
                    "parent_custom.txt",
                    new TextImporter(),
                    new ParentProcessor("child_custom.txt", assetName: "sub.xnb.dir/my_custom_child"));

                return collection;
            });

            var succeeded = workspace.RunBuilder(builder, maxDegreeOfParallelism: 4);
            Assert.IsTrue(succeeded);

            var childContentInfo = new ContentInfo(
                "Content",
                true,
                new TextImporter(),
                new TextProcessor());

            var expectedRelDest = $"Content/sub.xnb.dir/my_custom_child.{childContentInfo.MakeBuildHash():x}.xnb";
            var expectedOutputFile = Path.Combine(workspace.OutputDirectory, expectedRelDest).Sanitize();

            Assert.IsTrue(File.Exists(expectedOutputFile), $"Expected compiled custom child at {expectedOutputFile}");
        }

        [Test]
        public void ParallelBuild_BuildAndLoadAsset_WhenCachedOnDisk_ReturnsNonNullObjectAndTracksDependency()
        {
            using var workspace = new TempWorkspace();
            workspace.WriteSourceFile("load_child.txt", "loaded-content-42");
            workspace.WriteSourceFile("load_parent.txt", "parent-content-666");

            // Build load_child.txt first so that a ContentFileCache exists.
            var prebuild = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include("load_child.txt", new TextImporter(), new TextProcessor());
                return collection;
            });

            Assert.IsTrue(workspace.RunBuilder(prebuild, maxDegreeOfParallelism: 1));

            // Then build load_parent.txt which calls BuildAndLoadAsset on: load_child.txt
            var parentBuilder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include("load_child.txt", new TextImporter(), new TextProcessor());
                collection.Include("load_parent.txt", new TextImporter(), new LoadingParentProcessor("load_child.txt"));

                return collection;
            });

            Assert.IsTrue(workspace.RunBuilder(parentBuilder, maxDegreeOfParallelism: 4));
            Assert.AreEqual("PROCESSED: loaded-content-42", LoadingParentProcessor.LastLoadedChildValue);

            var parentCache = parentBuilder
                .ContentCache
                .ReadContentFileCache(parentBuilder, "Content/load_parent.xnb") as ContentFileCache;

            Assert.IsNotNull(parentCache);
            Assert.IsTrue(parentCache.Dependencies.ContainsKey("load_child.txt"));
        }

        [Test]
        public void ParallelBuild_CircularDependency_SameThreadAndCrossThread_DetectsCycleWithoutDeadlock()
        {
            using var workspace = new TempWorkspace();

            // Self-cycle: self_cycle.txt -> self_cycle.txt
            workspace.WriteSourceFile("self_cycle.txt", "self_cycle.txt");

            var selfCycleBuilder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include("self_cycle.txt", new TextImporter(), new SelfCycleProcessor());

                return collection;
            });

            var selfSucceeded = workspace.RunBuilder(selfCycleBuilder, maxDegreeOfParallelism: 2);

            Assert.IsFalse(selfSucceeded);
            Assert.GreaterOrEqual(selfCycleBuilder.FailedToBuild, 1u);

            // Cross-thread cycle: cycle_a.txt -> cycle_b.txt and cycle_b.txt -> cycle_a.txt
            using var crossWorkspace = new TempWorkspace();
            crossWorkspace.WriteSourceFile("cycle_a.txt", "cycle_b.txt");
            crossWorkspace.WriteSourceFile("cycle_b.txt", "cycle_a.txt");

            var crossCycleBuilder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include<WildcardRule>("cycle_*.txt",
                    () => new TextImporter(),
                    () => new CrossThreadCycleProcessor());

                return collection;
            });

            var buildTask = Task.Run(() => crossWorkspace.RunBuilder(crossCycleBuilder, maxDegreeOfParallelism: 2));
            var completedInTime = buildTask.Wait(TimeSpan.FromSeconds(10));

            Assert.IsTrue(completedInTime, $"Cross-thread circular dependency deadlocked instead of throwing {nameof(PipelineException)}!");
            Assert.IsFalse(buildTask.Result);
            Assert.GreaterOrEqual(crossCycleBuilder.FailedToBuild, 1u);
        }

        [Test]
        public void ParallelBuild_RuleImporterAndProcessorFactories_IsolateStatePerFile()
        {
            using var workspace = new TempWorkspace();
            uint fileCount = 16;

            for (var i = 0; i < fileCount; i++)
            {
                workspace.WriteSourceFile($"stateful_{i:D2}.txt", $"item-{i:D2}");
            }

            StatefulTestImporter.MismatchCount = 0;
            StatefulTestProcessor.MismatchCount = 0;

            var builder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include<WildcardRule>(
                    "stateful_*.txt",
                    () => new StatefulTestImporter
                    {
                        Prefix = "IMPORTER",
                        Tags = [.. StatefulTestImporter.TagsToUse],
                    },
                    () => new StatefulTestProcessor
                    {
                        Suffix = "PROCESSOR",
                        Metadata =
                        {
                            [StatefulTestProcessor.KeyToUse] = StatefulTestProcessor.ValueToUse,
                        }
                    });

                return collection;
            });

            var succeeded = workspace.RunBuilder(builder, maxDegreeOfParallelism: 8);

            Assert.IsTrue(succeeded);
            Assert.AreEqual(0, StatefulTestImporter.MismatchCount);
            Assert.AreEqual(0, StatefulTestProcessor.MismatchCount);
        }

        [Test]
        public void ParallelBuild_DeterministicCacheOutput_MatchesBetweenSerialAndParallel()
        {
            using var serialWorkspace = new TempWorkspace();
            using var parallelWorkspace = new TempWorkspace();

            // Patrick Stewart's birthday: 1940-07-13 around 17:00:00 BST.
            DateTime fixedTimestamp = new(1940, 7, 13, 16, 0, 0, DateTimeKind.Utc);
            for (var i = 0; i < 16; i++)
            {
                var fileA = serialWorkspace.WriteSourceFile($"asset_{i:D2}.txt", $"payload-{i}");
                var fileB = parallelWorkspace.WriteSourceFile($"asset_{i:D2}.txt", $"payload-{i}");

                File.SetLastWriteTimeUtc(fileA, fixedTimestamp);
                File.SetLastWriteTimeUtc(fileB, fixedTimestamp);
            }

            var importerFactory = () => new TextImporter();
            var processorFactory = () => new TextProcessor();

            var serialBuilder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include<WildcardRule>("asset_*.txt", importerFactory, processorFactory);

                return collection;
            });

            var parallelBuilder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include<WildcardRule>("asset_*.txt", importerFactory, processorFactory);

                return collection;
            });

            Assert.IsTrue(serialWorkspace.RunBuilder(serialBuilder, maxDegreeOfParallelism: 1));
            Assert.IsTrue(parallelWorkspace.RunBuilder(parallelBuilder, maxDegreeOfParallelism: 8));

            // Verify compiled .xnb output bytes are identical between serial and parallel builds.
            for (var i = 0; i < 12; i++)
            {
                var serialBuiltXnb = Path.Combine(serialWorkspace.OutputDirectory, $"Content/asset_{i:D2}.xnb");
                var parallelBuiltXnb = Path.Combine(parallelWorkspace.OutputDirectory, $"Content/asset_{i:D2}.xnb");

                CollectionAssert.AreEqual(File.ReadAllBytes(serialBuiltXnb), File.ReadAllBytes(parallelBuiltXnb));
            }

            // Sanitize output timestamps to be able to compare cache content ignoring timestamp differences.
            var matchPattern = @"(\.xnb:[ \t]+)[^\r\n]+";
            var replacementPattern = "$1<XNB_TIMESTAMP_PLACEHOLDER>";
            var sanitizedSerialCacheYaml = Regex.Replace(
                File.ReadAllText(Path.Combine(serialWorkspace.IntermediateDirectory, "cache.yaml")),
                matchPattern,
                replacementPattern);

            var sanitizedParallelCacheYaml = Regex.Replace(
                File.ReadAllText(Path.Combine(parallelWorkspace.IntermediateDirectory, "cache.yaml")),
                matchPattern,
                replacementPattern);

            Assert.AreEqual(sanitizedSerialCacheYaml, sanitizedParallelCacheYaml);

            // Verify incremental rebuild loads cache cleanly and skips all 12 up-to-date assets.
            TextProcessor.Reset();
            var incrementalParallelBuilder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();
                collection.Include<WildcardRule>("asset_*.txt",
                    () => new TextImporter(),
                    () => new TextProcessor());

                return collection;
            });

            Assert.IsTrue(parallelWorkspace.RunBuilder(incrementalParallelBuilder, maxDegreeOfParallelism: 8));
            Assert.AreEqual(0, TextProcessor.ProcessCount);
            Assert.AreEqual(16u, incrementalParallelBuilder.SucceededToBuild);
        }

        [Test]
        public void ParallelBuild_SelectiveIncrementalRebuild_OnlyTouchedLeafAndDependentsRebuild()
        {
            using var workspace = new TempWorkspace();
            var leafAPath = workspace.WriteSourceFile("leaf_a.txt", "Leaf A v1");
            workspace.WriteSourceFile("leaf_b.txt", "Leaf B v1");

            for (var i = 0; i < 3; i++)
            {
                workspace.WriteSourceFile($"parent_a_{i:D2}.txt", $"Parent A {i:D2}");
            }

            for (var i = 0; i < 9; i++)
            {
                workspace.WriteSourceFile($"parent_b_{i:D2}.txt", $"Parent B {i:D2}");
            }

            TextProcessor.Reset();
            ParentProcessor.Reset();

            TestContentBuilder CreateBuilder() => new(() =>
            {
                var collection = new ContentCollection();

                collection.Include<WildcardRule>(
                    "parent_a_*.txt",
                    () => new TextImporter(),
                    () => new ParentProcessor("leaf_a.txt", assetName: null));

                collection.Include<WildcardRule>(
                    "parent_b_*.txt",
                    () => new TextImporter(),
                    () => new ParentProcessor("leaf_b.txt", assetName: null));

                return collection;
            });

            var initialBuilder = CreateBuilder();

            Assert.IsTrue(workspace.RunBuilder(initialBuilder, maxDegreeOfParallelism: 8));
            Assert.AreEqual(2, TextProcessor.ProcessCount);
            Assert.AreEqual(12, ParentProcessor.ProcessCount);

            // Touch leaf_a.txt with a newer timestamp and rebuild in parallel
            TextProcessor.Reset();
            ParentProcessor.Reset();

            File.WriteAllText(leafAPath, "Leaf A v2");
            File.SetLastWriteTimeUtc(leafAPath, DateTime.UtcNow.AddSeconds(5));

            var incrementalBuilder = CreateBuilder();

            Assert.IsTrue(workspace.RunBuilder(incrementalBuilder, maxDegreeOfParallelism: 8));
            Assert.AreEqual(1, TextProcessor.ProcessCount);
            Assert.AreEqual(3, ParentProcessor.ProcessCount);
        }

        [Test]
        public void ParallelBuild_ContentBuilderLogger_IsolatesAsyncLocalFileStacksAcrossThreads()
        {
            var rootDir = Directory.GetCurrentDirectory();
            var logger = new MonoGame.Framework.Content.Pipeline.Builder.Logger.ContentBuilderLogger
            {
                LoggerRootDirectory = rootDir,
            };

            var errorCount = 0;
            var tasks = new Task[50];
            for (var i = 0; i < tasks.Length; i++)
            {
                var id = i;
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        var parentFile = $"parent_{id:D2}.txt";
                        var childFile = $"child_{id:D2}.txt";

                        logger.PushFile(Path.Combine(rootDir, parentFile));
                        logger.Indent();

                        await Task.Delay(5);

                        if (logger.GetCurrentFilename() != parentFile)
                        {
                            Interlocked.Increment(ref errorCount);
                        }

                        logger.PushFile(Path.Combine(rootDir, childFile));
                        logger.Indent();

                        await Task.Delay(5);

                        if (logger.GetCurrentFilename() != childFile)
                        {
                            Interlocked.Increment(ref errorCount);
                        }

                        logger.Unindent();
                        logger.PopFile();

                        if (logger.GetCurrentFilename() != parentFile)
                        {
                            Interlocked.Increment(ref errorCount);
                        }

                        logger.Unindent();
                        logger.PopFile();

                        if (!string.IsNullOrEmpty(logger.GetCurrentFilename()))
                        {
                            Interlocked.Increment(ref errorCount);
                        }
                    }
                    catch
                    {
                        Interlocked.Increment(ref errorCount);
                    }
                });
            }

            Task.WaitAll(tasks);

            Assert.AreEqual(0, errorCount);
        }

        [Test]
        public void ParallelBuild_SameOutputFromDifferentSources_FailsWithOutputConflict()
        {
            using var workspace = new TempWorkspace();

            // Test for collision in top-level assets.
            workspace.WriteSourceFile("dir_a/collide.txt", "source-a");
            workspace.WriteSourceFile("dir_b/collide.txt", "source-b");

            StatefulTestProcessor.ResetCount();
            var builder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();

                collection.Include(
                    "dir_a/collide.txt",
                    "collide",
                    new TextImporter(),
                    new StatefulTestProcessor
                    {
                        Suffix = "A",
                        Metadata =
                        {
                            [StatefulTestProcessor.KeyToUse] = StatefulTestProcessor.ValueToUse,
                        }
                    });

                collection.Include(
                    "dir_b/collide.txt",
                    "collide",
                    new TextImporter(),
                    new StatefulTestProcessor
                    {
                        Suffix = "B",
                        Metadata =
                        {
                            [StatefulTestProcessor.KeyToUse] = StatefulTestProcessor.ValueToUse,
                        }
                    });

                return collection;
            });

            var topLevelCollisionTestResult = workspace.RunBuilder(builder, maxDegreeOfParallelism: 4);

            Assert.IsFalse(topLevelCollisionTestResult);
            Assert.AreEqual(1u, builder.FailedToBuild);
            Assert.AreEqual(1u, builder.SucceededToBuild);

            // Test for collision in children.
            workspace.WriteSourceFile("parent_1.txt", "Parent 1");
            workspace.WriteSourceFile("parent_2.txt", "Parent 2");

            for (var i = 0; i < 20; i++)
            {
                var childCollisionBuilder = new TestContentBuilder(() =>
                {
                    var collection = new ContentCollection();

                    collection.Include(
                        "parent_1.txt",
                        new TextImporter(),
                        new ParentProcessor("dir_a/collide.txt", assetName: "shared_custom_child"));

                    collection.Include(
                        "parent_2.txt",
                        new TextImporter(),
                        new ParentProcessor("dir_b/collide.txt", assetName: "shared_custom_child"));

                    return collection;
                });

                var childCollisionTestResult = workspace.RunBuilder(childCollisionBuilder, maxDegreeOfParallelism: 4, rebuild: true);

                Assert.IsFalse(childCollisionTestResult);
                Assert.AreEqual(2u, childCollisionBuilder.FailedToBuild);
                Assert.AreEqual(2u, childCollisionBuilder.SucceededToBuild);
            }
        }

        [Test]
        public void ParallelBuild_SharedDependencyCaseInsensitivePath_BuildsChildOnceOnCaseInsensitiveOS()
        {
            if (!OperatingSystem.IsWindows() && !OperatingSystem.IsMacOS())
            {
                Assert.Ignore("Case-insensitive file paths only apply to Windows and macOS.");
            }

            using var workspace = new TempWorkspace();
            workspace.WriteSourceFile("SharedDependency.txt", "case-payload");
            workspace.WriteSourceFile("parent_upper.txt", "upper");
            workspace.WriteSourceFile("parent_lower.txt", "lower");

            TextProcessor.Reset();

            var builder = new TestContentBuilder(() =>
            {
                var collection = new ContentCollection();

                collection.Include(
                    "parent_upper.txt",
                    new TextImporter(),
                    new ParentProcessor("SharedDependency.txt", assetName: null));

                collection.Include(
                    "parent_lower.txt",
                    new TextImporter(),
                    new ParentProcessor("shareddependency.txt", assetName: null));

                return collection;
            });

            var succeeded = workspace.RunBuilder(builder, maxDegreeOfParallelism: 4);

            Assert.IsTrue(succeeded);
            Assert.AreEqual(0u, builder.FailedToBuild);
            Assert.AreEqual(1, TextProcessor.ProcessCount);
        }

        [Test]
        public void ParallelBuild_ContentCompilerAndReflectiveWriter_ConcurrentStressTest()
        {
            var compiler = new ContentCompiler();
            var errorCount = 0;

            Parallel.For(0, 64, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
            {
                try
                {
                    using var memoryStream = new MemoryStream();

                    var payload = new TestContent
                    {
                        Id = i,
                        Title = $"Title_{i}",
                        Values = [i, i + 1, i + 2],
                        SharedChild = new SharedNode
                        {
                            Label = $"Shared_{i}",
                        },
                    };

                    compiler.Compile(
                        memoryStream,
                        payload,
                        TargetPlatform.DesktopGL,
                        Microsoft.Xna.Framework.Graphics.GraphicsProfile.Reach,
                        compressContent: false,
                        rootDirectory: Directory.GetCurrentDirectory(),
                        referenceRelocationPath: Directory.GetCurrentDirectory());

                    Assert.Greater(memoryStream.ToArray().Length, 0);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    TestContext.WriteLine(ex);
                }
            });

            Assert.AreEqual(0, errorCount);
        }
    }
}
