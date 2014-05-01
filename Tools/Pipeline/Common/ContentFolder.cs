// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Linq;

namespace MonoGame.Tools.Pipeline
{
    public class FolderItem : IProjectItem
    {                
        public FolderItem(string path)
        {
            Location = path;
            Name = path;
            if (Name.Contains("/"))
                Name = Name.Split('/').Last();
        }
        
        public string Name { get; private set; }
        public string Location { get; private set; }
        public string Icon { get; set; }
    }
}
