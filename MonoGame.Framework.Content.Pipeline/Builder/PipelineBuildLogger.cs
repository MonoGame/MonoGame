﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    public class PipelineBuildLogger : ContentBuildLogger
    {
        public override void LogMessage(string message, params object[] messageArgs)
        {
            Console.WriteLine(message, messageArgs);
        }

        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
            // TODO: How do i make it high importance?
            Console.WriteLine(message, messageArgs);
        }

        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {
            var msg = string.Format(message, messageArgs);
            var fileName = GetCurrentFilename(contentIdentity);
            Console.WriteLine("{0}: {1}", fileName, msg);
        }
    }
}