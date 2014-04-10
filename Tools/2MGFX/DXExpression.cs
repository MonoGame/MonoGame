using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DXExpression
	{
        private string _indexName;
        private Preshader _preshader;

        public DXExpression(string indexName, byte[] expressionCode)
        {
            _indexName = indexName;
            _preshader = Preshader.CreatePreshader(expressionCode);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_indexName);
            _preshader.Write(writer);
        }
	}
}

