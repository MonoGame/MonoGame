// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

using Sce.PlayStation.Core.Graphics;
using PssVertexBuffer = Sce.PlayStation.Core.Graphics.VertexBuffer;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        internal GraphicsContext _graphics;
        internal List<PssVertexBuffer> _availableVertexBuffers = new List<PssVertexBuffer>();
        internal List<PssVertexBuffer> _usedVertexBuffers = new List<PssVertexBuffer>();
        internal GraphicsContext Context {
            get
            {
                return _graphics;
            }
            set
            {
                _graphics = value;
            }
        }

        private void PlatformSetup()
        {
            MaxTextureSlots = 8;
        }

        private void PlatformInitialize()
        {
            _graphics = new GraphicsContext();
        }

        public void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // TODO: We need to figure out how to detect if
            // we have a depth stencil buffer or not!
            options |= ClearOptions.DepthBuffer;
            options |= ClearOptions.Stencil;

            _graphics.SetClearColor(color.ToPssVector4());
            _graphics.Clear();
        }

        private void PlatformDispose()
        {

            if (_graphics != null)
            {
                _graphics.Dispose();
                _graphics = null;
            }
        }

        public void PlatformPresent()
        {
            _graphics.SwapBuffers();
            _availableVertexBuffers.AddRange(_usedVertexBuffers);
            _usedVertexBuffers.Clear();
        }

        private void PlatformSetViewport(ref Viewport value)
        {

            _graphics.SetViewport(
                value.X, value.Y, value.Width, value.Height
            );
        }

        private void PlatformApplyDefaultRenderTarget()
        {
            _graphics.SetFrameBuffer(_graphics.Screen);
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            var renderTarget = (RenderTarget2D)_currentRenderTargetBindings[0].RenderTarget;
            _graphics.SetFrameBuffer(renderTarget._frameBuffer);

            return renderTarget;
        }

        internal void PlatformApplyState(bool applyShaders)
        {
            if ( _scissorRectangleDirty )
	            _scissorRectangleDirty = false;

            if (_blendStateDirty)
            {
                _blendState.PlatformApplyState(this);
                _blendStateDirty = false;
            }
	        if ( _depthStencilStateDirty )
            {
                _depthStencilState.PlatformApplyState(this);
                _depthStencilStateDirty = false;
            }
	        if ( _rasterizerStateDirty )
            {
                _rasterizerState.PlatformApplyState(this);
	            _rasterizerStateDirty = false;
            }

            // If we're not applying shaders then early out now.
            if (!applyShaders)
                return;

            if (_indexBufferDirty)
                _indexBufferDirty = false;

            // Nothing was in here for PSM
            //if (_vertexBufferDirty)
            //{
            //}

            Textures.SetTextures(this);
            SamplerStates.SetSamplers(this);
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            BindVertexBuffer(true);
            PlatformApplyState(true);
            _graphics.DrawArrays(PSSHelper.ToDrawMode(primitiveType), startIndex, GetElementCountArray(primitiveType, primitiveCount));
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {
            throw new NotImplementedException("Not implemented");
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            BindVertexBuffer(false);
            _graphics.DrawArrays(PSSHelper.ToDrawMode(primitiveType), vertexStart, vertexCount);
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            throw new NotImplementedException("Not implemented");
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct, IVertexType
        {
            throw new NotImplementedException("Not implemented");
        }

        internal PssVertexBuffer GetVertexBuffer(VertexFormat[] vertexFormat, int requiredVertexLength, int requiredIndexLength)
        {
            int bestMatchIndex = -1;
            PssVertexBuffer bestMatch = null;
            
            //Search for a good one
            for (int i = _availableVertexBuffers.Count - 1; i >= 0; i--)
            {
                var buf = _availableVertexBuffers[i];

                // Check there is enough space
                if (buf.VertexCount != requiredVertexLength)
                    continue;
                if (requiredIndexLength == 0 && buf.IndexCount != 0)
                    continue;
                if (requiredIndexLength > 0 && buf.IndexCount != requiredIndexLength)
                    continue;

                // Check VertexFormat is the same
                var bufFormats = buf.Formats;
                if (vertexFormat.Length != bufFormats.Length)
                    continue;
                bool allEqual = true;
                for (int j = 0; j < bufFormats.Length; j++)
                {
                    if (vertexFormat[j] != bufFormats[j])
                    {
                        allEqual = false;
                        break;
                    }
                }
                if (!allEqual)
                    continue;

                //this one is acceptable
                
                //No current best or this one is smaller than the current best
                if (bestMatch == null || (buf.IndexCount + buf.VertexCount) < (bestMatch.IndexCount + bestMatch.VertexCount))
                {
                    bestMatch = buf;
                    bestMatchIndex = i;
                }
            }
            
            if (bestMatch != null)
            {
                return bestMatch;
            }
            else
            {
                //Create one
                bestMatch = new PssVertexBuffer(requiredVertexLength, requiredIndexLength, vertexFormat);
            }
            _usedVertexBuffers.Add(bestMatch);
            
            return bestMatch;
        }
        
        /// <summary>
        /// Set the current _graphics VertexBuffer based on _vertexBuffer and _indexBuffer, reusing an existing VertexBuffer if possible
        /// </summary>
        private void BindVertexBuffer(bool bindIndexBuffer)
        {
            int requiredVertexLength = _vertexBuffer.VertexCount;
            int requiredIndexLength = (!bindIndexBuffer || _indexBuffer == null) ? 0 : _indexBuffer.IndexCount;
            
            var vertexFormat = _vertexBuffer.VertexDeclaration.GetVertexFormat();
            
            var vertexBuffer = GetVertexBuffer(vertexFormat, requiredVertexLength, requiredIndexLength);
            
            vertexBuffer.SetVertices(_vertexBuffer._vertexArray);
            if (requiredIndexLength > 0)
                vertexBuffer.SetIndices(_indexBuffer._buffer);
            _graphics.SetVertexBuffer(0, vertexBuffer);
        }
    }
}
