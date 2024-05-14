// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class ShaderResource
    {
        internal ShaderResourceView _resourceView;
        internal UnorderedAccessView _unorderedAccessView;

        internal ShaderResourceView GetShaderResourceView()
        {
            if (_resourceView == null)
            {
                _resourceView = CreateShaderResourceView();
                _resourceView.DebugName = Name;
            }

            return _resourceView;
        }

        internal UnorderedAccessView GetUnorderedAccessView()
        {
            if (_unorderedAccessView == null)
                _unorderedAccessView = CreateUnorderedAccessView();

            return _unorderedAccessView;
        }

        internal abstract ShaderResourceView CreateShaderResourceView();
        internal abstract UnorderedAccessView CreateUnorderedAccessView();
    }
}
