// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms.Design;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tools.Pipeline
{
    internal class PipelineProjectProxy : IProjectItem
    {
        private readonly PipelineProject _project;

        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string OutputDir
        {
            get { return _project.OutputDir; }
            set { _project.OutputDir = value; }
        }

        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string IntermediateDir
        {
            get { return _project.IntermediateDir; }
            set { _project.IntermediateDir = value; }
        }

        [Editor(typeof (AssemblyReferenceListEditor), typeof (UITypeEditor))]
        public List<string> References
        {
            get { return _project.References; }
            set { _project.References = value; }
        }

        public TargetPlatform Platform
        {
            get { return _project.Platform; }
            set { _project.Platform = value; }
        }

        public GraphicsProfile Profile
        {
            get { return _project.Profile; }
            set { _project.Profile = value; }
        }

        public string Config 
        {
            get { return _project.Config; }
            set { _project.Config = value; }
        }

        #region IPipelineItem

        public string Name
        {
            get
            {
                return _project.Name;                
            }
        }

        public string Location
        {
            get
            {
                return _project.Location;                
            }
        }

        [Browsable(false)]
        public string Icon
        {
            get { return _project.Icon; }
            set { _project.Icon = value; }
        }

        #endregion

        public PipelineProjectProxy(PipelineProject project)
        {
            _project = project;
        }
    }
}