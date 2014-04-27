// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Sce.PlayStation.Core.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexDeclaration
    {
        private VertexFormat[] _vertexFormat;

        internal VertexFormat[] GetVertexFormat()
        {
            if (_vertexFormat == null)
            {
                _vertexFormat = new VertexFormat[_elements.Length];
                for (int i = 0; i < _vertexFormat.Length; i++)
                    _vertexFormat[i] = PSSHelper.ToVertexFormat(_elements[i].VertexElementFormat);
            }
            
            return _vertexFormat;
        }
    }
}