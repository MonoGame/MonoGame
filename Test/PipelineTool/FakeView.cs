using System;
using System.Collections.Generic;
using System.Diagnostics;
using MonoGame.Tools.Pipeline;

namespace MonoGame.Tests
{
    class FakeView : IView
    {
        public string AskOpenProjectFilePath;
        public bool AskOpenProjectResult;
        public string AskSaveNameFilePath;
        public bool AskSaveNameResult;

        public FakeView()
        {
            
        }

        public void AddTreeItem(IProjectItem item)
        {
            
        }

        public bool AskImportProject(out string projectFilePath)
        {
            projectFilePath = "";

            return false;
        }

        public bool AskOpenProject(out string projectFilePath)
        {
            projectFilePath = AskOpenProjectFilePath;
            return AskOpenProjectResult;
        }

        public bool AskSaveName(ref string filePath, string title)
        {
            filePath = AskSaveNameFilePath;
            return AskSaveNameResult;
        }

        public AskResult AskSaveOrCancel()
        {
            return AskResult.Yes;
        }

        public void Attach(IController controller)
        {
            
        }

        public void BeginTreeUpdate()
        {
            
        }

        public bool ChooseContentFile(string initialDirectory, out List<string> files)
        {
            files = new List<string>();
            return false;
        }

        public bool ChooseContentFolder(string initialDirectory, out string folder)
        {
            folder = "";
            return false;
        }

        public bool ChooseItemTemplate(string folder, out ContentItemTemplate template, out string name)
        {
            template = new ContentItemTemplate();
            name = "";
            return false;
        }

        public bool CopyOrLinkFile(string file, bool exists, out CopyAction action, out bool applyforall)
        {
            action = CopyAction.Copy;
            applyforall = false;
            return false;
        }

        public bool CopyOrLinkFolder(string folder, bool exists, out CopyAction action, out bool applyforall)
        {
            action = CopyAction.Copy;
            applyforall = false;
            return false;
        }

        public Process CreateProcess(string exe, string commands)
        {
            return new Process();
        }

        public void EndTreeUpdate()
        {
            
        }

        public void Invoke(Action action)
        {
            
        }

        public void OutputAppend(string text)
        {
            
        }

        public void OutputClear()
        {
            
        }

        public void RemoveTreeItem(IProjectItem item)
        {
            
        }

        public void SetTreeRoot(IProjectItem item)
        {
            
        }

        public bool ShowDeleteDialog(List<IProjectItem> items)
        {
            return false;
        }

        public bool ShowEditDialog(string title, string text, string oldname, bool file, out string newname)
        {
            newname = "";
            return false;
        }

        public void ShowError(string title, string message)
        {
            
        }

        public void ShowMessage(string message)
        {
            
        }

        public void UpdateCommands(MenuInfo info)
        {
            
        }

        public void UpdateProperties()
        {
            
        }

        public void UpdateRecentList(List<string> recentList)
        {
            
        }

        public void UpdateTreeItem(IProjectItem item)
        {
            
        }
    }
}

