using System;
using System.IO;
using System.Collections.Generic;

namespace TwoMGFX
{
    class CompilerInclude : SharpDX.D3DCompiler.Include
    {
        readonly string _rootPath;

        readonly Dictionary<Stream, string> _resolvedPaths = new Dictionary<Stream, string>();

        private readonly List<string> _dependencies;

        public CompilerInclude(string rootPath, List<string> dependencies)
        {
            _rootPath = rootPath;
            _dependencies = dependencies;
        }

        public void Close(Stream stream)
        {
            stream.Close();
            _resolvedPaths.Remove(stream);
        }

        public Stream Open(SharpDX.D3DCompiler.IncludeType type, string fileName, Stream parentStream)
        {
            try
            {
                var resolvedFile = fileName;

                if (!Path.IsPathRooted(resolvedFile))
                {
                    // Search in the directory containing the calling file
                    string parentPath;

                    if (parentStream != null && _resolvedPaths.TryGetValue(parentStream, out parentPath))
                    {
                        var subFilePath = Path.Combine(Path.GetDirectoryName(parentPath), fileName);
                        if (File.Exists(subFilePath))
                            resolvedFile = subFilePath;
                    }

                    if (!File.Exists(resolvedFile))
                    {
                        // Search in the root directory
                        resolvedFile = Path.Combine(_rootPath, fileName);
                    }

                    if (!File.Exists(resolvedFile))
                    {
                        // Use the current directory
                        resolvedFile = fileName;
                    }
                }

                var stream = new FileStream(resolvedFile, FileMode.Open, FileAccess.Read);

                var fullFileName = (new FileInfo(resolvedFile)).FullName;

                _resolvedPaths[stream] = fullFileName;
                if (!_dependencies.Contains(fullFileName))
                    _dependencies.Add(fullFileName);

                return stream;
            }
            catch
            {
                return null;
            }
        }

        public IDisposable Shadow
        {
            get;
            set;
        }

        public void Dispose()
        {
            if (Shadow != null)
            {
                Shadow.Dispose();
                Shadow = null;
            }
        }
    }
}
