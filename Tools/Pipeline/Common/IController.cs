﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    interface IController
    {
        /// <summary>
        /// True if there is a project.
        /// </summary>
        bool ProjectOpen { get; }

        /// <summary>
        /// True if the project has unsaved changes.
        /// </summary>
        bool ProjectDiry { get; }

        /// <summary>
        /// Notify controller that a property of Project or its contents has been modified.
        /// </summary>
        void OnProjectModified();

        /// <summary>
        /// Notify controller that Project.References has been modified.
        /// </summary>
        void OnReferencesModified();

        /// <summary>
        /// Notify controller that a property of ContentItem has been modified.
        /// </summary>        
        void OnItemModified(ContentItem contentItem);

        void NewProject();

        void ImportProject();

        void OpenProject();

        void CloseProject();

        bool SaveProject(bool saveAs);

        void OnTreeSelect(IProjectItem item);
        
        void Build(bool rebuild);

        void Clean();

        bool Exit();

        void Include(string initialDirectory);

        void Exclude(ContentItem item);

        /// <summary>
        /// If the passed path is not already rooted return the absolute path
        /// making this one relative to the currently open project location.
        /// </summary>        
        string GetFullPath(string filePath);
    }
}
