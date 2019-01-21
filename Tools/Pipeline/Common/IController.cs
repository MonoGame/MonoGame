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

    public interface IController : IContentItemObserver
    {
        /// <summary>
        /// Types of content which can be created and added to a project. 
        /// </summary>
        IEnumerable<ContentItemTemplate> Templates { get; }

        List<IProjectItem> SelectedItems { get; }

        IProjectItem SelectedItem { get; }

        PipelineProject ProjectItem { get; }

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

        void ClearRecentList();

        void CloseProject();

        bool SaveProject(bool saveAs);
        
        void Build(bool rebuild);

        void RebuildItems();

        void Clean();

        void CancelBuild();

        bool Exit();

        #region ContentItem

        void DragDrop(string initialDirectory, string[] folders, string[] files);

        void Include();

        void IncludeFolder();

        void Exclude(bool delete);

        void NewItem();

        void NewFolder();

        void Rename();
        
        void AddAction(IProjectAction action);

        void SelectionChanged(List<IProjectItem> items);

        IProjectItem GetItem(string originalPath);

        void CopyAssetPath();

        #endregion

        #region Undo, Redo

        bool CanRedo { get; }

        bool CanUndo { get; }

        void Undo();

        void Redo();

        #endregion

        string GetFullPath(string filePath);

        string GetRelativePath(string filePath);
    }
}
