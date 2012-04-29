using System;
using System.IO;

namespace TwoMGFX
{
    class CompilerInclude : SharpDX.D3DCompiler.Include
    {
        public void Close(Stream stream)
        {
            stream.Close();
        }

        public Stream Open(SharpDX.D3DCompiler.IncludeType type, string fileName, Stream parentStream)
        {
            try
            {
                var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                return stream;
            }
            catch (Exception ex)
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
