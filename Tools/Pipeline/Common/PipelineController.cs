// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    internal class PipelineController : IController
    {
        private readonly IView _view;
        private PipelineProject _project;

        private Task _buildTask;
        private Process _buildProcess;

        public PipelineController(IView view)
        {
            _view = view;
            _view.Attach(this);            
            ProjectOpen = false;
        }

        public bool ProjectOpen { get; private set; }

        public bool ProjectDiry { get; set; }

        public bool ProjectBuilding 
        {
            get
            {
                return _buildTask != null && !_buildTask.IsCompleted;
            }
        }

        public event Action OnProjectLoading;

        public event Action OnProjectLoaded;

        public event Action OnProjectClosed;

        public event Action OnBuildStarted;

        public event Action OnBuildFinished;

        public void OnProjectModified()
        {            
            Debug.Assert(ProjectOpen, "OnProjectModified called with no project open?");
            ProjectDiry = true;
        }

        public void OnReferencesModified()
        {
            Debug.Assert(ProjectOpen, "OnReferencesModified called with no project open?");
            ProjectDiry = true;
            ResolveTypes();
        }

        public void OnItemModified(ContentItem contentItem)
        {
            Debug.Assert(ProjectOpen, "OnItemModified called with no project open?");
            ProjectDiry = true;
            _view.UpdateProperties(contentItem);

            _view.BeginTreeUpdate();
            _view.UpdateTreeItem(contentItem);
            _view.EndTreeUpdate();
        }

        public void NewProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

           // Ask user to choose a location on disk for the new project.
            // Note: It is impossible to have a project without a project root directory, hence it has to be saved immediately.
            var projectFilePath = Environment.CurrentDirectory;
            if (!_view.AskSaveName(ref projectFilePath, "New Project"))
                return;

            if (OnProjectLoading != null)
                OnProjectLoading();

            // Clear existing project data, initialize to a new blank project.
            _project = new PipelineProject(this);            
            _project.DefinedConfigs.Add("Debug");
            _project.DefinedConfigs.Add("Release");
            _project.Platform = TargetPlatform.Windows;
            _project.Config = "Debug";
            PipelineTypes.Load(_project);

            // Save the new project.
            _project.FilePath = projectFilePath;
            ProjectOpen = true;
            
            UpdateTree();

            if (OnProjectLoaded != null)
                OnProjectLoaded();
        }

        public void ImportProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            string projectFilePath;
            if (!_view.AskImportProject(out projectFilePath))
                return;

            if (OnProjectLoading != null)
                OnProjectLoading();

#if SHIPPING
            try
#endif
            {
                _project = new PipelineProject(this);
                _project.DefinedConfigs.Add("Debug");
                _project.DefinedConfigs.Add("Release");
                _project.Platform = TargetPlatform.Windows;
                _project.Config = "Debug";

                var parser = new PipelineProjectParser(this, _project);
                parser.ImportProject(projectFilePath);

                ResolveTypes();                
                
                ProjectOpen = true;
                ProjectDiry = true;
            }
#if SHIPPING
            catch (Exception e)
            {
                _view.ShowError("Open Project", "Failed to open project!");
                return;
            }
