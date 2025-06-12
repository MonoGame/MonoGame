// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Wpf.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        private static void PlatformInit()
        {

        }

        private static System.Drawing.Bitmap PlatformGetFileIcon(string path)
        {
            return System.Drawing.Icon.ExtractAssociatedIcon(path).ToBitmap();
        }

        private static Bitmap ToEtoImage(System.Drawing.Bitmap bitmap)
        {
            var ret = new BitmapImage();

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                ret.BeginInit();
                ret.DecodePixelHeight = 16;
                ret.DecodePixelWidth = 16;
                ret.StreamSource = stream;
                ret.CacheOption = BitmapCacheOption.OnLoad;
                ret.EndInit();
            }

            return new Bitmap(new BitmapHandler(ret));
        }
    }
}

