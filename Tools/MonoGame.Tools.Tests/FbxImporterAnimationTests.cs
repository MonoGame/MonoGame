// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;

namespace MonoGame.Tests.ContentPipeline
{
    class FbxImporterAnimationTests
    {
        internal sealed record AnimationSnapshot(string Source, SortedDictionary<string, AnimationSnapshotEntry> Animations);

        internal sealed record AnimationSnapshotEntry(long DurationTicks, SortedDictionary<string, AnimationKeyframeSnapshot[]> Nodes);

        internal sealed record AnimationKeyframeSnapshot(long TimeTicks, AnimationTransformSnapshot Transform);

        internal sealed record AnimationTransformSnapshot(
            float M11,
            float M12,
            float M13,
            float M14,
            float M21,
            float M22,
            float M23,
            float M24,
            float M31,
            float M32,
            float M33,
            float M34,
            float M41,
            float M42,
            float M43,
            float M44);

        private void SaveAnimations(string file, IEnumerable<NodeContent> allnodes)
        {
            var animations = new SortedDictionary<string, AnimationSnapshotEntry>(StringComparer.Ordinal);
            foreach (var node in allnodes)
            {
                foreach (var (animationName, animation) in node.Animations.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                {
                    var channels = new SortedDictionary<string, AnimationKeyframeSnapshot[]>(StringComparer.Ordinal);
                    foreach (var (nodeName, channel) in animation.Channels.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                    {
                        channels.Add(nodeName, channel.Select(keyframe => new AnimationKeyframeSnapshot(
                            keyframe.Time.Ticks,
                            new AnimationTransformSnapshot(
                                keyframe.Transform.M11,
                                keyframe.Transform.M12,
                                keyframe.Transform.M13,
                                keyframe.Transform.M14,
                                keyframe.Transform.M21,
                                keyframe.Transform.M22,
                                keyframe.Transform.M23,
                                keyframe.Transform.M24,
                                keyframe.Transform.M31,
                                keyframe.Transform.M32,
                                keyframe.Transform.M33,
                                keyframe.Transform.M34,
                                keyframe.Transform.M41,
                                keyframe.Transform.M42,
                                keyframe.Transform.M43,
                                keyframe.Transform.M44))).ToArray());
                    }

                    animations.Add(node.Name + "." + animationName, new AnimationSnapshotEntry(animation.Duration.Ticks, channels));
                }
            }

            var snapshotJsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var snapshot = new AnimationSnapshot(Path.GetFileName(file), animations);
            File.WriteAllText(file, JsonSerializer.Serialize(snapshot, snapshotJsonOptions));

        }

        [Test]
        [TestCase("Assets/Models/run.fbx", "Assets/Models/run.json")]
        [TestCase("Assets/Models/idle.fbx", "Assets/Models/idle.json")]
        [TestCase("Assets/Models/jump.fbx", "Assets/Models/jump.json")]
        public void CheckImportedAnimations(string fbx, string json)
        {
            var context = new TestImporterContext("TestObj", "TestBin");
            var importer = new FbxImporter()
            {
                PreservePivots = false,
            };

            var nodeContent = importer.Import(fbx, context);
            Assert.IsNull(nodeContent.Parent);
            Assert.IsInstanceOf<NodeContent>(nodeContent);

            var allnodes = nodeContent.AsEnumerable().SelectDeep((n) => n.Children);
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MG_UPDATE_ANIM")))
                SaveAnimations(json, allnodes);

            var snapshotJsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var deserialized = JsonSerializer.Deserialize<AnimationSnapshot>(File.ReadAllText(json), snapshotJsonOptions)
                ?? throw new InvalidDataException($"Could not read animation snapshot: {json}");

            foreach (var node in allnodes)
            {
                foreach (var (animationName, animation) in node.Animations.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                {
                    if (!deserialized.Animations.TryGetValue(node.Name + "." + animationName, out var snapshotEntry))
                    {
                        continue;
                    }
                    if (snapshotEntry.DurationTicks != animation.Duration.Ticks)
                    {
                        Assert.Fail($"Animation duration mismatch for: {node.Name}.{animationName}");
                    }
                    foreach (var (nodeName, keyframes) in animation.Channels)
                    {
                        if (!snapshotEntry.Nodes.TryGetValue(nodeName, out var snapshotKeyframes))
                        {
                            Assert.Fail($"Animation keyframes missing for: {node.Name}.{animationName}.{nodeName}");
                        }
                        Assert.AreEqual(snapshotKeyframes.Length, keyframes.Count, $"Animation keyframe count mismatch for: {node.Name}.{animationName}.{nodeName}");
                        int i = 0;
                        foreach (var kf in keyframes)
                        {
                            var snapshotKf = snapshotKeyframes[i];
                            Assert.AreEqual(snapshotKf.TimeTicks, kf.Time.Ticks, $"Animation keyframe time mismatch for: {node.Name}.{animationName}.{nodeName}");
                            if (snapshotKf.Transform.M11 != kf.Transform.M11 ||
                                snapshotKf.Transform.M12 != kf.Transform.M12 ||
                                snapshotKf.Transform.M13 != kf.Transform.M13 ||
                                snapshotKf.Transform.M14 != kf.Transform.M14 ||
                                snapshotKf.Transform.M21 != kf.Transform.M21 ||
                                snapshotKf.Transform.M22 != kf.Transform.M22 ||
                                snapshotKf.Transform.M23 != kf.Transform.M23 ||
                                snapshotKf.Transform.M24 != kf.Transform.M24 ||
                                snapshotKf.Transform.M31 != kf.Transform.M31 ||
                                snapshotKf.Transform.M32 != kf.Transform.M32 ||
                                snapshotKf.Transform.M33 != kf.Transform.M33 ||
                                snapshotKf.Transform.M34 != kf.Transform.M34 ||
                                snapshotKf.Transform.M41 != kf.Transform.M41 ||
                                snapshotKf.Transform.M42 != kf.Transform.M42 ||
                                snapshotKf.Transform.M43 != kf.Transform.M43 ||
                                snapshotKf.Transform.M44 != kf.Transform.M44)
                            {
                                Assert.Fail($"Animation keyframe transform mismatch for: {node.Name}.{animationName}.{nodeName}");
                            }
                            i++;
                        }
                    }
                }
            }
        }
    }
}