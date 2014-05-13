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
        /// <summary>
        /// Notifies IView of the current IController object.
        /// Chance to hook in to IController events, etc.
        /// </summary>        
        void Attach(IController controller);

        AskResult AskSaveOrCancel();

        bool AskSaveName(ref string filePath, string title);

        bool AskOpenProject(out string projectFilePath);

        bool AskImportProject(out string projectFilePath);

        void BeginTreeUpdate();

        void SetTreeRoot(IProjectItem item);

        void AddTreeItem(IProjectItem item);

        void RemoveTreeItem(ContentItem contentItem);

        void SelectTreeItem(IProjectItem item);
    
        void UpdateTreeItem(IProjectItem item);

        void EndTreeUpdate();

        void ShowProperties(IProjectItem item);

        void UpdateProperties(IProjectItem item);

        /// <summary>
        /// Show the user an error message within a modal dialog.
        /// </summary>        
        void ShowError(string title, string message);

        /// <summary>
        /// Show the user a message within a modal dialog.
        /// </summary>
        void ShowMessage(string message);

        /// <summary>
        /// Append passed string to output window.
        /// </summary>
        void OutputAppend(string text);

        /// <summary>
        /// Append a newline character to output window.
        /// </summary>
        void OutputAppendLine();

        /// <summary>
        /// Append passed string followed by a newline to the output window.
        /// </summary>
        void OutputAppendLine(string text);

        /// <summary>
        /// Clear output window.
        /// </summary>
        void OutputClear();               

        bool ChooseContentFile(string initialDirectory, out List<string> files);        
    }
}
