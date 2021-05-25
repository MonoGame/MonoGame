// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.CommandLine.Invocation;

namespace MonoGame.Content.Builder.Editor.Launcher
{
    interface IPlatform
    {
        void Register(InvocationContext context);

        void Unregister(InvocationContext context);

        void Run(InvocationContext context, string project);
    }
}
