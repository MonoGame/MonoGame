// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    public class ContentIcons: IDisposable
    {
        public ImageList Icons { get; private set; }

        public const int ContentItemIcon = 0;
        public const int ContentMissingIcon = 1;
        public const int FolderOpenIcon = 2;
        public const int FolderClosedIcon = 3;
        public const int ProjectIcon = 4;
        public const int MaxDefinedIconIndex = 5;
     

        public ContentIcons()
        {
            Icons = new ImageList();
            Icons.ColorDepth = ColorDepth.Depth32Bit;

            var asm = Assembly.GetExecutingAssembly();
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.blueprint.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.missing.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_open.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.folder_closed.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"MonoGame.Tools.Pipeline.Icons.settings.png")));
        }

        ~ContentIcons()
        {
            Dispose(false);
        }
        
        internal int GetIcon(bool exists, string fullPath)
        {
            if (!exists)
                return ContentMissingIcon;

            string ext = Path.GetExtension(fullPath).ToLowerInvariant();

            int idx = Icons.Images.IndexOfKey(ext);
            //cache the icon
            if (idx == -1)
            {
                Icon icon =null;
                try
                {
                    icon = Icon.ExtractAssociatedIcon(fullPath);
                    Icons.Images.Add(ext, icon);
                    idx = Icons.Images.IndexOfKey(ext);
                }
                catch (ArgumentException aex) 
                {
                    //The filePath does not indicate a valid file. 
                    //-or-
                    //The filePath indicates a Universal Naming Convention (UNC) path
                }
                catch(FileNotFoundException fnfex) 
                { 
                }
            }
            if (idx != -1)
                return idx;
            
            //return default icon
            return ContentItemIcon;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Icons.Dispose();
            }
            Icons = null;
        }
    }
}
