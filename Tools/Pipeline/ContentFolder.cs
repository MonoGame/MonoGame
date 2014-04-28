
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class FolderItem : IProjectItem
    {                
        public FolderItem(string path)
        {
            Location = path;
            Name = path;
            if (Name.Contains("/"))
                Name = Name.Split('/').Last();
        }
        
        public string Name { get; private set; }
        public string Location { get; private set; }
        public string Icon { get; set; }
    }
}
