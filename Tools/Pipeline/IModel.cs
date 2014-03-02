// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace MonoGame.Tools.Pipeline
{
    interface IModel
    {
        void Attach(IModelObserver observer);

        void NewProject();

        void LoadProject(string filePath);

        void CloseProject();
    }
}
