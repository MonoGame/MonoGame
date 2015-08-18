// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace MonoGame.Tools.Pipeline
{
    public interface IContentItemObserver
    {
        void OnItemModified(ContentItem item);
    }

    interface IController : IContentItemObserver
    {
        /// <summary>
        /// Types of content which can be created and added to a project. 
        /// </summary>
        IEnumerable<ContentItemTemplate> Templates { get; }

        Selection Selection { get; }

        /// <summary>
        /// True if there is a project.
        /// </summary>
        bool ProjectOpen { get; }

        /// <summary>
        /// True if the project has unsaved changes.
        /// </summary>
        bool ProjectDirty { get; }

        /// <summary>
        /// True if the project is actively building.
        /// </summary>
        bool ProjectBuilding { get; }

        /// <summary>
        /// Passes /launchdebugger option when launching MGCB.
        /// </summary>
        bool LaunchDebugger { get; set; }

        /// <summary>
        /// The view this controller is attached to.
        /// </summary>
        IView View { get; }

        /// <summary>
        /// Triggered when the project starts loading.
        /// </summary>
        event Action OnProjectLoading;

        /// <summary>
        /// Triggered when the project finishes loading.
        /// </summary>
        event Action OnProjectLoaded;

        /// <summary>
        /// Triggered when the project finishes building.
        /// </summary>
        event Action OnBuildStarted;

        /// <summary>
        /// Triggered when the project finishes building.
        /// </summary>
        event Action OnBuildFinished;

        /// <summary>
        /// Notify controller that a property of Project or its contents has been modified.
        /// </summary>
        void OnProjectModified();

        /// <summary>
        /// Notify controller that Project.References has been modified.
        /// </summary>
        void OnReferencesModified();

        void NewProject();

        void ImportProject();

        void OpenProject();

        void OpenProject(string projectFilePath);

        void CloseProject();

        bool SaveProject(bool saveAs);
        
        void Build(bool rebuild);

        void RebuildItems(IEnumerable<IProjectItem> items);

        void Clean();

        void CancelBuild();

        bool Exit();

        #region ContentItem

        void DragDrop(string initialDirectory, string[] folders, string[] files);

        void Include(string initialDirectory);

        void IncludeFolder(string initialDirectory);

        void Exclude(IEnumerable<ContentItem> items, IEnumerable<string> folders);        

        void NewItem(string name, string location, ContentItemTemplate template);

        void NewFolder(string name, string location);

        void Move (string[] paths, string[] newpaths, FileType[] types);
        
        void AddAction(IProjectAction action);

        IProjectItem GetItem(string originalPath);

        #endregion

        #region Undo, Redo

        event CanUndoRedoChanged OnCanUndoRedoChanged;

        bool CanRedo { get; }

        bool CanUndo { get; }

        void Undo();

        void Redo();

        #endregion        

        string GetFullPath(string filePath);
    }
}
