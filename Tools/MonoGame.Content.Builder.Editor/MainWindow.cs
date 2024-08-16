// MonoGame - Copyright (C) MonoGame Foundation, Inc
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
#if IDE
    partial class MainWindow : DynamicLayout, IView
    {
        public string Title { get; set; }

        public Icon Icon { get; set; }

        public MenuBar Menu { get; set; }

        public ToolBar ToolBar { get; set; }
#else
    partial class MainWindow : Form, IView
    {
#endif
#pragma warning disable 649
        public EventHandler<EventArgs> RecentChanged;
        public EventHandler<EventArgs> TitleChanged;
#pragma warning restore 649
        public const string TitleBase = "MGCB Editor";
        public static MainWindow Instance;

        private List<Pad> _pads;
        private Clipboard _clipboard;
        private ContextMenu _contextMenu;
        private FileFilter _mgcbFileFilter, _allFileFilter, _xnaFileFilter;

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

            _contextMenu = new ContextMenu();
            projectControl.SetContextMenu(_contextMenu);

            _mgcbFileFilter = new FileFilter("MonoGame Content Build Project (*.mgcb)", new[] { ".mgcb" });
            _allFileFilter = new FileFilter("All Files (*.*)", new[] { ".*" });
            _xnaFileFilter = new FileFilter("XNA Content Projects (*.contentproj)", new[] { ".contentproj" });
        }

#if !IDE
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !PipelineController.Instance.Exit();

            base.OnClosing(e);
        }
#endif

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

            if (dialog.Show(this) == DialogResult.Ok)
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

            if (dialog.Show(this) == DialogResult.Ok)
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

            if (dialog.Show(this) == DialogResult.Ok)
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
#if !IDE
            Application.Instance.AsyncInvoke(() => buildOutput.WriteLine(text));
#endif
        }

        public void OutputClear()
        {
#if !IDE
            Application.Instance.Invoke(() => buildOutput.ClearOutput());
#endif
        }

        public bool ShowDeleteDialog(List<IProjectItem> items)
        {
            var dialog = new DeleteDialog(PipelineController.Instance, items);
            return dialog.Show(this);
        }

        public AskResult ShowReloadProjectDialog()
        {
            var result = MessageBox.Show(this, "The project file has been updated outside of the editor, do you want to reload the project? (Any unsaved changes will be lost)", "Reload Project", MessageBoxButtons.YesNo, MessageBoxType.Question);

            if (result == DialogResult.Yes)
                return AskResult.Yes;

            return AskResult.No;
        }

        public bool ShowEditDialog(string title, string text, string oldname, bool file, out string newname)
        {
            var dialog = new EditDialog(title, text, oldname, file);
            var result = dialog.Show(this);

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

            var result = dialog.Show(this) == DialogResult.Ok;

            files = new List<string>();
            files.AddRange(dialog.Filenames);

            return result;
        }

        public bool ChooseContentFolder(string initialDirectory, out string folder)
        {
            var dialog = new SelectFolderDialog();
            dialog.Directory = initialDirectory;

            var result = dialog.Show(this) == DialogResult.Ok;
            if (result)
                folder = dialog.Directory;
            else
                folder = string.Empty;

            return result;
        }

        public bool ChooseItemTemplate(string folder, out ContentItemTemplate template, out string name)
        {
            var dialog = new NewItemDialog(PipelineController.Instance.Templates.GetEnumerator(), folder);
            var result = dialog.Show(this);
            
            if (result)
            {
                template = dialog.Selected;
                name = dialog.Name + Path.GetExtension(template.TemplateFile);
            }
            else
            {
                template = null;
                name = "";
            }

            return result;
        }

        public bool CopyOrLinkFile(string file, bool exists, out IncludeType action, out bool applyforall)
        {
            var dialog = new AddItemDialog(file, exists, FileType.File);
            var result = dialog.Show(this);

            action = dialog.Responce;
            applyforall = dialog.ApplyForAll;

            return result;
        }

        public bool CopyOrLinkFolder(string folder, bool exists, out IncludeType action, out bool applyforall)
        {
            var afd = new AddItemDialog(folder, exists, FileType.Folder);
            applyforall = false;

            if (afd.Show(this))
            {
                action = afd.Responce;
                return true;
            }

            action = IncludeType.Link;
            return false;
        }

        public void UpdateCommands(MenuInfo info)
        {
#if IDE
            info.RebuildItem = false;
            info.OpenItemWith = false;
#endif

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
            AddContextMenu(cmRebuildItem, ref sep);
            AddSeparator(ref sep);
            AddContextMenu(cmOpenItemLocation, ref sep);
            AddContextMenu(cmOpenOutputItemLocation, ref sep);
            AddContextMenu(cmCopyAssetPath, ref sep);
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
            Process.Start(new ProcessStartInfo() { FileName = "https://docs.monogame.net/articles/tools/mgcb_editor.html", UseShellExecute = true, Verb = "open" });
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

            adialog.Show(this);
        }

        public void CmdOpenItem_Executed(object sender, EventArgs e)
        {
            if (PipelineController.Instance.SelectedItem is ContentItem)
            {
                var filePath = PipelineController.Instance.GetFullPath(PipelineController.Instance.SelectedItem.OriginalPath);

#if IDE
                MonoDevelop.Ide.IdeApp.Workbench.OpenDocument(filePath, MonoDevelop.Ide.Gui.OpenDocumentOptions.Default);
#else
                if (File.Exists(filePath))
                    Process.Start(new ProcessStartInfo() { FileName = filePath, UseShellExecute = true, Verb = "open" });
                else
                    ShowError("File not found", "The file was not found, did you forget to update file path in project?");
#endif
            }
        }

        private void CmdOpenItemWith_Executed(object sender, EventArgs e)
        {
            if (PipelineController.Instance.SelectedItem != null)
            {
                try
                {
                    var filepath = PipelineController.Instance.GetFullPath(PipelineController.Instance.SelectedItem.OriginalPath);
                    var dialog = new OpenWithDialog(filepath);
                    dialog.Show(this);
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
            {
                var filePath = PipelineController.Instance.GetFullPath(PipelineController.Instance.SelectedItem.Location);
                if (Directory.Exists(filePath))
                    Process.Start(new ProcessStartInfo() { FileName = filePath, UseShellExecute = true, Verb = "open" });
                else
                    ShowError("Directory Not Found", "The containing directory was not found, did you forget to update file path in project?");
            }
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
                    Process.Start(new ProcessStartInfo() { FileName = dir, UseShellExecute = true, Verb = "open" });
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

