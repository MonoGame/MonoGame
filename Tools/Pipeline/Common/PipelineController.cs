﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MGCB;
using PathHelper = MonoGame.Framework.Content.Pipeline.Builder.PathHelper;

namespace MonoGame.Tools.Pipeline
{
    public partial class PipelineController : IController
    {
        public static PipelineController Instance;

        private PipelineProject _project;
        private FileWatcher _watcher;

        private Task _buildTask;
        private Process _buildProcess;

        private readonly List<ContentItemTemplate> _templateItems;

        private static readonly string [] _mgcbSearchPaths = new []       
        {
            "",
#if DEBUG
            "../../../../../MGCB/bin/Windows/AnyCPU/Debug",
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../../../../MGCB/bin/Windows/AnyCPU/Debug"),
#else
            "../../../../../MGCB/bin/Windows/AnyCPU/Release",
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../../../../MGCB/bin/Windows/AnyCPU/Release"),
#endif
            "../MGCB",
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../MGCB"),
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        };

        public IEnumerable<ContentItemTemplate> Templates
        {
            get { return _templateItems; }
        }

        public bool LaunchDebugger { get; set; }

        public PipelineProject ProjectItem
        {
            get
            {
                return _project;
            }
        }

        public string ProjectLocation
        {
            get
            {
                var ret = _project.Location;

                if (!_project.Location.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    ret += Path.DirectorySeparatorChar;
                
                return ret; 
            }
        }

        public string ProjectOutputDir
        {
            get { return _project.OutputDir; }
        }

        public List<IProjectItem> SelectedItems { get; private set; }

        public IProjectItem SelectedItem { get; private set; }
        
        public bool ProjectOpen { get; private set; }

        public bool ProjectDirty { get; set; }

        public bool ProjectBuilding 
        {
            get
            {
                return _buildTask != null && !_buildTask.IsCompleted;
            }
        }

        public IView View { get; private set; }

        public event Action OnProjectLoading;

        public event Action OnProjectLoaded;

        private PipelineController(IView view)
        {
            Instance = this;
            PipelineSettings.Default.Load();

            SelectedItems = new List<IProjectItem>();
            _actionStack = new ActionStack(this);

            View = view;
            View.Attach(this);
            ProjectOpen = false;

            _watcher = new FileWatcher(this, view);

            _templateItems = new List<ContentItemTemplate>();
            LoadTemplates(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Templates"));
            UpdateMenu();

            view.UpdateRecentList(PipelineSettings.Default.ProjectHistory);
        }

        public static PipelineController Create(IView view)
        {
            return new PipelineController(view);
        }

        public void OnProjectModified()
        {            
            Debug.Assert(ProjectOpen, "OnProjectModified called with no project open?");
            ProjectDirty = true;
        }

        public void OnReferencesModified()
        {
            Debug.Assert(ProjectOpen, "OnReferencesModified called with no project open?");
            ProjectDirty = true;
            ResolveTypes();
        }

        public void OnItemModified(ContentItem contentItem)
        {
            Debug.Assert(ProjectOpen, "OnItemModified called with no project open?");
            ProjectDirty = true;

            View.BeginTreeUpdate();
            View.UpdateTreeItem(contentItem);
            View.EndTreeUpdate();
        }

        public void NewProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            // A project needs a root directory or it is impossible to resolve relative paths.
            // So we need the user to choose that location even though the project has not
            // yet actually been saved to disk.
            var projectFilePath = Environment.CurrentDirectory;
            if (!View.AskSaveName(ref projectFilePath, "New Project"))
                return;

            CloseProject();

            if (OnProjectLoading != null)
                OnProjectLoading();

            // Clear existing project data, initialize to a new blank project.
            _actionStack.Clear();
            _project = new PipelineProject();            
            PipelineTypes.Load(_project);

            // Save the new project.
            _project.OriginalPath = projectFilePath;
            ProjectOpen = true;
            ProjectDirty = true;

            UpdateTree();

            if (OnProjectLoaded != null)
                OnProjectLoaded();
            
            UpdateMenu();
        }

        public void ImportProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            string projectFilePath;
            if (!View.AskImportProject(out projectFilePath))
                return;

            CloseProject();

            if (OnProjectLoading != null)
                OnProjectLoading();

#if SHIPPING
            try
#endif
            {
                _actionStack.Clear();
                _project = new PipelineProject();
                var parser = new PipelineProjectParser(this, _project);
                parser.ImportProject(projectFilePath);

                ResolveTypes();                
                
                ProjectOpen = true;
                ProjectDirty = true;                
            }
#if SHIPPING
            catch (Exception e)
            {
                View.ShowError("Open Project", "Failed to open project!");
                return;
            }
#endif

            UpdateTree();
            View.UpdateTreeItem(_project);

            if (OnProjectLoaded != null)
                OnProjectLoaded();

            UpdateMenu();
        }

