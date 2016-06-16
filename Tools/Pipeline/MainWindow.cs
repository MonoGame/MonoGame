// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class MainWindow : Form, IView
    {
        public EventHandler<EventArgs> RecentChanged;
        public EventHandler<EventArgs> TitleChanged;
        public const string TitleBase = "MonoGame Pipeline Tool";
        public static MainWindow Instance;

        private ContextMenu _contextMenu;
        private FileDialogFilter _mgcbFileFilter, _allFileFilter, _xnaFileFilter;
        private string[] monoLocations = {
            "/usr/bin/mono",
            "/usr/local/bin/mono",
            "/Library/Frameworks/Mono.framework/Versions/Current/bin/mono"
        };

        public MainWindow()
        {
            InitializeComponent();

            Instance = this;
            Style = "MainWindow";

            _contextMenu = new ContextMenu();
            projectControl.SetContextMenu(_contextMenu);

            _mgcbFileFilter = new FileDialogFilter("MonoGame Content Build Project (*.mgcb)", new[] { ".mgcb" });
            _allFileFilter = new FileDialogFilter("All Files (*.*)", new[] { ".*" });
            _xnaFileFilter = new FileDialogFilter("XNA Content Projects (*.contentproj)", new[] { ".contentproj" });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !PipelineController.Instance.Exit();
            base.OnClosing(e);
        }

        public override void Close()
        {
            Application.Instance.Quit();
            base.Close();
        }

        public void ShowContextMenu()
        {
            if (PipelineController.Instance.ProjectOpen)
                _contextMenu.Show(projectControl);
        }

        #region IView implements

        public void Attach(IController controller)
        {
            cmdFilterOutput.Checked = PipelineSettings.Default.FilterOutput;
            CmdFilterOutput_Executed(this, EventArgs.Empty);

            cmdDebugMode.Checked = PipelineSettings.Default.DebugMode;
            CmdDebugMode_Executed(this, EventArgs.Empty);
        }

        public void Invoke(Action action)
        {
            Application.Instance.Invoke(action);
        }

        public AskResult AskSaveOrCancel()
        {
            var result = MessageBox.Show(this, "Do you want to save the project first?", "Save Project", MessageBoxButtons.YesNoCancel, MessageBoxType.Question);

            if (result == Eto.Forms.DialogResult.Yes)
                return AskResult.Yes;
            if (result == Eto.Forms.DialogResult.No)
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

            var result = dialog.ShowDialog(this) == DialogResult.Ok;
            filePath = dialog.FileName;

            if (result && dialog.CurrentFilter == _mgcbFileFilter && !filePath.EndsWith(".mgcb"))
                filePath += ".mgcb";

            return result;
        }

        public bool AskOpenProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(_mgcbFileFilter);
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _mgcbFileFilter;

            var result = dialog.ShowDialog(this) == DialogResult.Ok;
            projectFilePath = dialog.FileName;

            return result;
        }

        public bool AskImportProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(_xnaFileFilter);
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _xnaFileFilter;

            var result = dialog.ShowDialog(this) == DialogResult.Ok;
            projectFilePath = dialog.FileName;

            return result;
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
            projectControl.RefreshData();
        }

        public void UpdateProperties()
        {
            propertyGridControl.SetObjects(PipelineController.Instance.SelectedItems);
        }

        public void OutputAppend(string text)
        {
            Application.Instance.Invoke(() => buildOutput.WriteLine(text));
        }

        public void OutputClear()
        {
            Application.Instance.Invoke(() => buildOutput.ClearOutput());
        }

        public bool ShowDeleteDialog(List<IProjectItem> items)
        {
            var dialog = new DeleteDialog(PipelineController.Instance, items);
            return dialog.Run(this) == Eto.Forms.DialogResult.Ok;
        }

        public bool ShowEditDialog(string title, string text, string oldname, bool file, out string newname)
        {
            var dialog = new EditDialog(title, text, oldname, file);
            var result = dialog.Run(this) == Eto.Forms.DialogResult.Ok;

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

            folder = dialog.Directory;

            return result;
        }

        public bool ChooseItemTemplate(string folder, out ContentItemTemplate template, out string name)
        {
            var dialog = new NewItemDialog(PipelineController.Instance.Templates.GetEnumerator(), folder);
            var result = dialog.Run(this) == DialogResult.Ok;

            template = dialog.Selected;
            name = dialog.Name;

            return result;
        }

        public bool CopyOrLinkFile(string file, bool exists, out CopyAction action, out bool applyforall)
        {
            var dialog = new AddItemDialog(file, exists, FileType.File);
            var result = dialog.Run(this) == DialogResult.Ok;

            action = dialog.Responce;
            applyforall = dialog.ApplyForAll;

            return result;
        }

        public bool CopyOrLinkFolder(string folder, bool exists, out CopyAction action, out bool applyforall)
        {
            var afd = new AddItemDialog(folder, exists, FileType.Folder);
            applyforall = false;

            if (afd.Run(this) == DialogResult.Ok)
            {
                action = afd.Responce;
                return true;
            }

            action = CopyAction.Link;
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
                        monoLoc = path;
                }

                if (string.IsNullOrEmpty(monoLoc))
                {
                    monoLoc = "mono";
                    OutputAppend("Cound not find mono. Please install the latest version from http://www.mono-project.com");
                }

                proc.StartInfo.FileName = monoLoc;

                if (PipelineController.Instance.LaunchDebugger)
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
            cmdRebuildItem.Enabled = info.RebuildItem;

            // ToolBar

            if (info.Build && toolbar.Items.Contains(toolCancelBuild))
            {
                toolbar.Items.Remove(toolCancelBuild);

                toolbar.Items.Insert(12, toolBuild);
                toolbar.Items.Insert(13, toolRebuild);
                toolbar.Items.Insert(14, toolClean);
            }
            else if (info.Cancel && toolbar.Items.Contains(toolBuild))
            {
                toolbar.Items.Remove(toolBuild);
                toolbar.Items.Remove(toolRebuild);
                toolbar.Items.Remove(toolClean);

                toolbar.Items.Insert(12, toolCancelBuild);
            }

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
            AddContextMenu(cmRebuildItem, ref sep);
            AddSeparator(ref sep);
            AddContextMenu(cmExclude, ref sep);
            AddSeparator(ref sep);
            AddContextMenu(cmRename, ref sep);
            AddContextMenu(cmDelete, ref sep);

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
            this.Close();
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
            PipelineController.Instance.LaunchDebugger = cmdDebugMode.Checked;
        }

        private void CmdFilterOutput_Executed(object sender, EventArgs e)
        {
            PipelineSettings.Default.FilterOutput = cmdFilterOutput.Checked;
            buildOutput.Filtered = cmdFilterOutput.Checked;
        }

        private void CmdHelp_Executed(object sender, EventArgs e)
        {
            Process.Start("http://www.monogame.net/documentation/?page=Pipeline");
        }

        private void CmdAbout_Executed(object sender, EventArgs e)
        {
            var adialog = new AboutDialog();
            adialog.Run(this);
        }

        private void CmdOpenItem_Executed(object sender, EventArgs e)
        {
            if (PipelineController.Instance.SelectedItem is ContentItem)
                Process.Start(PipelineController.Instance.GetFullPath(PipelineController.Instance.SelectedItem.OriginalPath));
        }

        private void CmdOpenItemWith_Executed(object sender, EventArgs e)
        {
            if (PipelineController.Instance.SelectedItem != null)
                Global.ShowOpenWithDialog(PipelineController.Instance.GetFullPath(PipelineController.Instance.SelectedItem.OriginalPath));
        }

        private void CmdOpenItemLocation_Executed(object sender, EventArgs e)
        {
            if (PipelineController.Instance.SelectedItem != null)
                Process.Start(PipelineController.Instance.GetFullPath(PipelineController.Instance.SelectedItem.Location));
        }

        private void CmdRebuildItem_Executed(object sender, EventArgs e)
        {
            PipelineController.Instance.RebuildItems(PipelineController.Instance.SelectedItems.ToArray());
        }

        #endregion

    }
}

