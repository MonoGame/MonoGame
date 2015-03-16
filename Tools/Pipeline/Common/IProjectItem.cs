// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    interface IProjectItem
    {
        string OriginalPath { get; }
        string Name { get; }
        string Location { get; }
        string Icon { get; set; }
        bool Exists { get; set; }
    }
}
