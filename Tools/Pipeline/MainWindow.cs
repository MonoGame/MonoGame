// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;

namespace MonoGame.Tools.Pipeline
{
    partial class MainWindow : Form, IView
    {
#pragma warning disable 649
        public EventHandler<EventArgs> RecentChanged;
        public EventHandler<EventArgs> TitleChanged;
#pragma warning restore 649
        public const string TitleBase = "MonoGame Pipeline Tool";
        public static MainWindow Instance;

        private List<Pad> _pads;
        private Clipboard _clipboard;
        private ContextMenu _contextMenu;
        private FileFilter _mgcbFileFilter, _allFileFilter, _xnaFileFilter;
        private string[] monoLocations = {
            "/usr/bin/mono",
            "/usr/local/bin/mono",
            "/Library/Frameworks/Mono.framework/Versions/Current/bin/mono",
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mono"),
        };

        int setw = 0;

        public MainWindow()
        {
            _pads = new List<Pad>();
            _clipboard = new Clipboard();

            InitializeComponent();

            Instance = this;

            // Fill in Pad menu
            foreach (var pad in _pads)
            {
                if (pad.Commands.Count > 0)
                {
                    var menu = new ButtonMenuItem();
                    menu.Text = pad.Title;

                    foreach (var com in pad.Commands)
                        menu.Items.Add(com.CreateMenuItem());

                    menuView.Items.Add(menu);
                }
            }

            #if MONOMAC
            splitterVertical.PositionChanged += delegate {
                setw++;
                if (setw > 2)
                {
                    propertyGridControl.SetWidth();
                    setw = 0;
                }
            };
            #endif

            _contextMenu = new ContextMenu();
            projectControl.SetContextMenu(_contextMenu);

            _mgcbFileFilter = new FileFilter("MonoGame Content Build Project (*.mgcb)", new[] { ".mgcb" });
            _allFileFilter = new FileFilter("All Files (*.*)", new[] { ".*" });
            _xnaFileFilter = new FileFilter("XNA Content Projects (*.contentproj)", new[] { ".contentproj" });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !PipelineController.Instance.Exit();

            base.OnClosing(e);
        }

        #region IView implements

        public void Attach(IController controller)
        {
            PipelineController.Instance.OnProjectLoaded += () => projectControl.ExpandBase();

            cmdDebugMode.Checked = PipelineSettings.Default.DebugMode;
            foreach (var control in _pads)
                control.LoadSettings();

            Style = "MainWindow";
        }

        public void Invoke(Action action)
        {
            Application.Instance.Invoke(action);
        }

        public AskResult AskSaveOrCancel()
        {
            var result = MessageBox.Show(this, "Do you want to save the project first?", "Save Project", MessageBoxButtons.YesNoCancel, MessageBoxType.Question);

            if (result == DialogResult.Yes)
                return AskResult.Yes;
            if (result == DialogResult.No)
                return AskResult.No;

            return AskResult.Cancel;
        }

        public bool AskSaveName(ref string filePath, string title)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = title;
            dialog.Filters.Add(_mgcbFileFilter);
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _mgcbFileFilter;

            if (dialog.ShowDialog(this) == DialogResult.Ok)
            {
                filePath = dialog.FileName;
                if (dialog.CurrentFilter == _mgcbFileFilter && !filePath.EndsWith(".mgcb"))
                    filePath += ".mgcb";
                
                return true;
            }

            return false;
        }

        public bool AskOpenProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(_mgcbFileFilter);
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _mgcbFileFilter;

            if (dialog.ShowDialog(this) == DialogResult.Ok)
            {
                projectFilePath = dialog.FileName;
                return true;
            }

