// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace MonoGame.Tools.Pipeline
{
    delegate void SelectionChanged();

    enum AskResult
    {
        Yes,
        No,
        Cancel
    }

    interface IView
    {
        //event SelectionChanged OnSelectionChanged;

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

        void RemoveTreeItem(ContentItem contentItem);

        void SelectTreeItem(IProjectItem item);
    
        void UpdateTreeItem(IProjectItem item);

        void EndTreeUpdate();

        void ShowProperties(IProjectItem item);

        void UpdateProperties(IProjectItem item);

        void OutputAppend(string text);

        void OutputClear();

        bool ChooseContentFile(string initialDirectory, out List<string> files);        
    }
}
