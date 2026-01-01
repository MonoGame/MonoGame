// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// A collection that manipulates <see cref="DisplayMode"/> instances.
    /// </summary>
    public class DisplayModeCollection : IEnumerable<DisplayMode>
    {
        private readonly List<DisplayMode> _modes;

        /// <summary>
        /// Gets the <see cref="DisplayMode"/> instance with the specified format.
        /// </summary>
        public IEnumerable<DisplayMode> this[SurfaceFormat format]
        {
            get 
            {
                var list = new List<DisplayMode>();
                foreach (var mode in _modes)
                {
                    if (mode.Format == format)
                        list.Add(mode);
                }
                return list;
            }
        }

        /// <inheritdoc />
        public IEnumerator<DisplayMode> GetEnumerator()
        {
            return _modes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _modes.GetEnumerator();
        }
        
        internal DisplayModeCollection(List<DisplayMode> modes) 
        {
            // Sort the modes in a consistent way that happens
            // to match XNA behavior on some graphics devices.

            modes.Sort(delegate(DisplayMode a, DisplayMode b)
            {
                if (a == b) 
                    return 0;
                if (a.Format <= b.Format && a.Width <= b.Width && a.Height <= b.Height) 
                    return -1;
                return 1;
            });

            _modes = modes;
        }
    }
}
