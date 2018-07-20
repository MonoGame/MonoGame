// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MonoGame.Packaging
{
    public class BuildFlatpakTask : Task
    {
        [Required]
        public string IntermediateDir { get; set; }

        [Required]
        public string OutputPath { get; set; }

        [Required]
        public string ProjectDir { get; set; }

        [Required]
        public string PublishDir { get; set; }

        [Required]
        public string AssemblyName { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Id { get; set; }

        [Required]
        public string IconPath { get; set; }

        public override bool Execute()
        {
            try
            {
                Log.LogMessage(MessageImportance.Normal, "BuildFlatpakTask: Starting...");

                // Ensure that flatpak command is installed
                if (!IsFlatpakInstalled())
                {
                    Log.LogMessage(MessageImportance.High, "Flatpak command not found :(");
                    return false;
                }

                // Ensure that flatpak runtimes are installed
                if (!AreFlatpakRuntimesInstalled())
                {
                    Log.LogMessage(MessageImportance.High, "Error, the requred flatpak runtime components were not found:");
                    Log.LogMessage(MessageImportance.High, " - org.freedesktop.Platform/x86_64/1.6");
                    Log.LogMessage(MessageImportance.High, " - org.freedesktop.Sdk/x86_64/1.6");
                    return false;
                }

                // Setup intermediate directory
                var objpath = Path.Combine(ProjectDir, IntermediateDir, "Flatpak");
                if (Directory.Exists(objpath))
                    Directory.Delete(objpath, true);
                Directory.CreateDirectory(objpath);

                // Generate metadata
                Log.LogMessage(MessageImportance.Normal, "BuildFlatpakTask: Generating metadata");
                var metadatalines = new[] {
                    "[Application]",
                    "name=" + Id,
                    "runtime=org.freedesktop.Platform/x86_64/1.6",
                    "command=/app/opt/" + AssemblyName + "/" + AssemblyName,
                    "",
                    "[Context]",
                    "shared=ipc;network;",
                    "sockets=x11;wayland;pulseaudio;",
                    "devices=all;"
                };
                File.WriteAllLines(Path.Combine(objpath, "metadata"), metadatalines);

                // Copy over icon
                Log.LogMessage(MessageImportance.Normal, "BuildFlatpakTask: Copying over icon");
                var icondir = Path.Combine(objpath, "export/share/icons");
                Directory.CreateDirectory(icondir);
                File.Copy(Path.Combine(ProjectDir, IconPath), Path.Combine(icondir, Id + ".png"));

                // Generate .desktop launcher file
                Log.LogMessage(MessageImportance.Normal, "BuildFlatpakTask: Generating launcher file");
                var desktopdir = Path.Combine(objpath, "export/share/applications");
                Directory.CreateDirectory(desktopdir);
                var desktoplines = new[] {
                    "[Desktop Entry]",
                    "Name=" + Title,
                    "Exec=/app/opt/" + AssemblyName + "/" + AssemblyName,
                    "Type=Application",
                    "Icon=" + Id,
                    "Categories=Game;"
                };
                File.WriteAllLines(Path.Combine(desktopdir, Id + ".desktop"), desktoplines);

                // Copy over game
                Log.LogMessage(MessageImportance.Normal, "BuildFlatpakTask: Copying over game data");
                var sourcegamedir = Path.Combine(ProjectDir, PublishDir);
                var gamedir = Path.Combine(objpath, "files/opt/" + AssemblyName + "/");
                Directory.CreateDirectory(gamedir);
                foreach (var dirpath in Directory.GetDirectories(sourcegamedir, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirpath.Replace(sourcegamedir, gamedir));
                foreach (var filepath in Directory.GetFiles(sourcegamedir, "*.*", SearchOption.AllDirectories))
                    File.Copy(filepath, filepath.Replace(sourcegamedir, gamedir));

                // Copy over suplementary libraries
                var libdir = Path.Combine(Path.GetDirectoryName((new Uri(typeof(BuildFlatpakTask).Assembly.CodeBase)).LocalPath), "../../extra");
                Directory.CreateDirectory(Path.Combine(objpath, "files/lib"));
                File.Copy(Path.Combine(libdir, "libunwind-x86_64.so.8"), Path.Combine(objpath, "files/lib/libunwind-x86_64.so.8"));
                File.Copy(Path.Combine(libdir, "libunwind.so.8"), Path.Combine(objpath, "files/lib/libunwind.so.8"));
                File.Copy(Path.Combine(libdir, "libSDL2-2.0.so.0.8.0"), Path.Combine(objpath, "files/opt/" + AssemblyName + "/libSDL2-2.so"), true);

                // Generate flatpak
                Log.LogMessage(MessageImportance.Normal, "BuildFlatpakTask: Generating flatpak");
                var repodir = Path.Combine(ProjectDir, IntermediateDir, "FlatpakRepo");
                if (Directory.Exists(repodir))
                    Directory.Delete(repodir, true);
                Directory.CreateDirectory(repodir);
                var flatpak = Path.Combine(ProjectDir, OutputPath, AssemblyName + ".flatpak");
                CallFlatpak("build-export " + repodir + " " + objpath);
                CallFlatpak("build-bundle " + repodir + " " + flatpak + " " + Id);

                Log.LogMessage(MessageImportance.High, "BuildFlatpakTask: Flatpak generated at: " + flatpak);
            }
            catch (Exception e)
            {
                Log.LogMessage(MessageImportance.High, "An error occured while trying to package the project into a flatpak.");
                Log.LogMessage(MessageImportance.High, "Error Info: " + e.ToString());

                return false;
            }

            return true;
        }

        private bool IsFlatpakInstalled()
        {
            var proc = new Process();
            proc.StartInfo.FileName = "which";
            proc.StartInfo.Arguments = "flatpak";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.StandardOutput.ReadToEnd();

            return proc.ExitCode == 0;
        }

        private bool AreFlatpakRuntimesInstalled()
        {
            var rt = false;
            var rtsdk = false;
            var proc = new Process();
            proc.StartInfo.FileName = "flatpak";
            proc.StartInfo.Arguments = "list";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                var line = proc.StandardOutput.ReadLine();
                if (line.Contains("org.freedesktop.Platform/x86_64/1.6"))
                    rt = true;
                else if (line.Contains("org.freedesktop.Sdk/x86_64/1.6"))
                    rtsdk = true;
            }

            return rt && rtsdk;
        }

        private void CallFlatpak(string args)
        {
            var proc = new Process();
            proc.StartInfo.FileName = "flatpak";
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
                Log.LogMessage(MessageImportance.Normal, proc.StandardOutput.ReadLine());
        }
    }
}