        public void OpenProject()
        {
            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            string projectFilePath;
            if (!View.AskOpenProject(out projectFilePath))
                return;
            
            OpenProject(projectFilePath);
        }

        public void OpenProject(string projectFilePath)
        {
            CloseProject();

            if (OnProjectLoading != null)
                OnProjectLoading();

            try
            {
                _actionStack.Clear();
                _project = new PipelineProject();
                
                var parser = new PipelineProjectParser(this, _project);
                var errorCallback = new MGBuildParser.ErrorCallback((msg, args) => View.OutputAppend(string.Format(Path.GetFileName(projectFilePath) + ": " + msg, args)));
                parser.OpenProject(projectFilePath, errorCallback);

                ResolveTypes();

                ProjectOpen = true;
                ProjectDirty = false;

                PipelineSettings.Default.AddProjectHistory(projectFilePath);
                PipelineSettings.Default.StartupProject = projectFilePath;
                PipelineSettings.Default.Save();
                View.UpdateRecentList(PipelineSettings.Default.ProjectHistory);
            }
            catch (Exception)
            {
                View.ShowError("Open Project", "Failed to open project!");
                return;
            }

            UpdateTree();
            View.UpdateTreeItem(_project);

            if (OnProjectLoaded != null)
                OnProjectLoaded();

            _watcher.Run();

            UpdateMenu();
        }

        public void ClearRecentList()
        {
            PipelineSettings.Default.ProjectHistory.Clear();
            PipelineSettings.Default.Save();
            View.UpdateRecentList(PipelineSettings.Default.ProjectHistory);
        }

        public void CloseProject()
        {
            if (!ProjectOpen)
                return;

            // Make sure we give the user a chance to
            // save the project if they need too.
            if (!AskSaveProject())
                return;

            _watcher.Stop();

            ProjectOpen = false;
            ProjectDirty = false;
            _project = null;
            _actionStack.Clear();
            View.OutputClear();

            PipelineSettings.Default.StartupProject = null;
            PipelineSettings.Default.Save();

            UpdateTree();
            UpdateMenu();
        }

        public bool MoveProject(string newname)
        {
            string opath = _project.OriginalPath;
            string ext = Path.GetExtension(opath);

            PipelineSettings.Default.ProjectHistory.Remove(opath);

            try
            {
                File.Delete(_project.OriginalPath);
            }
            catch {
                View.ShowError("Error", "Could not delete old project file.");
                return false;
            }

            _project.OriginalPath = Path.GetDirectoryName(opath) + Path.DirectorySeparatorChar + newname + ext;
            if (!SaveProject(false))
            {
                _project.OriginalPath = opath;
                SaveProject(false);
                View.ShowError("Error", "Could not save the new project file.");
                return false;
            }
            View.SetTreeRoot(_project);

            return true;
        }
        
        public bool SaveProject(bool saveAs)
        {
            // Do we need file name?
            if (saveAs || string.IsNullOrEmpty(_project.OriginalPath))
            {
                string newFilePath = _project.OriginalPath;
                if (!View.AskSaveName(ref newFilePath, null))
                    return false;

                _project.OriginalPath = newFilePath;
				View.SetTreeRoot(_project);
            }

            // Do the save.
            ProjectDirty = false;
            var parser = new PipelineProjectParser(this, _project);
            parser.SaveProject();

            // Note: This is where a project loaded via 'new project' or 'import project' 
            //       get recorded into PipelineSettings because up until this point they did not
            //       exist as files on disk.
            PipelineSettings.Default.AddProjectHistory(_project.OriginalPath);
            PipelineSettings.Default.StartupProject = _project.OriginalPath;
            PipelineSettings.Default.Save();
            View.UpdateRecentList(PipelineSettings.Default.ProjectHistory);
            UpdateMenu();

            return true;
        }

        public void Build(bool rebuild)
        {
            var commands = string.Format("/@:\"{0}\" {1}", _project.OriginalPath, rebuild ? "/rebuild" : string.Empty);
            if (LaunchDebugger)
                commands += " /launchdebugger";
            BuildCommand(commands);
        }

