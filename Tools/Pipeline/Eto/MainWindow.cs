using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
#if WINDOWS
    partial class MainView : IView
#else
    partial class MainWindow : IView
#endif
    {
        private FileDialogFilter _mgcbFileFilter, _allFileFilter, _xnaFileFilter;
        private string[] monoLocations = {
            "/usr/bin/mono",
            "/usr/local/bin/mono",
            "/Library/Frameworks/Mono.framework/Versions/Current/bin/mono"
        };

        private void Init()
        {
            _mgcbFileFilter = new FileDialogFilter("MonoGame Content Build Project (*.mgcb)", new[] { ".mgcb" });
            _allFileFilter = new FileDialogFilter("All Files (*.*)", new[] { ".*" });
            _xnaFileFilter = new FileDialogFilter("XNA Content Projects (*.contentproj)", new[] { ".contentproj" });
        }

#region IView implements

        public AskResult AskSaveOrCancel()
        {
            var result = MessageBox.Show("Do you want to save the project first?", "Save Project", MessageBoxButtons.YesNoCancel, MessageBoxType.Question);

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

            var result = dialog.ShowDialog(null) == Eto.Forms.DialogResult.Ok;
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

            var result = dialog.ShowDialog(null) == Eto.Forms.DialogResult.Ok;
            projectFilePath = dialog.FileName;

            return result;
        }

        public bool AskImportProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(_xnaFileFilter);
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _xnaFileFilter;

            var result = dialog.ShowDialog(null) == Eto.Forms.DialogResult.Ok;
            projectFilePath = dialog.FileName;

            return result;
        }

        public void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxType.Error);
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Info", MessageBoxButtons.OK, MessageBoxType.Information);
        }

        public bool ShowDeleteDialog(string[] folders, string[] files)
        {
            var dialog = new DeleteDialog(_controller, folders, files);
            return dialog.Run() == Eto.Forms.DialogResult.Ok;
        }

        public bool ShowEditDialog(string title, string text, string oldname, bool file, out string newname)
        {
            var dialog = new EditDialog(title, text, oldname, file);
            var result = dialog.Run() == Eto.Forms.DialogResult.Ok;

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

            var result = dialog.ShowDialog(null) == Eto.Forms.DialogResult.Ok;

            files = new List<string>();
            files.AddRange(dialog.Filenames);

            return result;
        }

        public bool ChooseContentFolder(string initialDirectory, out string folder)
        {
            var dialog = new SelectFolderDialog();
            dialog.Directory = initialDirectory;
            var result = dialog.ShowDialog(null) == Eto.Forms.DialogResult.Ok;

            folder = dialog.Directory;

            return result;
        }

        public bool ChooseItemTemplate(string folder, out ContentItemTemplate template, out string name)
        {
            var dialog = new NewItemDialog(_controller.Templates.GetEnumerator(), folder);
            var result = dialog.Run() == Eto.Forms.DialogResult.Ok;

            template = dialog.Selected;
            name = dialog.Name;

            return result;
        }

        public bool CopyOrLinkFile(string file, bool exists, out CopyAction action, out bool applyforall)
        {
            var dialog = new AddItemDialog(file, exists, FileType.File);
            var result = dialog.Run() == Eto.Forms.DialogResult.Ok;

            action = dialog.Responce;
            applyforall = dialog.ApplyForAll;

            return result;
        }

        public bool CopyOrLinkFolder(string folder, bool exists, out CopyAction action, out bool applyforall)
        {
            var afd = new AddItemDialog(folder, exists, FileType.Folder);
            applyforall = false;

            if (afd.Run() == Eto.Forms.DialogResult.Ok)
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

                if (_controller.LaunchDebugger)
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

#endregion
    }
}
