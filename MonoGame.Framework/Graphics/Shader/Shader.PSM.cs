// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

enum ShaderType //FIXME: Major Hack
{
    VertexShader,
    FragmentShader
}

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class Shader : GraphicsResource
    {
        private void PlatformConstruct(BinaryReader reader, bool isVertexShader, byte[] shaderBytecode)
        {
        }

        private void PlatformGraphicsDeviceResetting()
        {
        }
	}
}

