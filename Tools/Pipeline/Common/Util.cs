using System;
using System.IO;
#if WINDOWS
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
#endif

namespace MonoGame.Tools.Pipeline
{
    internal static class Util
    {
        /// <summary>        
        /// Returns the path 'filspec' made relative path 'folder'.
        /// 
        /// If 'folder' is not an absolute path, throws ArgumentException.
        /// If 'filespec' is not an absolute path, returns 'filespec' unmodified.
        /// </summary>
        public static string GetRelativePath(string filespec, string folder)
        {
            if (!Path.IsPathRooted(filespec))
                return filespec;

            if (!Path.IsPathRooted(folder))
                throw new ArgumentException("Must be an absolute path.", "folder");

            var pathUri = new Uri(filespec);

            if (folder[folder.Length-1] != Path.DirectorySeparatorChar)
                folder += Path.DirectorySeparatorChar;

            var folderUri = new Uri(folder);
            var result = folderUri.MakeRelativeUri(pathUri).ToString();
            result = result.Replace('/', Path.DirectorySeparatorChar);
            result = Uri.UnescapeDataString(result);

            return result;
        }

        #if WINDOWS

        [DllImport("shell32.dll", EntryPoint = "ExtractAssociatedIcon", CharSet = CharSet.Auto)]
        internal static extern IntPtr ExtractAssociatedIcon(HandleRef hInst, StringBuilder lpIconPath, ref int lpiIcon);

        internal static Icon ExtractAssociatedIcon(string filePath)
        {
            try 
            {
                return Icon.ExtractAssociatedIcon(filePath);
            }
            catch (ArgumentException ae) // Network URIs throw an ArgumentException. Retry by calling shell32.
            {
                HandleRef hInst = new HandleRef(null, IntPtr.Zero);
                var iconPath = new StringBuilder(filePath);
                int iIcon = 0;
                var handle = Util.ExtractAssociatedIcon(hInst, iconPath, ref iIcon);
                if (handle != IntPtr.Zero)
                    return Icon.FromHandle(handle);
            }

            return null;
        }
        #endif
        
    }
}
