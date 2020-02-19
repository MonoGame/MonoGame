// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace MonoGame.Tools.Pipeline
{
    public partial class PipelineController
    {
        private class MoveAction : IProjectAction
        {
            private IProjectItem _item;
            private readonly PipelineController _con;
            private string _oldname, _newname;

            public MoveAction(IProjectItem item, string newname)
            {
                _con = PipelineController.Instance;
                _oldname = Path.GetFileName(item.OriginalPath);
                _newname = newname;
                _item = item;
            }

            public bool Do()
            {
                _con.View.RemoveTreeItem(_item);
                var folder = Path.GetDirectoryName(_item.DestinationPath);
                _item.DestinationPath = Path.Combine(folder, _newname).Replace('\\', '/');
                _con.View.AddTreeItem(_item);
                _con.ProjectDirty = true;

                return true;
            }

            public bool Undo()
            {
                _con.View.RemoveTreeItem(_item);
                var folder = Path.GetDirectoryName(_item.DestinationPath);
                _item.DestinationPath = Path.Combine(folder, _oldname).Replace('\\', '/');
                _con.View.AddTreeItem(_item);
                _con.ProjectDirty = true;

                return true;
            }
        }
    }
}
