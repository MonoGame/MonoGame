// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    interface IController
    {
        PipelineProject Project { get; }

        void NewProject();

        void OpenProject();

        void CloseProject();

        bool SaveProject(bool saveAs);

        void OnTreeSelect(IProjectItem item);
        
        void Build(bool rebuild);

        void Clean();

        bool Exit();

        void Include(string initialDirectory);

        void Exclude(ContentItem item);

        void ProjectModified();
    }
}