        private IEnumerable<IProjectItem> GetItems(IProjectItem dir)
        {
            foreach (var item in _project.ContentItems)
                if (item.OriginalPath.StartsWith(dir.OriginalPath + "/"))
                    yield return item;
        }

        public void RebuildItems()
        {
            var items = new List<IProjectItem>();

            // If the project itself was selected, just
            // rebuild the entire project
            if (items.Contains(_project))
            {
                Build(true);
                return;
            }

            // Convert selected DirectoryItems into ContentItems
            foreach (var item in SelectedItems)
            {
                if (item is ContentItem)
                {
                    if (!items.Contains(item))
                        items.Add(item);
                    
                    continue;
                }

                foreach (var subitem in GetItems(item))
                    if (!items.Contains(subitem))
                        items.Add(subitem);
            }

            // Create a unique file within the same folder as
            // the normal project to store this incremental build.
            var uniqueName = Guid.NewGuid().ToString();
            var tempPath = Path.Combine(Path.GetDirectoryName(_project.OriginalPath), uniqueName);

            // Write the incremental project file limiting the
            // content to just the files we want to rebuild.
            using (var io = File.CreateText(tempPath))
            {
                var parser = new PipelineProjectParser(this, _project);
                parser.SaveProject(io, (i) => !items.Contains(i));
            }

            // Run the build the command.
            var commands = string.Format("/@:\"{0}\" /rebuild /incremental", tempPath);
            if (LaunchDebugger)
                commands += " /launchdebugger";

            BuildCommand(commands);

            // Cleanup the temp file once we're done.
            _buildTask.ContinueWith((e) => File.Delete(tempPath));
        }

        private void BuildCommand(string commands)
        {
            Debug.Assert(_buildTask == null || _buildTask.IsCompleted, "The previous build wasn't completed!");

            if (ProjectDirty)
                SaveProject(false);

            View.OutputClear();

            _buildTask = Task.Factory.StartNew(() => DoBuild(commands));
            _buildTask.ContinueWith((e) => View.Invoke(UpdateMenu));

            UpdateMenu();
        }

        public void Clean()
        {
            Debug.Assert(_buildTask == null || _buildTask.IsCompleted, "The previous build wasn't completed!");

            // Make sure we save first!
            if (!AskSaveProject())
                return;

            View.OutputClear();

            var commands = string.Format("/clean /intermediateDir:\"{0}\" /outputDir:\"{1}\"", _project.IntermediateDir, _project.OutputDir);
            if (LaunchDebugger)
                commands += " /launchdebugger";

            _buildTask = Task.Factory.StartNew(() => DoBuild(commands));
            _buildTask.ContinueWith((e) => View.Invoke(UpdateMenu));

            UpdateMenu();       
        }

        private string FindMGCB()
        {            
            foreach (var root in _mgcbSearchPaths)
            {
                var mgcbPath = Path.Combine(root, "MGCB.exe");
                if (File.Exists(mgcbPath))
                    return Path.GetFullPath(mgcbPath);
            }

            throw new FileNotFoundException("MGCB.exe is not in the search path!");
        }

        private void DoBuild(string commands)
        {
            try
            {
                // Prepare the process.
                _buildProcess = View.CreateProcess(FindMGCB(), commands);
                _buildProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_project.OriginalPath);
                _buildProcess.StartInfo.CreateNoWindow = true;
                _buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _buildProcess.StartInfo.UseShellExecute = false;
                _buildProcess.StartInfo.RedirectStandardOutput = true;
                _buildProcess.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
                _buildProcess.OutputDataReceived += (sender, args) => View.OutputAppend(args.Data);

                // Fire off the process.
                _buildProcess.Start();
                _buildProcess.BeginOutputReadLine();
                _buildProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                // If we got a message assume it has everything the user needs to know.
                if (!string.IsNullOrEmpty(ex.Message))
                    View.OutputAppend("Build failed:  " + ex.Message);
                else
                {
                    // Else we need to get verbose.
                    View.OutputAppend("Build failed:" + Environment.NewLine);
                    View.OutputAppend(ex.ToString());
                }
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
                _buildProcess = null;
                View.OutputAppend("Build terminated!" + Environment.NewLine);
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
            // If the project is not dirty or open
            // then we can simply skip it.
            if (!ProjectDirty)
                return true;

            // Ask the user if they want to save or cancel.
            var result = View.AskSaveOrCancel();

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
            View.BeginTreeUpdate();

            if (_project == null || string.IsNullOrEmpty(_project.OriginalPath))
                View.SetTreeRoot(null);
            else
            {
                View.SetTreeRoot(_project);

                foreach (var item in _project.ContentItems)
                    View.AddTreeItem(item);
            }            

            View.EndTreeUpdate();
        }

