// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tools.Pipeline
{
    internal class PipelineProject : IProjectItem
    {        
        public readonly IController Controller;      
  
        public string FilePath { get; set; }

        public List<ContentItem> ContentItems { get; private set; }                

        public string OutputDir { get; set; }

        public string IntermediateDir { get; set; }

        public List<string> References { get; set; }

        public List<string> DefinedConfigs { get; set; }

        public string Config { get; set; }

        public TargetPlatform? Platform { get; set; }

        public GraphicsProfile Profile { get; set; }

        #region IPipelineItem

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(FilePath))
                    return "";

                return System.IO.Path.GetFileNameWithoutExtension(FilePath);
            }
        }

        public string Location
        {
            get
            {
                if (string.IsNullOrEmpty(FilePath))
                    return "";

                var idx = FilePath.LastIndexOfAny(new char[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, FilePath.Length - 1);
                return FilePath.Remove(idx);
            }
        }

        [Browsable(false)]
        public string Icon { get; set; }

        #endregion

        public PipelineProject(IController controller)
        {
            Controller = controller;
            ContentItems = new List<ContentItem>();
            References = new List<string>();
            DefinedConfigs = new List<string>();     
        }
    }
}