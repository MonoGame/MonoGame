using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public class ShaderExpression
	{
        private string _indexName;
        private Preshader _preshader;

        public ShaderExpression(string indexName, byte[] expressionCode)
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

