// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.IO;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class Shader
    {
        private VertexShader _vertexShader;
        private PixelShader _pixelShader;
        private byte[] _shaderBytecode;

        public byte[] Bytecode { get; private set; }

        internal VertexShader VertexShader
        {
            get
            {
                if (_vertexShader == null)
                    CreateVertexShader();
                return _vertexShader;
            }
        }

        internal PixelShader PixelShader
        {
            get
            {
                if (_pixelShader == null)
                    CreatePixelShader();
                return _pixelShader;
            }
        }

        private void PlatformConstruct(BinaryReader reader, bool isVertexShader, byte[] shaderBytecode)
        {
            _shaderBytecode = shaderBytecode;

            // We need the bytecode later for allocating the
            // input layout from the vertex declaration.
            Bytecode = shaderBytecode;

            HashKey = MonoGame.Utilities.Hash.ComputeHash(Bytecode);

            if (isVertexShader)
                CreateVertexShader();
            else
                CreatePixelShader();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _vertexShader);
            SharpDX.Utilities.Dispose(ref _pixelShader);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _vertexShader);
                SharpDX.Utilities.Dispose(ref _pixelShader);
            }

            base.Dispose(disposing);
        }

        private void CreatePixelShader()
        {
            System.Diagnostics.Debug.Assert(Stage == ShaderStage.Pixel);
            _pixelShader = new PixelShader(GraphicsDevice._d3dDevice, _shaderBytecode);
        }

        private void CreateVertexShader()
        {
            System.Diagnostics.Debug.Assert(Stage == ShaderStage.Vertex);
            _vertexShader = new VertexShader(GraphicsDevice._d3dDevice, _shaderBytecode, null);
        }
    }
}

