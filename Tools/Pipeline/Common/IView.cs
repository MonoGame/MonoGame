// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    delegate void SelectionChanged();

    enum AskResult
    {
        Yes,
        No,
        Cancel
    }

    interface IUserOutput
    {
        void OutputClear();
        void OutputAppend(string text);        
        void ShowError(string title, string message); 
    }

    interface IView : IUserOutput
    {
        //event SelectionChanged OnSelectionChanged;

        void Attach(IController controller);

        AskResult AskSaveOrCancel();

        bool AskSaveName(ref string filePath);

        bool AskOpenProject(out string projectFilePath);

        bool AskImportProject(out string projectFilePath);     

        void SetTreeRoot(IProjectItem item);

        void AddTreeItem(IProjectItem item);

        void RemoveTreeItem(ContentItem contentItem);

        void SelectTreeItem(IProjectItem item);
    
        void UpdateTreeItem(IProjectItem item);

        void ShowProperties(IProjectItem item);

        void UpdateProperties(IProjectItem item);

        bool ChooseContentFile(string initialDirectory, out string file);        
    }
}
