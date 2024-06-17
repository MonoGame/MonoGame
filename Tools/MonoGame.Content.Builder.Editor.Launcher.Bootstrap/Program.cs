// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonoGame.Content.Builder.Editor.Launcher.Bootstrap
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var executable =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "mgcb-editor-windows" :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "mgcb-editor-mac" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "mgcb-editor-linux" :
                throw new NotImplementedException("Unsupported Operating System");

            Process.Start("dotnet", $"{executable} \"{string.Join("\" \"", args)}\"");
        }
    }
}
