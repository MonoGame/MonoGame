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
using OpenTK.Graphics.ES11;

using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class SpriteBatcher
	{
		private const int InitialBatchSize = 256;
		private const int InitialVertexArraySize = 256;
		List<SpriteBatchItem> _batchItemList;
		Queue<SpriteBatchItem> _freeBatchItemQueue;
		VertexPosition2ColorTexture[] _vertexArray;
		ushort[] _index;
		GCHandle _vertexHandle;
		GCHandle _indexHandle;

		public SpriteBatcher ()
		{
			_batchItemList = new List<SpriteBatchItem>(InitialBatchSize);
			_freeBatchItemQueue = new Queue<SpriteBatchItem>(InitialBatchSize);

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
			return a.TextureID.CompareTo(b.TextureID);
		}
		int CompareDepth ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return a.Depth.CompareTo(b.Depth);
		}
		int CompareReverseDepth ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return b.Depth.CompareTo(a.Depth);
		}
		public void DrawBatch ( SpriteSortMode sortMode, SamplerState samplerState )
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
			
			// make sure an old draw isn't still going on.
			// cross fingers, commenting this out!!
			//GL.Flush();
			
			int size = sizeof(float)*4+sizeof(uint);
			GL.VertexPointer(2,All.Float,size,_vertexHandle.AddrOfPinnedObject() );
			GL.ColorPointer(4, All.UnsignedByte,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2)));
			GL.TexCoordPointer(2, All.Float,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2+sizeof(uint))) );

			// setup the vertexArray array
			int startIndex = 0;
			int index = 0;
			int texID = -1;

			// make sure the vertexArray has enough space
			if ( _batchItemList.Count*4 > _vertexArray.Length )
				ExpandVertexArray( _batchItemList.Count );
			
			foreach ( SpriteBatchItem item in _batchItemList )
			{
				// if the texture changed, we need to flush and bind the new texture
				if ( item.TextureID != texID )
				{
					FlushVertexArray( startIndex, index );
					startIndex = index;
					texID = item.TextureID;
					GL.BindTexture ( All.Texture2D, texID );
					
					samplerState.Activate();
				}
				// store the SpriteBatchItem data in our vertexArray
				_vertexArray[index++] = item.vertexTL;
				_vertexArray[index++] = item.vertexTR;
				_vertexArray[index++] = item.vertexBL;
				_vertexArray[index++] = item.vertexBR;
				
				_freeBatchItemQueue.Enqueue ( item );
			}
			// flush the remaining vertexArray data
			FlushVertexArray(startIndex, index);
			
			_batchItemList.Clear();
		}
		void ExpandVertexArray( int batchSize )
		{
			// increase the size of the vertexArray
			int newCount = _vertexArray.Length / 4;
			
			while ( batchSize*4 > newCount )
				newCount += 128;
			
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
		}
		void FlushVertexArray ( int start, int end )
		{
			// draw stuff
			if ( start != end )
				GL.DrawElements ( All.Triangles, (end-start)/2*3, All.UnsignedShort,(IntPtr)((uint)_indexHandle.AddrOfPinnedObject()+(uint)(start/2*3*sizeof(short))) );
		}
	}
}

