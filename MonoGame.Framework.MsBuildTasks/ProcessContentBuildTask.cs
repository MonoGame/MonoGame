using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MonoGame.Framework.MsBuildTasks
{
    public class ProcessContentBuildTask : ITask
    {

        private static readonly char[] PathSperators = new char[] { '/', '\\' };

        private System.Collections.Concurrent.ConcurrentBag<ITaskItem> _content;


        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        [Output]
        public ITaskItem[] ExtraContent { get; set; }

        [Required]
        public string ProjectDirectory { get; set; }

        [Required]
        public string MgcbExePath { get; set; }

        [Required]
        public ITaskItem[] ContentReferenceItems { get; set; }

        [Required]
        public string Platform { get; set; }

        public string PlatformResourcePrefix { get; set; }

        [Required]
        public string IntermediateOutputPath { get; set; }

        [Required]
        public string Configuration { get; set; }


        public bool Execute()
        {
            _content = new ConcurrentBag<ITaskItem>();

            var platformArgs = string.Format("/platform:{0}", Platform);

            //  var tasks = new List<Tuple<ITaskItem, Task>>();
            var tasks = new List<System.Threading.Tasks.Task>();

            // process each content reference in parallel for perf.
            foreach (var contentReference in ContentReferenceItems)
            {
                var fullPath = contentReference.GetMetadata("FullPath");
                string outputDir = GetOutputDir(contentReference);
                string intermediateOutputDir = GetIntermeditateOutputDir(contentReference);

                // trailing slash in working dir arg is not handled by MCGB.exe so we have to remove it..
                string workingDir = GetWorkingDir(contentReference).TrimEnd(PathSperators);

                var commandArgs = string.Format(@"/workingDir:""{4}"" /@:""{0}"" {1} /outputDir:""{2}"" /intermediateDir:""{3}"" /quiet", fullPath, platformArgs, outputDir, intermediateOutputDir, workingDir);

                var task = new System.Threading.Tasks.Task(() =>
                {
                    try
                    {
                        RunContentBuilderExecutable(workingDir, commandArgs, (output) =>
                        {
                            // Now gather the files as outputs.
                            var outputItems = GatherOutputs(contentReference, outputDir);
                            foreach (var item in outputItems)
                            {
                                _content.Add(item);
                            }

                        });
                    }
                    catch (Exception e)
                    {

                        throw;
                    }

                });
                tasks.Add(task);
                task.Start();
            }

            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            ExtraContent = _content.ToArray();
            return true;
        }

        private IEnumerable<ITaskItem> GatherOutputs(ITaskItem contentReference, string outputDir)
        {
            var items = new List<ITaskItem>();
            var outputDirectoryInfo = new DirectoryInfo(outputDir);
            var relativeDir = contentReference.GetMetadata("RelativeDir");
            var link = contentReference.GetMetadata("Link");

            VisitFiles(outputDir, (fileInfo) =>
            {
                var item = new TaskItem(fileInfo.FullName);
                item.SetMetadata("ContentOutputDir", outputDirectoryInfo.FullName);
                item.SetMetadata("RelativeContentOutputDir", relativeDir);

                if (string.IsNullOrWhiteSpace(fileInfo.FullName))
                {
                    return;
                }

                if (fileInfo.FullName.Length <= outputDirectoryInfo.FullName.Length)
                {
                    return;
                }

                // We add RecursiveDir metadata, as if the directory portion of the file paths was resolved using a glob include, from the outputdir root.
                // i.e include="$(outputDir)/**/*
                var recursiveDir = fileInfo.FullName.Remove(0, outputDirectoryInfo.FullName.Length).TrimStart(PathSperators);
                item.SetMetadata("RecursiveDir", recursiveDir);

                if (string.IsNullOrWhiteSpace(link))
                {
                    var linkPath = string.Format("{0}{1}{2}", PlatformResourcePrefix, relativeDir, recursiveDir);
                    item.SetMetadata("Link", linkPath);
                }
                else
                {
                    var linkRoot = Path.GetDirectoryName(link);
                    var linkPath = string.Format("{0}\\{1}", linkRoot, recursiveDir).TrimStart(PathSperators);
                    item.SetMetadata("Link", linkPath);
                }

                item.SetMetadata("CopyToOutputDirectory", "PreserveNewest");
                items.Add(item);
            });
            return items;

        }

        private string GetWorkingDir(ITaskItem contentReference)
        {
            var rootDir = contentReference.GetMetadata("RootDir");
            var dir = contentReference.GetMetadata("Directory");
            var path = Path.Combine(rootDir, dir);
            return path;
        }

        private string GetOutputDir(ITaskItem contentReference)
        {
            var relativeDir = contentReference.GetMetadata("RelativeDir");
            var fileName = contentReference.GetMetadata("Filename");
            var contentOutputDir = Path.Combine(ProjectDirectory, relativeDir, "bin", Platform, fileName);
            contentReference.SetMetadata("ContentOutputDir", contentOutputDir);
            return contentOutputDir;
        }

        private string GetIntermeditateOutputDir(ITaskItem contentReference)
        {
            var relativeDir = contentReference.GetMetadata("RelativeDir");
            var fileName = contentReference.GetMetadata("Filename");
            var contentOutputDir = Path.Combine(ProjectDirectory, relativeDir, "obj", Platform, fileName);
            contentReference.SetMetadata("ContentIntermediateOutputDir", contentOutputDir);
            return contentOutputDir;
        }

        public void RunContentBuilderExecutable(string workingDir, string commandLineArguments, Action<string> outputValidator)
        {
            if (!File.Exists(MgcbExePath))
            {
                // log error?
                throw new FileNotFoundException("Could not find MGCB.exe at: " + MgcbExePath);
            }

            var allOutput = new StringBuilder();

            Action<string> writer = (output) =>
            {
                allOutput.AppendLine(output);
                // todo: log output to msbuild log?
                //Console.WriteLine(output);
            };

            var result = SilentProcessRunner.ExecuteCommand(MgcbExePath, commandLineArguments, workingDir, writer, e => writer("ERROR: " + e));

            if (result != 0)
            {
                throw new FileNotFoundException("Could not find MGCB.exe at: " + MgcbExePath);
            }

            if (outputValidator != null)
            {
                outputValidator(allOutput.ToString());
            }
        }

        public static void VisitFiles(string root, Action<FileInfo> onVisit)
        {
            Stack<string> dirs = new Stack<string>(10);
            if (!Directory.Exists(root))
            {
                return;
            }

            dirs.Push(root);
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                var subDirs = Directory.EnumerateDirectories(currentDir); //TopDirectoryOnly
                foreach (string str in subDirs)
                {
                    dirs.Push(str);
                }
                var files = Directory.EnumerateFiles(currentDir);
                foreach (string file in files)
                {
                    // Perform whatever action is required in your scenario.
                    FileInfo fi = new FileInfo(file);
                    onVisit(fi);
                }
            }
        }
    }
}