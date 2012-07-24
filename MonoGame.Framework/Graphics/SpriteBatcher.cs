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

        short[] _index;

#if DIRECTX
        VertexPositionColorTexture[] _vertexArray;
#elif OPENGL
		VertexPosition2ColorTexture[] _vertexArray;
		GCHandle _vertexHandle;
		GCHandle _indexHandle;
#endif

		public SpriteBatcher (GraphicsDevice device)
		{
            _device = device;

			_batchItemList = new List<SpriteBatchItem>(InitialBatchSize);
			_freeBatchItemQueue = new Queue<SpriteBatchItem>(InitialBatchSize);

            _index = new short[6 * InitialVertexArraySize];
            for (int i = 0; i < InitialVertexArraySize; i++)
            {
                _index[i * 6 + 0] = (short)(i * 4);
                _index[i * 6 + 1] = (short)(i * 4 + 1);
                _index[i * 6 + 2] = (short)(i * 4 + 2);
                _index[i * 6 + 3] = (short)(i * 4 + 1);
                _index[i * 6 + 4] = (short)(i * 4 + 3);
                _index[i * 6 + 5] = (short)(i * 4 + 2);
            }

#if DIRECTX
            _vertexArray = new VertexPositionColorTexture[InitialVertexArraySize * 4];
#elif OPENGL
			_vertexArray = new VertexPosition2ColorTexture[4*InitialVertexArraySize];
			_vertexHandle = GCHandle.Alloc(_vertexArray,GCHandleType.Pinned);
			_indexHandle = GCHandle.Alloc(_index,GCHandleType.Pinned);		
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

			foreach ( var item in _batchItemList )
			{
				// if the texture changed, we need to flush and bind the new texture
				bool shouldFlush = item.Texture != tex;
				if ( shouldFlush )
				{
					FlushVertexArray( startIndex, index );
					startIndex = index;
                    tex = item.Texture;
					
#if DIRECTX
                    startIndex = index = 0;
                    _device.Textures[0] = tex;	  
#elif OPENGL
					GL.ActiveTexture(TextureUnit.Texture0);
					GL.BindTexture ( TextureTarget.Texture2D, tex.glTexture );

					samplerState.Activate(TextureTarget.Texture2D);
#endif
                }

				// store the SpriteBatchItem data in our vertexArray
				_vertexArray[index++] = item.vertexTL;
				_vertexArray[index++] = item.vertexTR;
				_vertexArray[index++] = item.vertexBL;
				_vertexArray[index++] = item.vertexBR;
				
				_freeBatchItemQueue.Enqueue( item );
			}

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

            _index = new short[6 * newCount];
            for (var i = 0; i < newCount; i++)
            {
                _index[i * 6 + 0] = (short)(i * 4);
                _index[i * 6 + 1] = (short)(i * 4 + 1);
                _index[i * 6 + 2] = (short)(i * 4 + 2);
                _index[i * 6 + 3] = (short)(i * 4 + 1);
                _index[i * 6 + 4] = (short)(i * 4 + 3);
                _index[i * 6 + 5] = (short)(i * 4 + 2);
            }

#if DIRECTX
            _vertexArray = new VertexPositionColorTexture[4 * newCount];
#elif OPENGL
			_vertexHandle.Free();
			_indexHandle.Free();			
			
			_vertexArray = new VertexPosition2ColorTexture[4*newCount];
			_vertexHandle = GCHandle.Alloc(_vertexArray,GCHandleType.Pinned);
			_indexHandle = GCHandle.Alloc(_index,GCHandleType.Pinned);
#endif
		}

		void FlushVertexArray( int start, int end )
		{
            if ( start == end )
                return;

            var vertexCount = end - start;
#if DIRECTX

            _device.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList, 
                _vertexArray, 
                0,
                vertexCount, 
                _index, 
                0, 
                (vertexCount / 4) * 2, 
                VertexPositionColorTexture.VertexDeclaration);

#elif OPENGL
			GL.DrawElements( BeginMode.Triangles,
				                vertexCount/2*3,
				                DrawElementsType.UnsignedShort,
				                (IntPtr)(_indexHandle.AddrOfPinnedObject().ToInt64()+(start/2*3*sizeof(short))) );
#endif
		}
	}
}

