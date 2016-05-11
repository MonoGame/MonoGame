using System;
using System.IO;

namespace MonoGame.Tools.Pipeline
{
    public static class Util
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
    }
}
