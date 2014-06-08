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
    internal class PipelineProject : IProjectItem
    {        
        public IController Controller;      
  
        public string OriginalPath { get; set; }

        public List<ContentItem> ContentItems { get; private set; }                

        public string OutputDir { get; set; }

        public string IntermediateDir { get; set; }

        public List<string> References { get; set; }

        public TargetPlatform Platform { get; set; }

        public GraphicsProfile Profile { get; set; }

        public string Config { get; set; }     

        #region IPipelineItem

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(OriginalPath))
                    return "";

                return System.IO.Path.GetFileNameWithoutExtension(OriginalPath);
            }
        }

        public string Location
        {
            get
            {
                if (string.IsNullOrEmpty(OriginalPath))
                    return "";

                var idx = OriginalPath.LastIndexOfAny(new char[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, OriginalPath.Length - 1);
                return OriginalPath.Remove(idx);
            }
        }

        [Browsable(false)]
        public string Icon { get; set; }

        #endregion

        public PipelineProject()
        {
            ContentItems = new List<ContentItem>();
            References = new List<string>();
            OutputDir = "bin";
            IntermediateDir = "obj";
        }
    }
}