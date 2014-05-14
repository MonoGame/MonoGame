// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.ComponentModel;

namespace MonoGame.Tools.Pipeline
{    
    interface IProjectItem
    {
		[Browsable(false)]
        string OriginalPath { get; }
		
        [Browsable(false)]
        string Icon { get; set; }
		
        string Name { get; }
		
        string Location { get; }
    }
}
