using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXExpression
	{
		private string _indexName;
        private float[] _outRegs;
        private DXPreshader _preshader;

        public DXExpression(string indexName, DXPreshader preshader)
        {
            _indexName = indexName;
            _preshader = preshader;
            _outRegs = new float[4];
        }

		public object Evaluate(EffectParameterCollection parameters)
		{
            // Execute the preshader.
			_preshader.Run(parameters, _outRegs);
	
		    // Get the output parameter.
            var param = parameters[_indexName];

            // Get the result element.
            var index = (int)_outRegs[0];
            var element = param.Elements[index];

            // Return the data object.
            return element.Data;
		}
		
	}
}

