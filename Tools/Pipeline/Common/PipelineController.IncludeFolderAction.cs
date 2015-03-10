// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonoGame.Tools.Pipeline
{
    internal partial class PipelineController
    {
        private class IncludeFolderAction : IProjectAction
        {
            private readonly PipelineController _con;
            private readonly string _folder;

            public IncludeFolderAction(PipelineController controller, string folder)
            {
                _con = controller;
                _folder = folder;

                if(Path.IsPathRooted(_folder))
                {
                    string projectloc = controller._project.Location;
                    if(_folder.Length >= projectloc.Length + 1)
                        _folder = _folder.Substring(projectloc.Length + 1);
                }
            }

            public void Do()
            {
                _con.View.BeginTreeUpdate();
                _con.View.AddTreeFolder(_folder);
                _con.View.EndTreeUpdate();
            }

            public void Undo()
            {
                _con.View.BeginTreeUpdate();
                _con.View.RemoveTreeFolder(_folder);
                _con.View.EndTreeUpdate();
            }
        }
    }
}