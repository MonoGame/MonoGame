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

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// This class handles the queueing of batch items into the GPU by creating the triangle tesselations
    /// that are used to draw the sprite textures. This class supports int.MaxValue number of sprites to be
    /// batched and will process them into short.MaxValue groups (strided by 6 for the number of vertices
    /// sent to the GPU). 
    /// </summary>
	internal class SpriteBatcher
	{
        /*
         * Note that this class is fundamental to high performance for SpriteBatch games. Please exercise
         * caution when making changes to this class.
         */

        /// <summary>
        /// Initialization size for the batch item list and queue.
        /// </summary>
        private const int InitialBatchSize = 256;
        /// <summary>
        /// The maximum number of batch items that can be processed per iteration
        /// </summary>
        private const int MaxBatchSize = short.MaxValue / 6; // 6 = 4 vertices unique and 2 shared, per quad
        /// <summary>
        /// Initialization size for the vertex array, in batch units.
        /// </summary>
		private const int InitialVertexArraySize = 256;

        /// <summary>
        /// The list of batch items to process.
        /// </summary>
	    private readonly List<SpriteBatchItem> _batchItemList;

        /// <summary>
        /// The available SpriteBatchItem queue so that we reuse these objects when we can.
        /// </summary>
        private readonly Queue<SpriteBatchItem> _freeBatchItemQueue;

        /// <summary>
        /// The target graphics device.
        /// </summary>
        private readonly GraphicsDevice _device;

        /// <summary>
        /// Vertex index array. The values in this array never change.
        /// </summary>
        private short[] _index;

        private VertexPositionColorTexture[] _vertexArray;

		public SpriteBatcher (GraphicsDevice device)
		{
            _device = device;

			_batchItemList = new List<SpriteBatchItem>(InitialBatchSize);
			_freeBatchItemQueue = new Queue<SpriteBatchItem>(InitialBatchSize);

            EnsureArrayCapacity(InitialBatchSize);
		}

        /// <summary>
        /// Create an instance of SpriteBatchItem if there is none available in the free item queue. Otherwise,
        /// a previously allocated SpriteBatchItem is reused.
        /// </summary>
        /// <returns></returns>
        public SpriteBatchItem CreateBatchItem()
        {
            SpriteBatchItem item;
            if (_freeBatchItemQueue.Count > 0)
                item = _freeBatchItemQueue.Dequeue();
            else
                item = new SpriteBatchItem();
            _batchItemList.Add(item);
            return item;
        }

        /// <summary>
        /// Resize and recreate the missing indices for the index and vertex position color buffers.
        /// </summary>
        /// <param name="numBatchItems"></param>
        private void EnsureArrayCapacity(int numBatchItems)
        {
            int neededCapacity = 6 * numBatchItems;
            if (_index != null && neededCapacity <= _index.Length)
            {
                // Short circuit out of here because we have enough capacity.
                return;
            }
            short[] newIndex = new short[6 * numBatchItems];
            int start = 0;
            if (_index != null)
            {
                _index.CopyTo(newIndex, 0);
                start = _index.Length / 6;
            }
            for (var i = start; i < numBatchItems; i++)
            {
                /*
                 *  TL    TR
                 *   0----1 0,1,2,3 = index offsets for vertex indices
                 *   |   /| TL,TR,BL,BR are vertex references in SpriteBatchItem.
                 *   |  / |
                 *   | /  |
                 *   |/   |
                 *   2----3
                 *  BL    BR
                 */
                // Triangle 1
                newIndex[i * 6 + 0] = (short)(i * 4);
                newIndex[i * 6 + 1] = (short)(i * 4 + 1);
                newIndex[i * 6 + 2] = (short)(i * 4 + 2);
                // Triangle 2
                newIndex[i * 6 + 3] = (short)(i * 4 + 1);
                newIndex[i * 6 + 4] = (short)(i * 4 + 3);
                newIndex[i * 6 + 5] = (short)(i * 4 + 2);
            }
            _index = newIndex;

            _vertexArray = new VertexPositionColorTexture[4 * numBatchItems];
        }

        /// <summary>
        /// Reference comparison of the underlying Texture objects for each given SpriteBatchitem.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>0 if they are not reference equal, and 1 if so.</returns>
	    static int CompareTexture ( SpriteBatchItem a, SpriteBatchItem b )
		{
            return ReferenceEquals( a.Texture, b.Texture ) ? 0 : 1;
		}

        /// <summary>
        /// Compares the Depth of a against b returning -1 if a is less than b, 
        /// 0 if equal, and 1 if a is greater than b. The test uses float.CompareTo(float)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>-1 if a is less than b, 0 if equal, and 1 if a is greater than b</returns>
	    static int CompareDepth ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return a.Depth.CompareTo(b.Depth);
		}

        /// <summary>
        /// Implements the opposite of CompareDepth, where b is compared against a.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>-1 if b is less than a, 0 if equal, and 1 if b is greater than a</returns>
        static int CompareReverseDepth(SpriteBatchItem a, SpriteBatchItem b)
		{
			return b.Depth.CompareTo(a.Depth);
		}
		
        /// <summary>
        /// Sorts the batch items and then groups batch drawing into maximal allowed batch sets that do not
        /// overflow the 16 bit array indices for vertices.
        /// </summary>
        /// <param name="sortMode">The type of depth sorting desired for the rendering.</param>
        /// <param name="effect">The custom effect to apply to the drawn geometry</param>
        public void DrawBatch(SpriteSortMode sortMode, Effect effect)
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

            // Determine how many iterations through the drawing code we need to make
            int batchIndex = 0;
            int batchCount = _batchItemList.Count;
            // Iterate through the batches, doing short.MaxValue sets of vertices only.
            while(batchCount > 0)
            {
                // setup the vertexArray array
                var startIndex = 0;
                var index = 0;
                Texture2D tex = null;

                int numBatchesToProcess = batchCount;
                if (numBatchesToProcess > MaxBatchSize)
                {
                    numBatchesToProcess = MaxBatchSize;
                }
                EnsureArrayCapacity(numBatchesToProcess);
                // Draw the batches
                for(int i = 0; i < numBatchesToProcess; i++, batchIndex++) 
                {
                    SpriteBatchItem item = _batchItemList[batchIndex];
                    // if the texture changed, we need to flush and bind the new texture
                    var shouldFlush = !ReferenceEquals(item.Texture, tex);
                    if (shouldFlush)
                    {
                        FlushVertexArray(startIndex, index, effect);

                        tex = item.Texture;
                        startIndex = index = 0;
                        _device.Textures[0] = tex;
                    }

                    // store the SpriteBatchItem data in our vertexArray
                    _vertexArray[index++] = item.vertexTL;
                    _vertexArray[index++] = item.vertexTR;
                    _vertexArray[index++] = item.vertexBL;
                    _vertexArray[index++] = item.vertexBR;

                    // Release the texture and return the item to the queue.
                    item.Texture = null;
                    _freeBatchItemQueue.Enqueue(item);
                }
                // flush the remaining vertexArray data
                FlushVertexArray(startIndex, index, effect);
                // Update our batch count to continue the process of culling down
                // large batches
                batchCount -= numBatchesToProcess;
            }
			_batchItemList.Clear();
		}
				
        /// <summary>
        /// Sends the triangle list to the graphics device. Here is where the actual drawing starts.
        /// </summary>
        /// <param name="start">Start index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
        /// <param name="end">End index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
        /// <param name="effect">The custom effect to apply to the geometry</param>
        private void FlushVertexArray(int start, int end, Effect effect)
        {
            if (start == end)
                return;

            var vertexCount = end - start;

            // If the effect is not null, then apply each pass and render the geometry
            if (effect != null)
            {
                var passes = effect.CurrentTechnique.Passes;
                foreach (var pass in passes)
                {
                    pass.Apply();

                    _device.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        _vertexArray,
                        0,
                        vertexCount,
                        _index,
                        0,
                        (vertexCount / 4) * 2,
                        VertexPositionColorTexture.VertexDeclaration);
                }
            }
            else
            {
                // If no custom effect is defined, then simply render.
                _device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertexArray,
                    0,
                    vertexCount,
                    _index,
                    0,
                    (vertexCount / 4) * 2,
                    VertexPositionColorTexture.VertexDeclaration);
            }
        }
	}
}

