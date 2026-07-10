﻿// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        private static void PlatformInit()
        {
            
        }

        private static Bitmap PlatformGetFileIcon(string path)
        {
            return null;
        }

        private static Bitmap ToEtoImage(Image image)
        {
            throw new NotImplementedException();
        }
    }
}

