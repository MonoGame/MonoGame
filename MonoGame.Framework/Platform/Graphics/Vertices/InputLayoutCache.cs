// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Caches DirectX input layouts for the input assembler stage.
    /// </summary>
    internal class InputLayoutCache : IDisposable
    {
#if DEBUG
        // Flag to print warning only once per shader.
        private bool _printWarning = true;
#endif
        private readonly GraphicsDevice _graphicsDevice;
        private readonly byte[] _shaderByteCode;
        private readonly Dictionary<VertexInputLayout, InputLayout> _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputLayoutCache"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="shaderByteCode">The byte code of the vertex shader.</param>
        public InputLayoutCache(GraphicsDevice graphicsDevice, byte[] shaderByteCode)
        {
            Debug.Assert(graphicsDevice != null);
            Debug.Assert(shaderByteCode != null);

            _graphicsDevice = graphicsDevice;
            _shaderByteCode = shaderByteCode;
            _cache = new Dictionary<VertexInputLayout, InputLayout>();
        }

        /// <summary>
        /// Releases all resources used by an instance of the <see cref="InputLayoutCache"/> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
        /// <see langword="true"/>, and then suppresses finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by an instance of the
        /// <see cref="InputLayoutCache"/> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
                foreach (var entry in _cache)
                    entry.Value.Dispose();

                _cache.Clear();
            }
        }

        /// <summary>
        /// Gets or create the DirectX input layout for the specified vertex buffers.
        /// </summary>
        /// <param name="vertexBuffers">The vertex buffers.</param>
        /// <returns>The DirectX input layout.</returns>
        public InputLayout GetOrCreate(VertexBufferBindings vertexBuffers)
        {
            InputLayout inputLayout;
            if (_cache.TryGetValue(vertexBuffers, out inputLayout))
                return inputLayout;

            var vertexInputLayout = vertexBuffers.ToImmutable();
            var inputElements = vertexInputLayout.GetInputElements();
            try
            {
                inputLayout = new InputLayout(_graphicsDevice._d3dDevice, _shaderByteCode, inputElements);
            }
            catch (SharpDXException ex)
            {
                if (ex.Descriptor != Result.InvalidArg)
                    throw;

                // If InputLayout ctor fails with InvalidArg then it's most likely because the 
                // vertex declaration doesn't match the vertex shader. 
                // Shader probably used the semantic "SV_Position" in the vertex shader input.
                // Background information:
                // "SV_Position" is a "system-value semantic" which is interpreted by the
                // rasterizer stage. This means it needs to be used in the vertex shader output
                // or the pixel shader input. (See
                // https://msdn.microsoft.com/en-us/library/windows/desktop/bb509647.aspx)
                //
                // However, some effects (notably the original XNA stock effects) use
                // "SV_Position" for the vertex shader input. This is technically allowed, but
                // rather uncommon and causes problems:
                // - XNA/MonoGame only has VertexElementUsage.Position, so there is no way to
                //   distinguish between "POSITION" and "SV_Position".
                // - "SV_Position" cannot be used with any index other than 0, i.e. the DirectX
                //   FX compiler does not accept "SV_Position1", "SV_Position2", ...
                //   This is a problem when using multiple vertex streams, e.g. for blend shape
                //   animations. It makes it impossible to correctly match the vertex
                //   declaration with the vertex shader signature.
                //
                // Conclusion:
                // - MonoGame needs to translate VertexElementUsage.Position to "POSITION".
                // - MonoGame effects should always use "POSITION" for vertex shader inputs.

                // Here is a workaround ("hack") for old vertex shaders which haven't been
                // updated: Rename "POSITION0" to "SV_Position" and try again.
                bool retry = false;
                for (int i = 0; i < inputElements.Length; i++)
                {
                    if (inputElements[i].SemanticIndex == 0 && inputElements[i].SemanticName.Equals("POSITION", StringComparison.OrdinalIgnoreCase))
                    {
                        inputElements[i].SemanticName = "SV_Position";
                        retry = true;
                        break;
                    }
                }

                if (!retry)
                    throw new InvalidOperationException(GetInvalidArgMessage(inputElements), ex);

                try
                {
                    inputLayout = new InputLayout(_graphicsDevice._d3dDevice, _shaderByteCode, inputElements);

                    // Workaround succeeded? This means that there is a vertex shader that needs
                    // to be updated.
#if DEBUG
                    if (_printWarning)
                    {
                        Debug.WriteLine(
                            "Warning: Vertex shader uses semantic 'SV_Position' for input register. " +
                            "Please update the shader and use the semantic 'POSITION0' instead. The " +
                            "semantic 'SV_Position' should only be used for the vertex shader output or " +
                            "pixel shader input!");
                        _printWarning = false;
                    }
#endif
                }
                catch (SharpDXException)
                {
                    // Workaround failed.
                    throw new InvalidOperationException(GetInvalidArgMessage(inputElements), ex);
                }
            }

            _cache.Add(vertexInputLayout, inputLayout);

            return inputLayout;
        }

        /// <summary>
        /// Gets a more helpful message for the SharpDX invalid arg error.
        /// </summary>
        /// <param name="inputElements">The input elements.</param>
        /// <returns>The exception message.</returns>
        private static string GetInvalidArgMessage(InputElement[] inputElements)
        {
            var elements = string.Join(", ", inputElements.Select(x => x.SemanticName + x.SemanticIndex));
            return "An error occurred while preparing to draw. "
                   + "This is probably because the current vertex declaration does not include all the elements "
                   + "required by the current vertex shader. The current vertex declaration includes these elements: "
                   + elements + ".";
        }
    }
}
