﻿// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    public interface IProjectItem
    {
        string OriginalPath { get; set; }
        string Name { get; }
        string Location { get; }
        string DestinationPath { get; set; }
    }
}
