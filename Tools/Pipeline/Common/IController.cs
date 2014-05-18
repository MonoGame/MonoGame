// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;

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
        /// True if the project is actively building.
        /// </summary>
        bool ProjectBuilding { get; }

        /// <summary>
        /// Triggered when the project starts loading.
        /// </summary>
        event Action OnProjectLoading;

        /// <summary>
        /// Triggered when the project finishes loading.
        /// </summary>
        event Action OnProjectLoaded;

        /// <summary>
        /// Triggered when the project is closed.
        /// </summary>
        event Action OnProjectClosed;

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

        void DebugBuild();

        void Clean();

        void CancelBuild();

        bool Exit();

        void Include(string initialDirectory);

        void Exclude(ContentItem item);

        /// <summary>
        /// Enumerate configurations which the project can target. 
        /// </summary>
        IEnumerable<string> DefinedConfigurations { get; }

        /// <summary>
        /// Enumerate platforms which the project can target.
        /// </summary>
        IEnumerable<TargetPlatform> DefinedPlatforms { get; }

        /// <summary>
        /// Event fired when IController.CurrentConfig is assigned a new value.
        /// </summary>
        event Action<string> OnConfigChanged;

        /// <summary>
        /// Event fired when IController.CurrentPlatform is assigned a new value.
        /// </summary>
        event Action<TargetPlatform?> OnPlatformChanged;

        /// <summary>
        /// The config for which build commands will be executed.
        /// </summary>
        string CurrentConfig { get; set; }

        /// <summary>
        /// The platform for which build commands will be executed.
        /// </summary>
        TargetPlatform? CurrentPlatform { get; set; }
    }
}
