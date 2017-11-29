using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuildTasks
{
    public class GetMGCBTask : Task
    {
        [DllImport("libc")]
        private static extern int uname(IntPtr buf);

        [Output]
        public string MGCBCommandPath { get; set; }
        
        public string MgcbPath { get; set; }

        private string _os;
        private bool _unix;

        public override bool Execute()
        {
            // Figure out the current system
            _os = "Windows";
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    _unix = true;
                    _os = "Mac";
                    break;
                case PlatformID.Unix:
                    _unix = true;
                    _os = "Linux";
                    try
                    {
                        var buf = Marshal.AllocHGlobal(8192);
                        if (uname(buf) == 0 && Marshal.PtrToStringAnsi(buf) == "Darwin")
                            _os = "Mac";

                        Marshal.FreeHGlobal(buf);
                    }
                    catch { }

                    break;
            }

            // Get MGCB from NuGet
            if (string.IsNullOrEmpty(MgcbPath))
                LocateMGCBFromNuget();

            MGCBCommandPath = _unix ? "mono \"" + MgcbPath + "\"" : MgcbPath;
            return true;
        }

        private void LocateMGCBFromNuget()
        {
            var basedir = Path.GetDirectoryName((new Uri(typeof(GetMGCBTask).Assembly.CodeBase)).LocalPath);
            var targetdir = Path.Combine(basedir, "../../build");
            var mgcbdir = Path.Combine(targetdir, "MGCB");
            var mgcbpath = Path.Combine(mgcbdir, "build/MGCB.exe");

            if (!Directory.Exists(mgcbdir))
            {
                var mgcbzip = Path.Combine(targetdir, "MGCB.zip");
                var version = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(basedir)));
                var webClient = new WebClient();
                var progress = 0;

                // Process probably got terminated before download completed
                if (File.Exists(mgcbzip))
                    File.Delete(mgcbzip);
                
                // Download MGCB for the current platform
                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    if (e.ProgressPercentage != progress)
                    {
                        progress = e.ProgressPercentage;
                        Log.LogMessage(MessageImportance.High, "MGCB for " + _os + " is being downloaded, status: " + progress + "%");
                    }
                };
                webClient.DownloadFile("https://www.nuget.org/api/v2/package/MonoGame.Content.Builder." + _os + "/" + version, mgcbzip);
                
                // Extract MGCB
                ZipFile.ExtractToDirectory(mgcbzip, mgcbdir);
            }

            MgcbPath = mgcbpath;
        }
    }
}

