﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto;
using Eto.GtkSharp.Forms.Controls;

namespace MonoGame.Tools.Pipeline
{
    public static class Styles
    {
        public static void Load()
        {
            Style.Add<LabelHandler>("Wrap", h => h.Control.MaxWidthChars = 55);
        }
    }
}
