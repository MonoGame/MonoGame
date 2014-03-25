// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexDeclaration
    {
        internal SharpDX.Direct3D11.InputElement[] GetInputLayout()
        {
            var inputs = new SharpDX.Direct3D11.InputElement[_elements.Length];
            for (var i = 0; i < _elements.Length; i++)
                inputs[i] = _elements[i].GetInputElement();

            return inputs;
        }
    }
}
