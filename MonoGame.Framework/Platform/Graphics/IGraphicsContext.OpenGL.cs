// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.OpenGL
{
    internal interface IGraphicsContext : IDisposable
    {
        int SwapInterval { get; set; }
        bool IsDisposed { get; }
        void MakeCurrent(IWindowInfo info);
        void SwapBuffers();
        bool IsCurrent { get; }
    }
}
