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
                
        /// <summary>
        /// Sorts the batch items and then groups batch drawing into maximal allowed batch sets that do not
        /// overflow the 16 bit array indices for vertices.
        /// </summary>
        /// <param name="sortMode">The type of depth sorting desired for the rendering.</param>
        /// <param name="customEffect">Optional custom effect passed by user via SpriteBatch.Begin.</param>
        /// <param name="defaultSpriteEffect">The standard SpriteEffect used for non-distance-field glyphs.</param>
        /// <param name="distanceFieldEffect">The distance field sprite effect used when a SpriteFont has distance field metadata.</param>
		public unsafe void DrawBatch(SpriteSortMode sortMode, Effect customEffect, Effect defaultSpriteEffect, Effect distanceFieldEffect)
		{
            if (customEffect != null && customEffect.IsDisposed)
                throw new ObjectDisposedException("effect");

            if (distanceFieldEffect != null && distanceFieldEffect.IsDisposed)
                throw new ObjectDisposedException("distanceFieldEffect");

			// nothing to do
            if (_batchItemCount == 0)
				return;

            // sort the batch items
            switch (sortMode)
            {
                case SpriteSortMode.Texture:
                case SpriteSortMode.FrontToBack:
                case SpriteSortMode.BackToFront:
                    Array.Sort(_batchItemList, 0, _batchItemCount);
                    break;
            }

            // Determine how many iterations through the drawing code we need to make
            int batchIndex = 0;
            int batchCount = _batchItemCount;

            
            unchecked
            {
                _device._graphicsMetrics._spriteCount += batchCount;
            }

            // Iterate through the batches, doing short.MaxValue sets of vertices only.
            while (batchCount > 0)
            {
                var startIndex = 0;
                var index = 0;
                Texture2D tex = null;
                int currentVariant = -1;
                float currentVariantDFSpread = 0f;
                float currentVariantDFOutlineThickness = 0f;
                Vector4 currentVariantDFOutlineColor = Vector4.Zero;
                int currentVariantDFSpreadKey = 0;
                int currentVariantDFOutlineThicknessKey = 0;
                uint currentVariantDFOutlineColorKey = 0;

                int numBatchesToProcess = batchCount;
                if (numBatchesToProcess > MaxBatchSize)
                    numBatchesToProcess = MaxBatchSize;

                Effect GetEffectForVariant(int shaderVariant)
                {
                    if (shaderVariant == 0 || shaderVariant == 1)
                    {
                        if (shaderVariant == 1)
                        {
                            if (distanceFieldEffect != null)
                            {
                                return distanceFieldEffect;
                            }
                            else
                            {
                                throw new NotSupportedException($"shaderVariant == 1 but distanceFieldEffect is null, is not supported");
                            }
                        }

                        return (customEffect != null) ? customEffect : defaultSpriteEffect;
                    }
                    else
                    {
                        throw new NotSupportedException($"Unknown shaderVariant");
                    }
                }

                fixed (VertexPositionColorTexture* vertexArrayFixedPtr = _vertexArray)
                {
                    var vertexArrayPtr = vertexArrayFixedPtr;
                    for (int i = 0; i < numBatchesToProcess; i++, batchIndex++, index += 4, vertexArrayPtr += 4)
                    {
                        var item = _batchItemList[batchIndex];
                        bool groupMismatch = false;
                        if (currentVariant == -1)
                        {
                            tex = item.Texture;
                            currentVariant = item.ShaderVariant;
                            currentVariantDFSpread = item.DFSpread;
                            currentVariantDFOutlineThickness = item.DFOutlineThickness;
                            currentVariantDFOutlineColor = item.DFOutlineColor;
                            currentVariantDFSpreadKey = item.DFSpreadKey;
                            currentVariantDFOutlineThicknessKey = item.DFOutlineThicknessKey;
                            currentVariantDFOutlineColorKey = item.DFOutlineColorKey;
                            _device.Textures[0] = tex;
                        }
                        else
                        {
                            groupMismatch = !ReferenceEquals(item.Texture, tex) || item.ShaderVariant != currentVariant ||
                                (item.ShaderVariant == 1 && (item.DFSpreadKey != currentVariantDFSpreadKey ||
                                    item.DFOutlineThicknessKey != currentVariantDFOutlineThicknessKey || item.DFOutlineColorKey != currentVariantDFOutlineColorKey));
                        }

                        if (groupMismatch)
                        {
                            var effectToUse = GetEffectForVariant(currentVariant);
                            FlushVertexArray(startIndex, index, effectToUse, tex, currentVariant, currentVariantDFSpread, currentVariantDFOutlineThickness, currentVariantDFOutlineColor);
                            startIndex = index = 0;
                            vertexArrayPtr = vertexArrayFixedPtr;
                            tex = item.Texture;
                            currentVariant = item.ShaderVariant;
                            currentVariantDFSpread = item.DFSpread;
                            currentVariantDFOutlineThickness = item.DFOutlineThickness;
                            currentVariantDFOutlineColor = item.DFOutlineColor;
                            currentVariantDFSpreadKey = item.DFSpreadKey;
                            currentVariantDFOutlineThicknessKey = item.DFOutlineThicknessKey;
                            currentVariantDFOutlineColorKey = item.DFOutlineColorKey;
                            _device.Textures[0] = tex;
                        }

                        *(vertexArrayPtr + 0) = item.vertexTL;
                        *(vertexArrayPtr + 1) = item.vertexTR;
                        *(vertexArrayPtr + 2) = item.vertexBL;
                        *(vertexArrayPtr + 3) = item.vertexBR;
                        item.Texture = null;
                    }

                    if (currentVariant != -1)
                    {
                        var finalEffect = GetEffectForVariant(currentVariant);
                        FlushVertexArray(startIndex, index, finalEffect, tex, currentVariant, currentVariantDFSpread, currentVariantDFOutlineThickness, currentVariantDFOutlineColor);
                    }
                }

                batchCount -= numBatchesToProcess;
            }
            // return items to the pool.  
            _batchItemCount = 0;
		}

        /// <summary>
        /// Sends the triangle list to the graphics device. Here is where the actual drawing starts.
        /// </summary>
        /// <param name="start">Start index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
        /// <param name="end">End index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
        /// <param name="effect">The custom effect to apply to the geometry</param>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="shaderVariant">0 for normal sprites/glyphs, 1 for distance field glyphs.</param>
        /// <param name="dfSpread">Distance field spread (in texels) used to normalize smoothing width.</param>
        /// <param name="dfOutlineThickness">Outline thickness in normalised distance units (0 = no outline).</param>
        /// <param name="dfOutlineColor">Outline colour as a Vector4 (RGBA).</param>
        private void FlushVertexArray(int start, int end, Effect effect, Texture texture, int shaderVariant, float dfSpread, float dfOutlineThickness, Vector4 dfOutlineColor)
        {
            if (start == end)
                return;

            var vertexCount = end - start;

            // If the effect is not null, then apply each pass and render the geometry
            if (effect != null)
            {
                if (shaderVariant == 1)
                {
                    if (effect is DistanceFieldSpriteEffect dfs)
                    {
                        dfs.ApplyDistanceFieldSettings(dfSpread);
                        if (dfOutlineThickness > 0f)
                            dfs.SetOutline(dfOutlineThickness, dfOutlineColor);
                        else
                            dfs.ClearOutline();
                    }
                    if (effect is not DistanceFieldSpriteEffect)
                    {
                        throw new NotSupportedException($"shaderVariant == 1 is not compatible with non distance field effects.");
                    }
                }

                var passes = effect.CurrentTechnique.Passes;
                foreach (var pass in passes)
                {
                    pass.Apply();
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
                if (shaderVariant == 1)
                {
                    throw new NotSupportedException($"shaderVariant == 1 and no effect set.");
                }
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

