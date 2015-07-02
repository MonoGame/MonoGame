// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;

namespace MonoGame.Tools.Pipeline
{
    enum AskResult
    {
        Yes,
        No,
        Cancel
    }

    public enum CopyAction
    {
        Copy,
        Link,
        Skip
    }

    interface IView
    {
        void Attach(IController controller);

        AskResult AskSaveOrCancel();

        bool AskSaveName(ref string filePath, string title);

        bool AskOpenProject(out string projectFilePath);

        bool AskImportProject(out string projectFilePath);

        void ShowError(string title, string message);

        void ShowMessage(string message);

        void BeginTreeUpdate();

        void SetTreeRoot(IProjectItem item);

        void AddTreeItem(IProjectItem item);

        void AddTreeFolder(string folder);

        void RemoveTreeItem(ContentItem contentItem);

        void RemoveTreeFolder(string folder);

        void UpdateTreeItem(IProjectItem item);

        void EndTreeUpdate();

        void UpdateProperties(IProjectItem item);

        void OutputAppend(string text);

        void OutputClear();

        bool ChooseContentFile(string initialDirectory, out List<string> files);  

        bool ChooseContentFolder(string initialDirectory, out string folder);        

        bool CopyOrLinkFile(string file, bool exists, out CopyAction action, out bool applyforall);

        bool CopyOrLinkFolder(string folder, bool exists, out CopyAction action, out bool applyforall);
        
        void OnTemplateDefined(ContentItemTemplate item);

        Process CreateProcess(string exe, string commands);

        void ItemExistanceChanged(IProjectItem item);
    }
}
