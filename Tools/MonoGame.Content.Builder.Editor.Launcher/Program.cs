// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoGame.Content.Builder.Editor.Launcher
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var mgcbEditorApp =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(currentPath, "mgcb-editor-windows-data", "mgcb-editor-windows.exe") :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Path.Combine(currentPath, "MGCB Editor.app") :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.Combine(currentPath, "mgcb-editor-linux-data", "mgcb-editor-linux") :
                throw new NotImplementedException("Unsupported Operating System");
            var mgcbEditorArgs = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]) ? $"\"{args[0]}\"" : "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Process.Start("open", $"-n \"{mgcbEditorApp}\" --args {mgcbEditorArgs}");
            else
                Process.Start(mgcbEditorApp, mgcbEditorArgs);
        }
    }
}
