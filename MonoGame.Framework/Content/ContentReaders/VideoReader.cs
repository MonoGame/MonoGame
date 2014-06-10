// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class VideoReader : ContentTypeReader<Video>
    {
#if ANDROID
        static string[] supportedExtensions = new string[] { ".3gp", ".mkv", ".mp4", ".ts", ".webm" };
#elif IOS || MONOMAC
        static string[] supportedExtensions = new string[] { ".mp4", ".mov", ".avi", ".m4v", ".3gp" };
#elif WINDOWS || WINRT
        static string[] supportedExtensions = new string[] { ".wma" };
#else
        static string[] supportedExtensions = new string[] { ".mp4", ".mov", ".avi", ".m4v" };
#endif

        internal static string Normalize(string fileName)
        {
            return Normalize(fileName, supportedExtensions);
        }

        protected internal override Video Read(ContentReader input, Video existingInstance)
        {
            string path = input.ReadString();

            if (!string.IsNullOrEmpty(path))
            {
                path = FileHelpers.ResolveRelativePath(input.AssetName, path);
                path = Path.Combine(input.ContentManager.RootDirectoryFullPath, path);
            }

            var durationMS = input.ReadInt32();
            var width = input.ReadInt32();
            var height = input.ReadInt32();
            var framesPerSecond = input.ReadSingle();
            var soundTrackType = input.ReadInt32();   // 0 = Music, 1 = Dialog, 2 = Music and dialog
            return new Video(path, durationMS)
            {
                Width = width,
                Height = height,
                FramesPerSecond = framesPerSecond,
                VideoSoundtrackType = (VideoSoundtrackType)soundTrackType
            };
        }
    }
}
