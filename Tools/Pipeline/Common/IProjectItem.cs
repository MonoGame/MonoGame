using System.ComponentModel;

namespace MonoGame.Tools.Pipeline
{
    interface IProjectItem
    {
        string Name { get; }
        string Location { get; }

        [Browsable(false)]
        string Icon { get; set; }
    }
}
