using System;
using System.IO;

namespace MonoGame.Tools.Pipeline
{
    public static class PipelineUtil
    {
        /// <summary>        
        /// Returns the path 'filspec', assumed relative to path 'folder', as a full path.
        /// If 'filespec' is an absolute path, returns 'filespec' unmodified.        
        /// If 'folder' is not an absolute path, throws ArgumentException.     
        /// </summary>
        public static string GetFullPath(string filespec, string folder)
        {
            var path = filespec.Replace("/", "\\");
            if (path.StartsWith("\\"))
                path = filespec.Substring(2);

            if (Path.IsPathRooted(path))
                return path;

            if (!Path.IsPathRooted(folder))
                throw new ArgumentException("Must be an absolute path.", "folder");

            return folder + "\\" + path;
        }

        /// <summary>        
        /// Returns the path 'filspec' made relative to path 'folder'.
        /// If 'filespec' is an absolute path, returns 'filespec' unmodified.        
        /// 
        /// If 'folder' is not an absolute path, throws ArgumentException.        
        /// </summary>
        public static string GetRelativePath(string filespec, string folder)
        {
            if (Path.IsPathRooted(filespec))
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
