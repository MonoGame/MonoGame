// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MonoGame.Framework.Graphics
{
    public class Image2DArray : IReadOnlyList<Image2D>
    {
        internal Image2D[] _array;
        private int _width;
        private int _height;

        public Image2D this[int index]
        {
            get { return _array[index]; }
            internal set
            {
                _array[index] = value;
            }
        }

        public int Count { get { return _array.Length; } }

        public int Width { get { return _width; } }

        public int Height { get { return _height; } }

        public Image2DArray(int width, int height, Image2D[] array)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height");
            if (array == null)
                throw new ArgumentNullException("array");

            _array = array;
            _width = width;
            _height = height;
        }

        public IEnumerator<Image2D> GetEnumerator()
        {
            return ((IEnumerable<Image2D>)_array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _array.GetEnumerator();
        }
    }
}
