// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif WINRT
// TODO
#elif GLES
using OpenTK.Graphics.ES20;
using VertexAttribPointerType = OpenTK.Graphics.ES20.All;
using TextureUnit = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
using DrawElementsType = OpenTK.Graphics.ES20.All;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BeginMode = OpenTK.Graphics.ES20.All;
#elif PSS
using Sce.Pss.Core.Graphics;
using PssVertexBuffer = Sce.Pss.Core.Graphics.VertexBuffer;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	internal class SpriteBatcher
	{
		private const int InitialBatchSize = 256;
		private const int InitialVertexArraySize = 256;
		List<SpriteBatchItem> _batchItemList;
		Queue<SpriteBatchItem> _freeBatchItemQueue;

        GraphicsDevice _device;

#if DIRECTX
        VertexPositionColorTexture[] _vertexArray;
        DynamicVertexBuffer _vertexBuffer;
        IndexBuffer _indexBuffer;
#elif OPENGL
		VertexPosition2ColorTexture[] _vertexArray;
		ushort[] _index;
		GCHandle _vertexHandle;
		GCHandle _indexHandle;
#elif PSS
        PssVertexBuffer _vertexBuffer;
        VertexPosition2ColorTexture[] _vertexArray;
        ushort[] _index;
#endif

		public SpriteBatcher (GraphicsDevice device)
		{
            _device = device;

			_batchItemList = new List<SpriteBatchItem>(InitialBatchSize);
			_freeBatchItemQueue = new Queue<SpriteBatchItem>(InitialBatchSize);

#if DIRECTX

            _vertexArray = new VertexPositionColorTexture[InitialVertexArraySize * 4];
            _vertexBuffer = new DynamicVertexBuffer(_device, VertexPositionColorTexture.VertexDeclaration, InitialVertexArraySize * 4, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(_device, IndexElementSize.SixteenBits, InitialVertexArraySize * 6, BufferUsage.WriteOnly);

            var index = new ushort[6 * InitialVertexArraySize];
            for (int i = 0; i < InitialVertexArraySize; i++)
            {
                index[i * 6 + 0] = (ushort)(i * 4);
                index[i * 6 + 1] = (ushort)(i * 4 + 1);
                index[i * 6 + 2] = (ushort)(i * 4 + 2);
                index[i * 6 + 3] = (ushort)(i * 4 + 1);
                index[i * 6 + 4] = (ushort)(i * 4 + 3);
                index[i * 6 + 5] = (ushort)(i * 4 + 2);
            }

            _indexBuffer.SetData(index);

#elif OPENGL
			_vertexArray = new VertexPosition2ColorTexture[4*InitialVertexArraySize];
			_index = new ushort[6*InitialVertexArraySize];
			_vertexHandle = GCHandle.Alloc(_vertexArray,GCHandleType.Pinned);
			_indexHandle = GCHandle.Alloc(_index,GCHandleType.Pinned);
			
			for ( int i = 0; i < InitialVertexArraySize; i++ )
			{
				_index[i*6+0] = (ushort)(i*4);
				_index[i*6+1] = (ushort)(i*4+1);
				_index[i*6+2] = (ushort)(i*4+2);
				_index[i*6+3] = (ushort)(i*4+1);
				_index[i*6+4] = (ushort)(i*4+3);
				_index[i*6+5] = (ushort)(i*4+2);
			}
#elif PSS
        _vertexArray = new VertexPosition2ColorTexture[4 * InitialVertexArraySize];
        _vertexBuffer = new PssVertexBuffer(4 * InitialVertexArraySize, 6 * InitialVertexArraySize, VertexFormat.Float2, VertexFormat.UByte4N, VertexFormat.Float2);
        _index = new ushort[6 * InitialVertexArraySize];

        for ( int i = 0; i < InitialVertexArraySize; i++ )
        {
            _index[i*6+0] = (ushort)(i*4);
            _index[i*6+1] = (ushort)(i*4+1);
            _index[i*6+2] = (ushort)(i*4+2);
            _index[i*6+3] = (ushort)(i*4+1);
            _index[i*6+4] = (ushort)(i*4+3);
            _index[i*6+5] = (ushort)(i*4+2);
        }
        _vertexBuffer.SetIndices(_index, 0, 0, 6 * InitialVertexArraySize);
#endif
		}
		
		public SpriteBatchItem CreateBatchItem()
		{
			SpriteBatchItem item;
			if ( _freeBatchItemQueue.Count > 0 )
				item = _freeBatchItemQueue.Dequeue();
			else
				item = new SpriteBatchItem();
			_batchItemList.Add(item);
			return item;
		}
		
		int CompareTexture ( SpriteBatchItem a, SpriteBatchItem b )
		{
            return ReferenceEquals( a.Texture, b.Texture ) ? 0 : 1;
		}
		
		int CompareDepth ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return a.Depth.CompareTo(b.Depth);
		}
		
		int CompareReverseDepth ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return b.Depth.CompareTo(a.Depth);
		}
		
		public void DrawBatch ( SpriteSortMode sortMode, SamplerState samplerState)
		{
			// nothing to do
			if ( _batchItemList.Count == 0 )
				return;
			// sort the batch items
			switch ( sortMode )
			{
			case SpriteSortMode.Texture :
				_batchItemList.Sort( CompareTexture );
				break;
			case SpriteSortMode.FrontToBack :
				_batchItemList.Sort ( CompareDepth );
				break;
			case SpriteSortMode.BackToFront :
				_batchItemList.Sort ( CompareReverseDepth );
				break;
			}
			
#if OPENGL

			//Unbind VBOs
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			
			GL.EnableVertexAttribArray(GraphicsDevice.attributePosition);
			GL.EnableVertexAttribArray(GraphicsDevice.attributeTexCoord);
			GL.EnableVertexAttribArray(GraphicsDevice.attributeColor);
			
			int size = VertexPosition2ColorTexture.GetSize();
			GL.VertexAttribPointer(GraphicsDevice.attributePosition,
			                       2,
			                       VertexAttribPointerType.Float,
			                       false,
			                       size,
			                       _vertexHandle.AddrOfPinnedObject());

			GL.VertexAttribPointer(GraphicsDevice.attributeColor,
			                       4,
			                       VertexAttribPointerType.UnsignedByte,
			                       true,
			                       size,
			                       (IntPtr)(_vertexHandle.AddrOfPinnedObject().ToInt64()
			         					+(sizeof(float)*2)));

			GL.VertexAttribPointer(GraphicsDevice.attributeTexCoord,
			                       2,
			                       VertexAttribPointerType.Float,
			                       false,
			                       size,
			                       (IntPtr)(_vertexHandle.AddrOfPinnedObject().ToInt64()
			         					+(sizeof(float)*2+sizeof(uint))));
#endif

			// setup the vertexArray array
			int startIndex = 0;
			int index = 0;
			Texture2D tex = null;

			// make sure the vertexArray has enough space
			if ( _batchItemList.Count*4 > _vertexArray.Length )
				ExpandVertexArray( _batchItemList.Count );
			
#if DIRECTX
            _device.SetVertexBuffer(_vertexBuffer);
            _device.Indices = _indexBuffer;
#elif PSS
            _device._graphics.SetVertexBuffer(0, _vertexBuffer);
#endif
            for (int i = 0; i < _batchItemList.Count; i++)
			{
                var item = _batchItemList[i];
				// if the texture changed, we need to flush and bind the new texture
#if !PSS
				bool shouldFlush = item.Texture != tex;
				if ( shouldFlush )
				{
					FlushVertexArray( startIndex, index );
					startIndex = index;
                    tex = item.Texture;
					
#if DIRECTX
                    _device.Textures[0] = tex;	  
#elif OPENGL
					GL.ActiveTexture(TextureUnit.Texture0);
					GL.BindTexture ( TextureTarget.Texture2D, tex.glTexture );

					samplerState.Activate(TextureTarget.Texture2D);
#endif
                }
#endif

				// store the SpriteBatchItem data in our vertexArray
				_vertexArray[index++] = item.vertexTL;
				_vertexArray[index++] = item.vertexTR;
				_vertexArray[index++] = item.vertexBL;
				_vertexArray[index++] = item.vertexBR;
				
				_freeBatchItemQueue.Enqueue ( item );
			}

#if PSS
            _vertexBuffer.SetVertices(_vertexArray, 0, 0, index);
            startIndex = index = 0;
            
            for (int i = 0; i < _batchItemList.Count; i++)
            {
                var item = _batchItemList[i];
                // if the texture changed, we need to flush and bind the new texture
                bool shouldFlush = item.Texture != tex;
                if ( shouldFlush )
                {
                    FlushVertexArray( startIndex, index );
                    startIndex = index;
                    tex = item.Texture;
                    
                    _device._graphics.SetTexture(0, tex._texture2D);
                }
                index += 4;
            }
#endif

			// flush the remaining vertexArray data
			FlushVertexArray(startIndex, index);
			
			_batchItemList.Clear();
		}
				
		void ExpandVertexArray( int batchSize )
		{
			// increase the size of the vertexArray
			var newCount = _vertexArray.Length / 4;
			
			while ( batchSize*4 > newCount )
				newCount += 128;
			
#if DIRECTX

            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();

            _vertexArray = new VertexPositionColorTexture[4 * newCount];
            _vertexBuffer = new DynamicVertexBuffer(_device, VertexPositionColorTexture.VertexDeclaration, newCount * 4, BufferUsage.WriteOnly);

            _indexBuffer = new IndexBuffer(_device, IndexElementSize.SixteenBits, InitialVertexArraySize * 6, BufferUsage.WriteOnly);

            var index = new ushort[6 * InitialVertexArraySize];
            for (int i = 0; i < InitialVertexArraySize; i++)
            {
                index[i * 6 + 0] = (ushort)(i * 4);
                index[i * 6 + 1] = (ushort)(i * 4 + 1);
                index[i * 6 + 2] = (ushort)(i * 4 + 2);
                index[i * 6 + 3] = (ushort)(i * 4 + 1);
                index[i * 6 + 4] = (ushort)(i * 4 + 3);
                index[i * 6 + 5] = (ushort)(i * 4 + 2);
            }

            _indexBuffer.SetData(index);

#elif OPENGL
			_vertexHandle.Free();
			_indexHandle.Free();			
			
			_vertexArray = new VertexPosition2ColorTexture[4*newCount];
			_index = new ushort[6*newCount];
			_vertexHandle = GCHandle.Alloc(_vertexArray,GCHandleType.Pinned);
			_indexHandle = GCHandle.Alloc(_index,GCHandleType.Pinned);
			
			for ( int i = 0; i < newCount; i++ )
			{
				_index[i*6+0] = (ushort)(i*4);
				_index[i*6+1] = (ushort)(i*4+1);
				_index[i*6+2] = (ushort)(i*4+2);
				_index[i*6+3] = (ushort)(i*4+1);
				_index[i*6+4] = (ushort)(i*4+3);
				_index[i*6+5] = (ushort)(i*4+2);
			}
#elif PSS
            _vertexBuffer.Dispose();
            _vertexBuffer = new PssVertexBuffer(4 * newCount, 6 * newCount, VertexFormat.Float2, VertexFormat.UByte4N, VertexFormat.Float2);
            
            _vertexArray = new VertexPosition2ColorTexture[4*newCount];
            _index = new ushort[6*newCount];
            for ( int i = 0; i < newCount; i++ )
            {
                _index[i*6+0] = (ushort)(i*4);
                _index[i*6+1] = (ushort)(i*4+1);
                _index[i*6+2] = (ushort)(i*4+2);
                _index[i*6+3] = (ushort)(i*4+1);
                _index[i*6+4] = (ushort)(i*4+3);
                _index[i*6+5] = (ushort)(i*4+2);
            }
            _vertexBuffer.SetIndices(_index, 0, 0, 6 * newCount);
#endif
		}

		void FlushVertexArray ( int start, int end )
		{
            if ( start == end )
                return;

#if WINRT
            var vertexCount = end - start;
            _vertexBuffer.SetData(start * _vertexBuffer.VertexDeclaration.VertexStride, _vertexArray, start, vertexCount, _vertexBuffer.VertexDeclaration.VertexStride);
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, start, 0, vertexCount, 0, (vertexCount / 4) * 2);
#elif OPENGL
			GL.DrawElements( BeginMode.Triangles,
				                (end-start)/2*3,
				                DrawElementsType.UnsignedShort,
				                (IntPtr)(_indexHandle.AddrOfPinnedObject().ToInt64()+(start/2*3*sizeof(short))) );
#elif PSS
#warning this should be applied somewhere else
            _device._graphics.Enable(EnableMode.Blend);
            _device._graphics.SetBlendFunc(BlendFuncMode.Add, BlendFuncFactor.SrcAlpha, BlendFuncFactor.OneMinusSrcAlpha);
            
            var vertexCount = end - start;
            _device._graphics.DrawArrays(DrawMode.Triangles, start / 2 * 3, vertexCount / 2 * 3);
#endif
		}
	}
}