        public bool Exit()
        {
            // Can't exit if we're building!
            if (ProjectBuilding)
            {
                View.ShowMessage("You cannot exit while the project is building!");
                return false;
            }

            // Make sure we give the user a chance to
            // save the project if they need too.
            var ret = AskSaveProject();

            if (ret)
            {
                _watcher.Stop();
                PipelineSettings.Default.Save();
            }

            return ret;
        }

        public void DragDrop(string initialDirectory, string[] folders, string[] files)
        {
            initialDirectory = GetFullPath(initialDirectory);
            IncludeFolder(initialDirectory, folders);
            Include(initialDirectory, files);
        }

        private string GetCurrentPath()
        {
            if (SelectedItem is DirectoryItem)
                return SelectedItem.OriginalPath;

            if (SelectedItem is ContentItem)
                return SelectedItem.Location;

            return _project.Location;
        }

        public void Include()
        {
            var path = GetFullPath(GetCurrentPath());

            List<string> files;
            if (!View.ChooseContentFile(path, out files))
                return;

            Include(path, files.ToArray());
        }

        private void Include(string initialDirectory, string[] f)
        {
            List<string> files = new List<string>();
            files.AddRange(f);

            List<string> sc = new List<string>(), dc = new List<string>();
            int def = 0;

            for (int i = 0; i < files.Count; i++)
            {
                if (!files[i].StartsWith(initialDirectory))
                {
                    string newfile = Path.Combine(initialDirectory, Path.GetFileName(files[i]));
                    int daction = def;

                    if (daction == 1)
                        if (File.Exists(newfile))
                            daction = 2;

                    if (daction == 0)
                    {
                        bool applyforall;
                        CopyAction act;

                        if (!View.CopyOrLinkFile(files[i], File.Exists(newfile), out act, out applyforall))
                            return;

                        daction = (int)act + 1;
                        if (applyforall)
                            def = daction;
                    }

                    if (daction == 1)
                    {
                        sc.Add(files[i]);
                        dc.Add(newfile);
                        files[i] = newfile;
                    }
                    else if (daction == 3)
                    {
                        files.RemoveAt(i);
                        i--;
                    }
                }
            }

            if (files.Count == 0)
                return;

            try
            {
                for (int i = 0; i < sc.Count; i++)
                    File.Copy(sc[i], dc[i]);

                var action = new IncludeAction(this, files);
                if(action.Do())
                    _actionStack.Add(action);  
            }
            catch
            {
                View.ShowError("Error While Copying Files", "An error occurred while the files were being copied, aborting.");
            }
        }

        public void IncludeFolder()
        {
            var path = GetFullPath(GetCurrentPath());

            string folder;
            if (!View.ChooseContentFolder(path, out folder))
                return;

            IncludeFolder(path, new[] { folder });
        }

