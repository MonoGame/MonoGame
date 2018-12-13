// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.ComponentModel;
using System.IO;

namespace MonoGame.Tools.Pipeline
{
    public class DirectoryItem : IProjectItem
    {
        public DirectoryItem(string name, string location) : this(location + Path.DirectorySeparatorChar + name)
        {
            
        }

        public DirectoryItem(string path)
        {
            OriginalPath = path.Trim(Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/');
            Exists = true;
        }

        #region IProjectItem

        [Browsable(false)]
        public string OriginalPath { get; set; }

        [Category("Common")]
        [Description("The file name of this item.")]
        public string Name
        {
            get
            {
                return Path.GetFileName(OriginalPath);
            }
        }

        [Category("Common")]
        [Description("The folder where this item is located.")]
        public string Location
        {
            get
            {
                return Path.GetDirectoryName(OriginalPath);
            }
        }

        [Browsable(false)]
        public bool Exists { get; set; }

        [Browsable(false)]
        public bool ExpandToThis { get; set; }

        [Browsable(false)]
        public bool SelectThis { get; set; }

        [Browsable(false)]
        public string DestinationPath
        {
            get { return OriginalPath; }
            set { OriginalPath = value; }
        }

        #endregion
    }
}

