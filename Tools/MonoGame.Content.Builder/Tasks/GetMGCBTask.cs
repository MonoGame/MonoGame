using System;
using System.IO;
using System.Net;
using System.IO.Compression;
using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace MSBuildTasks
{
    public class GetMGCBTask : MSBuildTask
    {
        [Required]
        public string Version { get; set; }

        public override bool Execute()
        {
            // Not to self, rewrite this file, it literally checks for no errors...
            // and you know, add some progress printing...
            // and replace the OS detection

            Log.LogMessage(MessageImportance.Normal, "[GetMGCBTask] Start");

            var basedir = Path.GetDirectoryName((new Uri(typeof(GetMGCBTask).Assembly.CodeBase)).LocalPath);
            var targetdir = Path.Combine(basedir, "../../build");
            var mgcbzip = Path.Combine(targetdir, "MGCB.zip");
            var mgcbdir = Path.Combine(targetdir, "MGCB");

            if (!Directory.Exists(mgcbdir))
            {
                var os = Environment.OSVersion.Platform == PlatformID.Unix ? "Linux" : "Windows";

                if (os == "Linux" && Directory.Exists("/Applications"))
                    os = "Mac";

                if (!File.Exists(mgcbzip))
                {
                    Log.LogMessage(MessageImportance.High, "[GetMGCBTask] Downloading MGCB");
                    var webClient = new WebClient();
                    webClient.DownloadFile("https://www.nuget.org/api/v2/package/MonoGame.Content.Builder." + os + "/" + Version, mgcbzip);
                }
                else
                    Log.LogMessage(MessageImportance.Normal, "[GetMGCBTask] File found, skipping download");

                Log.LogMessage(MessageImportance.High, "[GetMGCBTask] Extracting MGCB");
                ZipFile.ExtractToDirectory(mgcbzip, mgcbdir);
            }
            
            Log.LogMessage(MessageImportance.Normal, "[GetMGCBTask] End");

            return true;
        }
    }
}