#endif

            UpdateTree();

            if (OnProjectLoaded != null)
                OnProjectLoaded();
        }

        public void OpenProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            string projectFilePath;
            if (!_view.AskOpenProject(out projectFilePath))
                return;

            OpenProject(projectFilePath);
        }

        public void OpenProject(string projectFilePath)
        {
            if (OnProjectLoading != null)
                OnProjectLoading();

#if SHIPPING
            try
#endif
            {
                _project = new PipelineProject(this);
                var parser = new PipelineProjectParser(this, _project);
                parser.OpenProject(projectFilePath);

                _project.Platform = TargetPlatform.Windows;
                if (_project.DefinedConfigs.Count == 0)
                {
                    _project.DefinedConfigs.Add("Debug");
                    _project.DefinedConfigs.Add("Release");
                }
                _project.Config = _project.DefinedConfigs[0];

                ResolveTypes();

                ProjectOpen = true;
                ProjectDiry = false;
            }
#if SHIPPING
            catch (Exception e)
            {
                _view.ShowError("Open Project", "Failed to open project!");
                return;
            }
#endif

            UpdateTree();

            if (OnProjectLoaded != null)
                OnProjectLoaded();
        }

        public void CloseProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            ProjectOpen = false;
            ProjectDiry = false;
            _project = null;

            UpdateTree();

            if (OnProjectClosed != null)
                OnProjectClosed();
        }

        public bool SaveProject(bool saveAs)
        {
            // Do we need file name?
            if (saveAs || string.IsNullOrEmpty(_project.FilePath))
            {
                string newFilePath = _project.FilePath;
                if (!_view.AskSaveName(ref newFilePath, null))
                    return false;

                _project.FilePath = newFilePath;
            }

            // Do the save.
            ProjectDiry = false;
            var parser = new PipelineProjectParser(this, _project);
            parser.SaveProject();            

            return true;
        }

        public void OnTreeSelect(IProjectItem item)
        {
            _view.ShowProperties(item);
        }

        public void Build(bool rebuild)
        {
            Debug.Assert(_buildTask == null || _buildTask.IsCompleted, "The previous build wasn't completed!");

            // Make sure we save first!
            if (!AskSaveProject())
                return;

            if (OnBuildStarted != null)
                OnBuildStarted();

            _view.OutputClear();
                        
            var commands = GetHeader() + string.Format(" /@:\"{0}\"", _project.FilePath);
            if (rebuild)
                commands += " /rebuild";
            _buildTask = new Task(DoBuild, commands);

            if (OnBuildFinished != null)
                _buildTask.ContinueWith((e) => OnBuildFinished());

            _buildTask.Start();
        }

        public void Clean()
        {
            Debug.Assert(_buildTask == null || _buildTask.IsCompleted, "The previous build wasn't completed!");

            // Make sure we save first!
            if (!AskSaveProject())
                return;

            if (OnBuildStarted != null)
                OnBuildStarted();

            _view.OutputClear();

            var commands = GetHeader() + " /clean";
            _buildTask = new Task(DoBuild, commands);

            if (OnBuildFinished != null)
                _buildTask.ContinueWith((e) => OnBuildFinished());       
   
            _buildTask.Start();
        }

        private void DoBuild(object commands)
        {
            _buildProcess = new Process();
            _buildProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_project.FilePath);
            _buildProcess.StartInfo.FileName = "MGCB.exe";
            _buildProcess.StartInfo.Arguments = (string)commands;
            _buildProcess.StartInfo.CreateNoWindow = true;
            _buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _buildProcess.StartInfo.UseShellExecute = false;
            _buildProcess.StartInfo.RedirectStandardError = true;
            _buildProcess.StartInfo.RedirectStandardOutput = true;
            _buildProcess.OutputDataReceived += (sender, args) => _view.OutputAppend(args.Data);
            _buildProcess.ErrorDataReceived += (sender, args) => _view.OutputAppend(args.Data);

            //string stdError = null;
            try
            {
                _buildProcess.Start();
                _buildProcess.BeginOutputReadLine();
                _buildProcess.BeginErrorReadLine();
                _buildProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                _view.OutputAppend("Build process failed!" + Environment.NewLine);
                _view.OutputAppend(ex.Message);
                _view.OutputAppend(ex.StackTrace);
            }

            // Clear the process pointer, so that cancel
            // can run after we've already finished.
            lock (_buildTask)
                _buildProcess = null;
        }

        public void CancelBuild()
        {
            if (_buildTask == null || _buildTask.IsCompleted)
                return;

            lock (_buildTask)
            {
                if (_buildProcess == null)
                    return;

                _buildProcess.Kill();
                _view.OutputAppend("Build terminated!" + Environment.NewLine);
            }
        }

        /// <summary>
        /// Prompt the user if they wish to save the project.
        /// Save it if yes is chosen.
        /// Return true if yes or no is chosen.
        /// Return false if cancel is chosen.
        /// </summary>
        private bool AskSaveProject()
        {
            // If the project is not dirty 
            // then we can simply skip it.
            if (!ProjectDiry)
                return true;

            // Ask the user if they want to save or cancel.
            var result = _view.AskSaveOrCancel();

            // Did we cancel the exit?
            if (result == AskResult.Cancel)
                return false;

            // Did we want to skip saving?
            if (result == AskResult.No)
                return true;

            return SaveProject(false);
        }

        private void UpdateTree()
        {
            _view.BeginTreeUpdate();

            if (_project == null || string.IsNullOrEmpty(_project.FilePath))
                _view.SetTreeRoot(null);
            else
            {
                _view.SetTreeRoot(_project);

                foreach (var item in _project.ContentItems)
                    _view.AddTreeItem(item);
            }

            _view.EndTreeUpdate();
        }

        public bool Exit()
        {
            // Can't exit if we're building!
            if (ProjectBuilding)
            {
                _view.ShowMessage("You cannot exit while the project is building!");
                return false;
            }

            // Make sure we give the user a chance to
            // save the project if they need too.
            return AskSaveProject();
        }

        public void Include(string initialDirectory)
        {                        
            List<string> files;
            if (!_view.ChooseContentFile(initialDirectory, out files))
                return;

            var parser = new PipelineProjectParser(this, _project);
            _view.BeginTreeUpdate();

            foreach (var file in files)
            {
                if (!parser.AddContent(file, true))
                    continue;

                var item = _project.ContentItems.Last();
                item.Controller = this;
                item.ResolveTypes();
                _view.AddTreeItem(item);
                _view.SelectTreeItem(item);
            }

            _view.EndTreeUpdate();
            ProjectDiry = true;                  
        }

        public void Exclude(ContentItem item)
        {
            _project.ContentItems.Remove(item);

            _view.BeginTreeUpdate();
            _view.RemoveTreeItem(item);
            _view.EndTreeUpdate();

            ProjectDiry = true;
        }

        public string GetFullPath(string filePath)
        {
            filePath = filePath.Replace("/", "\\");
            if (filePath.StartsWith("\\"))
                filePath = filePath.Substring(2);

            if (Path.IsPathRooted(filePath))
                return filePath;
            
            return _project.Location + "\\" + filePath;
        }

        public IEnumerable<string> DefinedConfigurations
        {
            get
            {
                if (_project != null && _project.DefinedConfigs != null)
                {
                    foreach (var i in _project.DefinedConfigs)
                        yield return i;
                }
            }
        }

        private readonly TargetPlatform[] _definedPlatforms = (TargetPlatform[])Enum.GetValues(typeof (TargetPlatform));

        public IEnumerable<TargetPlatform> DefinedPlatforms
        {
            get
            {
                if (_project != null)
                {
                    foreach (var i in _definedPlatforms)
                        yield return i;
                }
            }
        }

        public event Action<string> OnConfigChanged;

        public event Action<TargetPlatform?> OnPlatformChanged;
        
        public string CurrentConfig
        {
            get
            {
                if (_project == null)
                    return null;

                return _project.Config;
            }
            set
            {
                if (_project.Config.Equals(value))
                    return;

                _project.Config = value;
                if (OnConfigChanged != null)
                    OnConfigChanged(value);
            }
        }

        public TargetPlatform? CurrentPlatform
        {
            get
            {
                if (_project == null)                    
                    return null;

                return _project.Platform;
            }
            set
            {
                if (_project == null)
                    return;
                                
                if (_project.Platform.Equals(value))
                    return;
                
                _project.Platform = value;
                if (OnPlatformChanged != null)
                    OnPlatformChanged(value);
            }
        }

        private void ResolveTypes()
        {
            PipelineTypes.Load(_project);
            foreach (var i in _project.ContentItems)
            {
                i.Controller = this;
                i.ResolveTypes();
                _view.UpdateProperties(i);
            }        
        }

        /// <summary>
        /// Command arguments for the platform, config, intermediatedir, outputdir, and other common
        /// values needed for all build actions.
        /// </summary>        
        private string GetHeader()
        {
            var targetDir = TargetDir;
            var outDir = "bin/" + targetDir;
            var intDir = "obj/" + targetDir;
            return string.Format("/platform:{0} /config:{1} /intermediateDir:\"{2}\" /outputDir:\"{3}\"", CurrentPlatform.Value, CurrentConfig, intDir, outDir, _project.FilePath);
        }

        /// <summary>
        /// Returns the path (relative to the output/intermediate directories, respectively) which is targeted by the current platform/config.
        /// </summary>
        private string TargetDir
        {
            get
            {
                if (_project == null)
                    return null;

                return string.Format("{0}/{1}", CurrentPlatform.ToString(), CurrentConfig);
            }
        }
    }
}