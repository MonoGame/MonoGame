// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        public static bool Linux { get; private set; }
        public static bool UseHeaderBar { get; private set; }
        public static bool Unix { get; private set; }

        static Global()
        {
            Unix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;

            PlatformInit();
        }

        public static string NotAllowedCharacters
        {
            get
            {
                if (Global.Unix)
                    return Global.Linux ? "/" : ":";

                return "/?<>\\:*|\"";
            }
        }

        public static bool CheckString(string s)
        {
            var notAllowed = Path.GetInvalidFileNameChars();

            for (int i = 0; i < notAllowed.Length; i++)
                if (s.Contains(notAllowed[i].ToString()))
                    return false;

            return true;
        }

        public static void ShowOpenWithDialog(string filePath)
        {
            try
            {
                PlatformShowOpenWithDialog(filePath);
            }
            catch
            {
                MainWindow.Instance.ShowError("Error", "The current platform does not have this dialog implemented.");
            }
        }

        public static Image GetDirectoryIcon(bool exists)
        {
            try
            {
                return PlatformGetDirectoryIcon(exists);
            }
            catch { }

            return exists ? Bitmap.FromResource("TreeView.Folder.png") : Bitmap.FromResource("TreeView.FolderMissing.png");
        }

        public static Image GetFileIcon(string path, bool exists)
        {
            try
            {
                return PlatformGetFileIcon(path, exists);
            }
            catch { }

            return exists ? Bitmap.FromResource("TreeView.File.png") : Bitmap.FromResource("TreeView.FileMissing.png");
        }

        public static void SetIcon(Command cmd)
        {
            if (PlatformSetIcon(cmd))
                return;

            switch (cmd.MenuText)
            {
                case "New...":
                    cmd.Image = Icon.FromResource("Toolbar.New.png");
                    break;
                case "Open...":
                    cmd.Image = Icon.FromResource("Toolbar.Open.png");
                    break;
                case "Save...":
                    cmd.Image = Icon.FromResource("Toolbar.Save.png");
                    break;
                case "Undo":
                    cmd.Image = Icon.FromResource("Toolbar.Undo.png");
                    break;
                case "Redo":
                    cmd.Image = Icon.FromResource("Toolbar.Redo.png");
                    break;
                case "New Item...":
                    cmd.Image = Icon.FromResource("Toolbar.NewItem.png");
                    break;
                case "New Folder...":
                    cmd.Image = Icon.FromResource("Toolbar.NewFolder.png");
                    break;
                case "Existing Item...":
                    cmd.Image = Icon.FromResource("Toolbar.ExistingItem.png");
                    break;
                case "Existing Folder...":
                    cmd.Image = Icon.FromResource("Toolbar.ExistingFolder.png");
                    break;
                case "Build":
                    cmd.Image = Icon.FromResource("Toolbar.Build.png");
                    break;
                case "Rebuild":
                    cmd.Image = Icon.FromResource("Toolbar.Rebuild.png");
                    break;
                case "Cancel Build":
                    cmd.Image = Icon.FromResource("Toolbar.CancelBuild.png");
                    break;
                case "Clean":
                    cmd.Image = Icon.FromResource("Toolbar.Clean.png");
                    break;
                case "Filter Output":
                    cmd.Image = Icon.FromResource("Toolbar.FilterOutput.png");
                    break;
            }
        }
    }
}