        public void IncludeFolder(string initialDirectory, string[] dirs)
        {
            CopyAction caction = CopyAction.Copy;
            bool applyforall = false;

            List<string> ffiles = new List<string>();
            List<string> ddirectories = new List<string>();

            List<string> sc = new List<string>(), dc = new List<string>();

            foreach (string fol in dirs)
            {
                List<string> files = new List<string>();
                List<string> directories = new List<string>();

                string folder = fol;

                if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    folder += Path.DirectorySeparatorChar;

                files.AddRange(GetFiles(folder));
                directories.Add(folder);
                directories.AddRange(GetDirectories(folder));

                if (!folder.StartsWith(initialDirectory))
                {
                    string nd = folder.Replace(folder, Path.Combine(initialDirectory, (new DirectoryInfo(folder)).Name + Path.DirectorySeparatorChar));

                    if (!applyforall)
                    if (!View.CopyOrLinkFolder(folder, Directory.Exists(nd), out caction, out applyforall))
                        return;

                    if (caction == CopyAction.Copy)
                    {
                        for (int i = 0; i < directories.Count; i++)
                            ddirectories.Add(directories[i].Replace(folder, Path.Combine(initialDirectory, (new DirectoryInfo(folder)).Name + Path.DirectorySeparatorChar)));

                        for (int i = 0; i < files.Count; i++)
                            ffiles.Add(files[i].Replace(folder, Path.Combine(initialDirectory, (new DirectoryInfo(folder)).Name + Path.DirectorySeparatorChar)));

                        sc.Add(folder);
                        dc.Add(nd);
                    }
                    else if (caction == CopyAction.Link)
                    {
                        string pl = _project.Location;
                        if (!pl.EndsWith(Path.DirectorySeparatorChar.ToString()))
                            pl += Path.DirectorySeparatorChar;

                        Uri folderUri = new Uri(pl);

                        for (int i = 0; i < directories.Count; i++)
                        {
                            Uri pathUri = new Uri(directories[i]);
                            ddirectories.Add(Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)));
                        }

                        for (int i = 0; i < files.Count; i++)
                        {
                            Uri pathUri = new Uri(files[i]);
                            ffiles.Add(Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)));
                        }
                    }
                }
                else
                {
                    ddirectories.AddRange(directories);
                    ffiles.AddRange(files);
                }
            }

            try
            {
                for (int i = 0; i < sc.Count; i++)
                    DirectoryCopy(sc[i], dc[i]);

                var action2 = new IncludeAction(this, ffiles, ddirectories);
                if (action2.Do())
                    _actionStack.Add(action2);
            }
            catch
            {
                View.ShowError("Error While Copying Files", "An error occurred while the directories were being copied, aborting.");
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            }
        }

        public void Move (string[] paths, string[] newnames, FileType[] types)
        {
            var action = new MoveAction(this, paths, newnames, types);
            if(action.Do())
                _actionStack.Add(action);
        }
        
        private List<string> GetFiles(string folder)
        {
            List<string> ret = new List<string>();

            string[] directories = Directory.GetDirectories(folder);
            foreach (string d in directories)
                ret.AddRange(GetFiles(d));

            ret.AddRange(Directory.GetFiles(folder));

            return ret;
        }

        private List<string> GetDirectories(string folder)
        {
            List<string> ret = new List<string>();

            string[] directories = Directory.GetDirectories(folder);
            foreach (string d in directories)
            {
                ret.Add(d);
                ret.AddRange(GetDirectories(d));
            }

            return ret;
        }

        public void Exclude(bool delete)
        {
            if (delete && !View.ShowDeleteDialog(SelectedItems))
                return;

            var action = new ExcludeAction(this, SelectedItems, delete);
            if(action.Do())
                _actionStack.Add(action);

            UpdateMenu();
        }

        public void NewItem()
        {
            var path = GetFullPath(GetCurrentPath());

            string name;
            ContentItemTemplate template;

            if (!View.ChooseItemTemplate(path, out template, out name))
                return;

            var action = new NewAction(this, name, path, template);
            if(action.Do())
                _actionStack.Add(action);
        }

        public void NewFolder()
        {
            string name;
            if (!View.ShowEditDialog("New Folder", "Folder Name:", "", true, out name))
                return;

            string folder = Path.Combine(GetFullPath(GetCurrentPath()), name);

            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            catch
            {
                View.ShowError ("Error While Creating a Directory", "An error has occured while the directory: \"" + folder + "\" was beeing created, aborting...");
                return;
            }

            var action = new IncludeAction(this, null, new List<string> { folder });
            if(action.Do())
                _actionStack.Add(action);
        }

        public void Rename()
        {
            string name;
            if (SelectedItem == null || !View.ShowEditDialog("Rename Item", "New Name:", SelectedItem.Name, true, out name))
                return;

            FileType type = FileType.Base;
            var path = SelectedItem.OriginalPath;
            var newpath = Path.Combine(Path.GetDirectoryName(SelectedItem.OriginalPath), name).Replace("\\", "/");

            if (SelectedItem is ContentItem)
                type = FileType.File;
            else if (SelectedItem is DirectoryItem)
                type = FileType.Folder;
            else
            {
                path = SelectedItem.Name;
                newpath = name;
            }

            Move(new[] { path }, new[] { newpath }, new[] { type });
        }

        public void AddAction(IProjectAction action)
        {
            _actionStack.Add(action);
            if (!ProjectDirty)
            {
                ProjectDirty = true;
                UpdateMenu();
            }
        }

        public IProjectItem GetItem(string originalPath)
        {
            if (_project.OriginalPath.Equals(originalPath, StringComparison.OrdinalIgnoreCase))
                return _project;

            foreach (var i in _project.ContentItems)
            {
                if (string.Equals(i.OriginalPath, originalPath, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return null;
        }

        public void CopyAssetPath()
        {
            var item = SelectedItem as ContentItem;
            if (item != null)
            {
                var path = item.OriginalPath;
                path = path.Remove(path.Length - Path.GetExtension(path).Length);
                path = path.Replace('\\', '/');

                View.SetClipboard(path);
            }
        }

        #region Undo, Redo

        private readonly ActionStack _actionStack;

        public bool CanUndo { get { return _actionStack.CanUndo; } }

        public bool CanRedo { get { return _actionStack.CanRedo; } }

        public void Undo()
        {
            _actionStack.Undo();
        }

        public void Redo()
        {
            _actionStack.Redo();
        }

        #endregion

        private void ResolveTypes()
        {
            PipelineTypes.Load(_project);
            foreach (var i in _project.ContentItems)
            {
                i.Observer = this;
                i.ResolveTypes();
            }

            View.UpdateProperties();
            LoadTemplates(Path.Combine(_project.Location, "MGTemplates"));
        }

        private void LoadTemplates(string path)
        {
            if (!Directory.Exists(path))
                return;

            var files = Directory.GetFiles(path, "*.template", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                var lines = File.ReadAllLines(f);
                if (lines.Length != 5)
                    throw new Exception("Invalid template");

                var item = new ContentItemTemplate()
                    {
                        Label = lines[0],
                        Icon = lines[1],
                        ImporterName = lines[2],
                        ProcessorName = lines[3],
                        TemplateFile = lines[4],
                    };
                
                if (_templateItems.Any(i => i.Label == item.Label))
                    continue;

                var fpath = Path.GetDirectoryName(f);
                item.TemplateFile = Path.GetFullPath(Path.Combine(fpath, item.TemplateFile));

                _templateItems.Add(item);
            }
        }

        public string GetFullPath(string filePath)
        {
            if (_project == null)
                return filePath;

            filePath = filePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            if (filePath.StartsWith("\\"))
                filePath = filePath.Substring(1);

            if (Path.IsPathRooted(filePath))
                return filePath;

            return _project.Location + Path.DirectorySeparatorChar + filePath;
        }

        public string GetRelativePath(string path)
        {
            if (!ProjectOpen)
                return path;

            var dirUri = new Uri(ProjectLocation);
            var fileUri = new Uri(path);
            var relativeUri = dirUri.MakeRelativeUri(fileUri);

            if (relativeUri == null)
                return path;

            return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public void SelectionChanged(List<IProjectItem> items)
        {
            SelectedItems = items;

            if (items.Count < 2)
            {
                if (items.Count == 1)
                    SelectedItem = items[0];
                else
                    SelectedItem = _project;
            }
            else
                SelectedItem = null;

            UpdateContextMenu();
            View.UpdateCommands(info);
            View.UpdateProperties();
        }

        MenuInfo info;

        public void UpdateMenu()
        {
            var notBuilding = !ProjectBuilding;
            var projectOpenAndNotBuilding = ProjectOpen && notBuilding;

            info = new MenuInfo();

            info.New = notBuilding;
            info.Open = notBuilding;
            info.Import = notBuilding;
            info.Save = projectOpenAndNotBuilding;
            info.SaveAs = projectOpenAndNotBuilding;
            info.Close = projectOpenAndNotBuilding;
            info.Exit = notBuilding;

            info.Undo = _actionStack.CanUndo;
            info.Redo = _actionStack.CanRedo;

            info.Build = projectOpenAndNotBuilding;
            info.Rebuild = projectOpenAndNotBuilding;
            info.Clean = projectOpenAndNotBuilding;
            info.Cancel = ProjectBuilding;

            UpdateContextMenu();

            View.UpdateCommands(info);
        }

        private void UpdateContextMenu()
        {
            var oneselected = SelectedItems.Count == 1;
            var somethingselected = SelectedItems.Count > 0;
            var exists = true;

            foreach (var item in SelectedItems)
                exists &= item.Exists;

            info.OpenItem = exists && oneselected && SelectedItem is ContentItem;
            info.OpenItemWith = exists && oneselected && !(SelectedItem is DirectoryItem);
            info.OpenItemLocation = exists && oneselected;
            info.CopyAssetPath = exists && oneselected && SelectedItem is ContentItem;
            info.Add = (exists && oneselected && !(SelectedItem is ContentItem)) || !somethingselected && ProjectOpen;
            info.Exclude = somethingselected && !SelectedItems.Contains(_project);
            info.Rename = exists && oneselected;
            info.Delete = exists && info.Exclude;
            info.RebuildItem = exists && somethingselected && !ProjectBuilding;
        }
    }
}
