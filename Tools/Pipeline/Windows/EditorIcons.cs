using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    public static class EditorIcons
    {
        public static ImageList Templates { get; private set; }

        static EditorIcons()
        {
            Templates = new ImageList();

            var asm = Assembly.GetExecutingAssembly();
            Templates.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.blueprint.png")));
        }
    }
}
