using System;
using System.Runtime.InteropServices;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DXExpression
	{
        private string _indexName;
        private DXPreshader _preshader;

        public DXExpression(string indexName, byte[] expressionCode)
        {
            _indexName = indexName;
            _preshader = DXPreshader.CreatePreshader(expressionCode);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_indexName);
            _preshader.Write(writer);
        }
	}
}

