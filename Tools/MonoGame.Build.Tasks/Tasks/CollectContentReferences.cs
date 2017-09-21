// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MonoGame.Build.Tasks
{
    public static class StringExt
    {
        public static string NormalisePath(this string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }
    }

    public class CollectContentReferences : Task
    {
        [Required]
        public ITaskItem[] ContentReferences { get; set; }

        [Required]
        public string MonoGamePlatform { get; set; }

        [Output]
        public ITaskItem[] Output { get; set; }

        public override bool Execute()
        {
            var output = new List<ITaskItem>();
            foreach (var content in ContentReferences)
            {
                var relative = content.GetMetadata("RelativeDir").NormalisePath();
                var fp = content.GetMetadata("FullPath").NormalisePath();
                var link = content.GetMetadata("Link").NormalisePath();
                var metaData = new Dictionary<string, string>();
                var contentFolder = String.Empty;
                if (!string.IsNullOrEmpty(link))
                {
                    contentFolder = Path.GetFileName(Path.GetDirectoryName(link));
                }
                else if (!string.IsNullOrEmpty(relative))
                {
                    contentFolder = Path.GetFileName(Path.GetDirectoryName(relative));
                }
                var outputPath = Path.GetFileNameWithoutExtension(fp);
                if (!outputPath.EndsWith(contentFolder, StringComparison.OrdinalIgnoreCase))
                    outputPath = Path.Combine(outputPath, contentFolder);
                metaData.Add("ContentDirectory", !string.IsNullOrEmpty(contentFolder) ? contentFolder + Path.DirectorySeparatorChar : "");
                metaData.Add("RelativeFullPath", !string.IsNullOrEmpty(relative) ? Path.GetFullPath(relative) : "");
                metaData.Add("ContentOutputDir", Path.Combine("bin", MonoGamePlatform, outputPath));
                metaData.Add("ContentIntermediateOutputDir", Path.Combine("obj", MonoGamePlatform, outputPath));
                output.Add(new TaskItem(fp, metaData));
            }
            Output = output.ToArray();
            return !Log.HasLoggedErrors;
        }
    }
}
