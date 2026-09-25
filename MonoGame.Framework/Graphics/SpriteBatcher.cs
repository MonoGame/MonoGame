// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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
	    private SpriteBatchItem[] _batchItemList;
        /// <summary>
        /// Index pointer to the next available SpriteBatchItem in _batchItemList.
        /// </summary>
        private int _batchItemCount;
        
        /// <summary>
        /// The target graphics device.
        /// </summary>
        private readonly GraphicsDevice _device;

        /// <summary>
        /// Vertex index array. The values in this array never change.
        /// </summary>
        private short[] _index;

        private VertexPositionColorTexture[] _vertexArray;

        private DynamicVertexBuffer _vertexBuffer;

        private DynamicIndexBuffer _indexBuffer;

        private TextureRun[] _textureRuns;

        public SpriteBatcher(GraphicsDevice device, int capacity = 0)
		{
            _device = device;

            if (capacity <= 0)
                capacity = InitialBatchSize;
            else
                capacity = (capacity + 63) & (~63); // ensure chunks of 64.

            _batchItemList = new SpriteBatchItem[capacity];
            _batchItemCount = 0;

            for (int i = 0; i < capacity; i++)
                _batchItemList[i] = new SpriteBatchItem();

            EnsureArrayCapacity(capacity);
            _textureRuns = new TextureRun[capacity];
		}

        /// <summary>
        /// Reuse a previously allocated SpriteBatchItem from the item pool. 
        /// if there is none available grow the pool and initialize new items.
        /// </summary>
        /// <returns></returns>
        public SpriteBatchItem CreateBatchItem()
        {
            if (_batchItemCount >= _batchItemList.Length)
            {
                var oldSize = _batchItemList.Length;
                var newSize = oldSize + oldSize/2; // grow by x1.5
                newSize = (newSize + 63) & (~63); // grow in chunks of 64.
                Array.Resize(ref _batchItemList, newSize);
                for(int i=oldSize; i<newSize; i++)
                    _batchItemList[i]=new SpriteBatchItem();

                EnsureArrayCapacity(Math.Min(newSize, MaxBatchSize));
            }
            var item = _batchItemList[_batchItemCount++];
            return item;
        }

        /// <summary>
        /// Resize and recreate the missing indices for the index and vertex position color buffers.
        /// </summary>
        /// <param name="numBatchItems"></param>
        private unsafe void EnsureArrayCapacity(int numBatchItems)
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
            fixed (short* indexFixedPtr = newIndex)
            {
                var indexPtr = indexFixedPtr + (start * 6);
                for (var i = start; i < numBatchItems; i++, indexPtr += 6)
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
                    *(indexPtr + 0) = (short)(i * 4);
                    *(indexPtr + 1) = (short)(i * 4 + 1);
                    *(indexPtr + 2) = (short)(i * 4 + 2);
                    // Triangle 2
                    *(indexPtr + 3) = (short)(i * 4 + 1);
                    *(indexPtr + 4) = (short)(i * 4 + 3);
                    *(indexPtr + 5) = (short)(i * 4 + 2);
                }
            }
            _index = newIndex;

            _vertexArray = new VertexPositionColorTexture[4 * numBatchItems];
        }

        private void EnsureBufferCapacity(int numBatchItems)
        {
            int vertexCount = 4 * numBatchItems;
            int indexCount = 6 * numBatchItems;

            if (_vertexBuffer != null
                && _vertexBuffer.VertexCount >= vertexCount
                && _indexBuffer != null
                && _indexBuffer.IndexCount >= indexCount)
            {
                return;
            }

            if (_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
            }

            if (_indexBuffer != null)
            {
                _indexBuffer.Dispose();
            }

            _vertexBuffer = new DynamicVertexBuffer(
                _device,
                VertexPositionColorTexture.VertexDeclaration,
                vertexCount,
                BufferUsage.WriteOnly);
            _indexBuffer = new DynamicIndexBuffer(
                _device,
                IndexElementSize.SixteenBits,
                indexCount,
                BufferUsage.WriteOnly);
        }

        private void EnsureTextureRunCapacity(int numBatchItems)
        {
            if (numBatchItems <= _textureRuns.Length)
            {
                return;
            }

            Array.Resize(ref _textureRuns, numBatchItems);
        }

        /// <summary>
        /// Sorts the batch items and then groups batch drawing into maximal allowed batch sets that do not
        /// overflow the 16 bit array indices for vertices.
        /// </summary>
        /// <param name="sortMode">The type of depth sorting desired for the rendering.</param>
        /// <param name="effect">The custom effect to apply to the drawn geometry</param>
        public unsafe void DrawBatch(SpriteSortMode sortMode, Effect effect)
		{
            if (effect != null && effect.IsDisposed)
                throw new ObjectDisposedException("effect");

			// nothing to do
            if (_batchItemCount == 0)
				return;
			
			// sort the batch items
			switch ( sortMode )
			{
			case SpriteSortMode.Texture :                
			case SpriteSortMode.FrontToBack :
			case SpriteSortMode.BackToFront :
                Array.Sort(_batchItemList, 0, _batchItemCount);
				break;
			}

            int batchCount = _batchItemCount;

            
            unchecked
            {
                _device._graphicsMetrics._spriteCount += batchCount;
            }

            if (sortMode == SpriteSortMode.Immediate)
            {
                DrawImmediateBatch(effect);
                _batchItemCount = 0;
                return;
            }

            DrawBufferedBatch(effect);

            // return items to the pool.
            _batchItemCount = 0;
        }

        /// <summary>
        /// Draws an immediate batch without changing its upload and draw timing.
        /// </summary>
        /// <param name="effect">The custom effect to apply to the geometry.</param>
        private unsafe void DrawImmediateBatch(Effect effect)
        {
            int batchIndex = 0;
            int batchCount = _batchItemCount;

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
                // Avoid the array checking overhead by using pointer indexing!
                fixed (VertexPositionColorTexture* vertexArrayFixedPtr = _vertexArray)
                {
                    var vertexArrayPtr = vertexArrayFixedPtr;

                    // Draw the batches
                    for (int i = 0; i < numBatchesToProcess; i++, batchIndex++, index += 4, vertexArrayPtr += 4)
                    {
                        SpriteBatchItem item = _batchItemList[batchIndex];
                        // if the texture changed, we need to flush and bind the new texture
                        var shouldFlush = !ReferenceEquals(item.Texture, tex);
                        if (shouldFlush)
                        {
                            FlushVertexArray(startIndex, index, effect, tex);

                            tex = item.Texture;
                            startIndex = index = 0;
                            vertexArrayPtr = vertexArrayFixedPtr;
                            _device.Textures[0] = tex;
                        }

                        // store the SpriteBatchItem data in our vertexArray
                        *(vertexArrayPtr+0) = item.vertexTL;
                        *(vertexArrayPtr+1) = item.vertexTR;
                        *(vertexArrayPtr+2) = item.vertexBL;
                        *(vertexArrayPtr+3) = item.vertexBR;

                        // Release the texture.
                        item.Texture = null;
                    }
                }
                // flush the remaining vertexArray data
                FlushVertexArray(startIndex, index, effect, tex);
                // Update our batch count to continue the process of culling down
                // large batches
                batchCount -= numBatchesToProcess;
            }
		}

        /// <summary>
        /// Draws each existing processing chunk after uploading its vertices and indices once.
        /// </summary>
        /// <param name="effect">The custom effect to apply to the geometry.</param>
        private unsafe void DrawBufferedBatch(Effect effect)
        {
            int batchIndex = 0;
            int batchCount = _batchItemCount;

            while (batchCount > 0)
            {
                int numBatchesToProcess = Math.Min(batchCount, MaxBatchSize);
                EnsureBufferCapacity(numBatchesToProcess);
                EnsureTextureRunCapacity(numBatchesToProcess);

                int textureRunCount = 0;
                int textureRunStart = 0;
                Texture2D texture = null;

                // Avoid the array checking overhead by using pointer indexing!
                fixed (VertexPositionColorTexture* vertexArrayFixedPtr = _vertexArray)
                {
                    VertexPositionColorTexture* vertexArrayPtr = vertexArrayFixedPtr;

                    // Build the chunk's vertices and contiguous texture runs.
                    for (int i = 0; i < numBatchesToProcess; i++, batchIndex++, vertexArrayPtr += 4)
                    {
                        SpriteBatchItem item = _batchItemList[batchIndex];

                        // A texture change ends the previous run and starts a new one.
                        if (!ReferenceEquals(item.Texture, texture))
                        {
                            if (texture != null)
                            {
                                _textureRuns[textureRunCount++] = new TextureRun(
                                    texture,
                                    textureRunStart,
                                    i - textureRunStart);
                            }

                            texture = item.Texture;
                            textureRunStart = i;
                        }

                        // Store the SpriteBatchItem data in the chunk vertex array.
                        *(vertexArrayPtr + 0) = item.vertexTL;
                        *(vertexArrayPtr + 1) = item.vertexTR;
                        *(vertexArrayPtr + 2) = item.vertexBL;
                        *(vertexArrayPtr + 3) = item.vertexBR;

                        // Release the texture.
                        item.Texture = null;
                    }
                }

                // Record the final texture run in the chunk.
                if (texture != null)
                {
                    _textureRuns[textureRunCount++] = new TextureRun(
                        texture,
                        textureRunStart,
                        numBatchesToProcess - textureRunStart);
                }

                _vertexBuffer.SetData(
                    _vertexArray,
                    0,
                    4 * numBatchesToProcess,
                    SetDataOptions.Discard);
                _indexBuffer.SetData(
                    _index,
                    0,
                    6 * numBatchesToProcess,
                    SetDataOptions.Discard);
                _device.SetVertexBuffer(_vertexBuffer);
                _device.Indices = _indexBuffer;

                // Draw each texture run in its original order from the uploaded chunk.
                try
                {
                    for (int i = 0; i < textureRunCount; i++)
                    {
                        DrawTextureRun(_textureRuns[i], effect);
                    }
                }
                finally
                {
                    Array.Clear(_textureRuns, 0, textureRunCount);
                }

                batchCount -= numBatchesToProcess;
            }
        }

        /// <summary>
        /// Sends the triangle list to the graphics device. Here is where the actual drawing starts.
        /// </summary>
        /// <param name="start">Start index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
        /// <param name="end">End index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
        /// <param name="effect">The custom effect to apply to the geometry</param>
        /// <param name="texture">The texture to draw.</param>
        private void FlushVertexArray(int start, int end, Effect effect, Texture texture)
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

                    // Whatever happens in pass.Apply, make sure the texture being drawn
                    // ends up in Textures[0].
                    _device.Textures[0] = texture;

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

        private void DrawTextureRun(TextureRun textureRun, Effect effect)
        {
            if (effect != null)
            {
                EffectPassCollection passes = effect.CurrentTechnique.Passes;
                foreach (EffectPass pass in passes)
                {
                    pass.Apply();
                    _device.Textures[0] = textureRun.Texture;
                    _device.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        textureRun.StartSprite * 6,
                        textureRun.SpriteCount * 2);
                }
            }
            else
            {
                _device.Textures[0] = textureRun.Texture;
                _device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    textureRun.StartSprite * 6,
                    textureRun.SpriteCount * 2);
            }
        }

        /// <summary>
        /// Releases the buffers owned by this batcher.
        /// </summary>
        public void Dispose()
        {
            if (_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }

            if (_indexBuffer != null)
            {
                _indexBuffer.Dispose();
                _indexBuffer = null;
            }
        }

        private readonly struct TextureRun
        {
            public readonly Texture2D Texture;
            public readonly int StartSprite;
            public readonly int SpriteCount;

            public TextureRun(Texture2D texture, int startSprite, int spriteCount)
            {
                Texture = texture;
                StartSprite = startSprite;
                SpriteCount = spriteCount;
            }
        }
	}
}