            projectFilePath = "";
            return false;
        }

        public bool AskImportProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(_xnaFileFilter);
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _xnaFileFilter;

            if (dialog.ShowDialog(this) == DialogResult.Ok)
            {
                projectFilePath = dialog.FileName;
                return true;
            }

            projectFilePath = "";
            return false;
        }

        public void ShowError(string title, string message)
        {
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxType.Error);
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(this, message, "Info", MessageBoxButtons.OK, MessageBoxType.Information);
        }

        public void BeginTreeUpdate()
        {

        }

        public void SetTreeRoot(IProjectItem item)
        {
            projectControl.SetRoot(item);
        }

        public void AddTreeItem(IProjectItem item)
        {
            projectControl.AddItem(item);
        }

        public void RemoveTreeItem(IProjectItem item)
        {
            projectControl.RemoveItem(item);
        }

        public void UpdateTreeItem(IProjectItem item)
        {
            projectControl.UpdateItem(item);
        }

        public void EndTreeUpdate()
        {
            
        }

        public void UpdateProperties()
        {
            propertyGridControl.SetObjects(PipelineController.Instance.SelectedItems);
        }

        public void OutputAppend(string text)
        {
            Application.Instance.AsyncInvoke(() => buildOutput.WriteLine(text));
        }

        public void OutputClear()
        {
            Application.Instance.Invoke(() => buildOutput.ClearOutput());
        }

        public bool ShowDeleteDialog(List<IProjectItem> items)
        {
            var dialog = new DeleteDialog(PipelineController.Instance, items);
            return dialog.ShowModal(this);
        }

        public bool ShowEditDialog(string title, string text, string oldname, bool file, out string newname)
        {
            var dialog = new EditDialog(title, text, oldname, file);
            var result = dialog.ShowModal(this);

            newname = dialog.Text;

            return result;
        }

        public bool ChooseContentFile(string initialDirectory, out List<string> files)
        {
            var dialog = new OpenFileDialog();
            dialog.Directory = new Uri(initialDirectory);
            dialog.MultiSelect = true;
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _allFileFilter;

            var result = dialog.ShowDialog(this) == DialogResult.Ok;

            files = new List<string>();
            files.AddRange(dialog.Filenames);

            return result;
        }

        public bool ChooseContentFolder(string initialDirectory, out string folder)
        {
            var dialog = new SelectFolderDialog();
            dialog.Directory = initialDirectory;

            var result = dialog.ShowDialog(this) == DialogResult.Ok;
            if (result)
                folder = dialog.Directory;
            else
                folder = string.Empty;

            return result;
        }

        public bool ChooseItemTemplate(string folder, out ContentItemTemplate template, out string name)
        {
            var dialog = new NewItemDialog(PipelineController.Instance.Templates.GetEnumerator(), folder);
            var result = dialog.ShowModal(this);

            template = dialog.Selected;
            name = dialog.Name + Path.GetExtension(template.TemplateFile);

            return result;
        }

        public bool CopyOrLinkFile(string file, bool exists, out IncludeType action, out bool applyforall)
        {
            var dialog = new AddItemDialog(file, exists, FileType.File);
            var result = dialog.ShowModal(this);

            action = dialog.Responce;
            applyforall = dialog.ApplyForAll;

            return result;
        }

        public bool CopyOrLinkFolder(string folder, bool exists, out IncludeType action, out bool applyforall)
        {
            var afd = new AddItemDialog(folder, exists, FileType.Folder);
            applyforall = false;

            if (afd.ShowModal(this))
            {
                action = afd.Responce;
                return true;
            }

            action = IncludeType.Link;
            return false;
        }

        public Process CreateProcess(string exe, string commands)
        {
            var proc = new Process();

            if (!Global.Unix)
            {
                proc.StartInfo.FileName = exe;
                proc.StartInfo.Arguments = commands;
            }
            else
            {
                string monoLoc = null;

                foreach (var path in monoLocations)
                {
                    if (File.Exists(path))
                    {
                        monoLoc = path;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(monoLoc))
                {
                    monoLoc = "mono";
                    OutputAppend("Could not find mono. Please install the latest version from http://www.mono-project.com");
                }

                proc.StartInfo.FileName = monoLoc;

                if (PipelineSettings.Default.DebugMode)
                {
                    var port = Environment.GetEnvironmentVariable("MONO_DEBUGGER_PORT");
                    port = !string.IsNullOrEmpty(port) ? port : "55555";
                    var monodebugger = string.Format("--debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:{0}",
                        port);
                    proc.StartInfo.Arguments = string.Format("{0} \"{1}\" {2}", monodebugger, exe, commands);
                    OutputAppend("************************************************");
                    OutputAppend("RUNNING MGCB IN DEBUG MODE!!!");
                    OutputAppend(string.Format("Attach your Debugger to localhost:{0}", port));
                    OutputAppend("************************************************");
                }
                else
                {
                    proc.StartInfo.Arguments = string.Format("\"{0}\" {1}", exe, commands);
                }
            }

            return proc;
        }

        public void UpdateCommands(MenuInfo info)
        {
            // Title

            if (TitleChanged != null)
                TitleChanged(this, EventArgs.Empty);
            else
            {
                var title = TitleBase;

                if (PipelineController.Instance.ProjectOpen)
                {
                    title += " - " + Path.GetFileName(PipelineController.Instance.ProjectItem.OriginalPath);

                    if (PipelineController.Instance.ProjectDirty)
                        title += "*";
                }

                Title = title;
            }

            // Menu

            cmdNew.Enabled = info.New;
            cmdOpen.Enabled = info.Open;
            cmdImport.Enabled = info.Import;
            cmdSave.Enabled = info.Save;
            cmdSaveAs.Enabled = info.SaveAs;
            cmdClose.Enabled = info.Close;
            cmdExit.Enabled = info.Exit;

            cmdUndo.Enabled = info.Undo;
            cmdRedo.Enabled = info.Redo;
            cmdAdd.Enabled = info.Add;
            cmdNewItem.Enabled = info.Add;
            cmdNewFolder.Enabled = info.Add;
            cmdExistingItem.Enabled = info.Add;
            cmdExistingFolder.Enabled = info.Add;
            cmdExclude.Enabled = info.Exclude;
            cmdRename.Enabled = info.Rename;
            cmdDelete.Enabled = info.Delete;

            cmdBuild.Enabled = info.Build;
            cmdRebuild.Enabled = info.Rebuild;
            cmdClean.Enabled = info.Clean;
            cmdCancelBuild.Enabled = info.Cancel;

            cmdOpenItem.Enabled = info.OpenItem;
            cmdOpenItemWith.Enabled = info.OpenItemWith;
            cmdOpenItemLocation.Enabled = info.OpenItemLocation;
            cmdOpenOutputItemLocation.Enabled = info.OpenOutputItemLocation;
            cmdCopyAssetName.Enabled = info.CopyAssetPath;
            cmdRebuildItem.Enabled = info.RebuildItem;

            // Visibility of menu items can't be changed so 
            // we need to recreate the context menu each time.

            // Context Menu

            var sep = false;
            _contextMenu.Items.Clear();

            AddContextMenu(cmOpenItem, ref sep);
            AddContextMenu(cmOpenItemWith, ref sep);
            AddContextMenu(cmAdd, ref sep);
            AddSeparator(ref sep);
            AddContextMenu(cmOpenItemLocation, ref sep);
            AddContextMenu(cmOpenOutputItemLocation, ref sep);
            AddContextMenu(cmCopyAssetPath, ref sep);
            AddContextMenu(cmRebuildItem, ref sep);
            AddSeparator(ref sep);
            AddContextMenu(cmExclude, ref sep);
            AddSeparator(ref sep);
            AddContextMenu(cmRename, ref sep);
            //AddContextMenu(cmDelete, ref sep);

            if (_contextMenu.Items.Count > 0)
            {
                var lastItem = _contextMenu.Items[_contextMenu.Items.Count - 1];
                if (lastItem is SeparatorMenuItem)
                    _contextMenu.Items.Remove(lastItem);
            }
        }

        private void AddContextMenu(MenuItem item, ref bool separator)
        {
            item.Shortcut = Keys.None;

            if (item.Enabled)
                _contextMenu.Items.Add(item);

            separator |= item.Enabled;
        }

        private void AddSeparator(ref bool separator)
        {
            if (separator)
            {
                if (!(_contextMenu.Items[_contextMenu.Items.Count - 1] is SeparatorMenuItem))
                    _contextMenu.Items.Add(new SeparatorMenuItem());

                separator = false;
            }
        }

        public void UpdateRecentList(List<string> recentList)
        {
            if (RecentChanged != null)
            {
                RecentChanged(recentList, EventArgs.Empty);
                return;
            }

            menuRecent.Items.Clear();

            foreach (var recent in recentList)
            {
                var item = new ButtonMenuItem();
                item.Text = recent;
                item.Click += (sender, e) => PipelineController.Instance.OpenProject(recent);

                menuRecent.Items.Insert(0, item);
            }

            if (menuRecent.Items.Count > 0)
            {
                menuRecent.Items.Add(new SeparatorMenuItem());
                var clearItem = new ButtonMenuItem();
                clearItem.Text = "Clear";
                clearItem.Click += (sender, e) => PipelineController.Instance.ClearRecentList();
                menuRecent.Items.Add(clearItem);
            }
        }

        public void SetClipboard(string text)
        {
            _clipboard.Clear();
            _clipboard.Text = text;
        }

        #endregion

        #region Commands

        private void CmdNew_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.NewProject();
        }

        private void CmdOpen_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.OpenProject();
        }

        private void CmdClose_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.CloseProject();
        }

        private void CmdImport_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.ImportProject();
        }

        private void CmdSave_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.SaveProject(false);
        }

        private void CmdSaveAs_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.SaveProject(true);
        }

        private void CmdExit_Executed(object sender, EventArgs e)
        {
            Application.Instance.Quit();
        }

        private void CmdUndo_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.Undo();
        }

        private void CmdRedo_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.Redo();
        }

        private void CmdExclude_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.Exclude(false);
        }

        private void CmdRename_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.Rename();
        }

        private void CmdDelete_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.Exclude(true);
        }

        private void CmdNewItem_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.NewItem();
        }

        private void CmdNewFolder_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.NewFolder();
        }

        private void CmdExistingItem_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.Include();
        }

        private void CmdExistingFolder_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.IncludeFolder();
        }

        private void CmdBuild_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.Build(false);
        }

        private void CmdRebuild_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.Build(true);
        }

        private void CmdClean_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.Clean();
        }

        private void CmdCancelBuild_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.CancelBuild();
        }

        private void CmdDebugMode_Executed(object sender, EventArgs e)
        {
            PipelineSettings.Default.DebugMode = cmdDebugMode.Checked;
        }

        private void CmdHelp_Executed(object sender, EventArgs e)
        {
            Process.Start("http://www.monogame.net/documentation/?page=Pipeline");
        }

        private void CmdAbout_Executed(object sender, EventArgs e)
        {
            var adialog = new AboutDialog();
            adialog.Logo = Bitmap.FromResource("Icons.monogame.png");
            adialog.WebsiteLabel = "MonoGame Website";
            adialog.Website = new Uri("http://www.monogame.net/");

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LICENSE.txt"))
                using (var reader = new StreamReader(stream))
                    adialog.License = reader.ReadToEnd();

            adialog.ShowDialog(this);
        }

        private void CmdOpenItem_Executed(object sender, EventArgs e)
        {
            if (PipelineController.Instance.SelectedItem is ContentItem)
                Process.Start(PipelineController.Instance.GetFullPath(PipelineController.Instance.SelectedItem.OriginalPath));
        }

        private void CmdOpenItemWith_Executed(object sender, EventArgs e)
        {
            if (PipelineController.Instance.SelectedItem != null)
            {
                try
                {
                    var filepath = PipelineController.Instance.GetFullPath(PipelineController.Instance.SelectedItem.OriginalPath);
                    var dialog = new OpenWithDialog(filepath);
                    dialog.ShowDialog(this);
                }
                catch
                {
                    ShowError("Error", "An error occured while trying to launch an open with dialog.");
                }
            }
        }

        private void CmdOpenItemLocation_Executed(object sender, EventArgs e)
        {
            if (PipelineController.Instance.SelectedItem != null)
                Process.Start(PipelineController.Instance.GetFullPath(PipelineController.Instance.SelectedItem.Location));
        }

        private void CmdOpenOutputItemLocation_Executed(object sender, EventArgs e)
        {
            if (PipelineController.Instance.SelectedItem != null)
            {
                var dir = Path.Combine(
                    PipelineController.Instance.ProjectItem.Location,
                    PipelineController.Instance.ProjectOutputDir,
                    PipelineController.Instance.SelectedItem.Location
                );

                dir = dir.Replace("$(Platform)", PipelineController.Instance.ProjectItem.Platform.ToString());
                dir = dir.Replace("$(Configuration)", PipelineController.Instance.ProjectItem.Config);
                dir = dir.Replace("$(Config)", PipelineController.Instance.ProjectItem.Config);
                dir = dir.Replace("$(Profile)", PipelineController.Instance.ProjectItem.Profile.ToString());

                if (Directory.Exists(dir))
                    Process.Start(dir);
                else
                    ShowError("Directory Not Found", "The project output directory was not found, did you forget to build the project?");
            }
        }

        private void CmdCopyAssetPath_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.CopyAssetPath();
        }

        private void CmdRebuildItem_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.RebuildItems();
        }

        #endregion

    }
}

