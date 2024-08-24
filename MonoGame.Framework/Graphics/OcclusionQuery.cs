// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Used to perform an occlusion query against the latest drawn objects.
    /// </summary>
    public partial class OcclusionQuery : GraphicsResource
    {
        private bool _inBeginEndPair;  // true if Begin was called and End was not yet called.
        private bool _queryPerformed;  // true if Begin+End were called at least once.
        private bool _isComplete;      // true if the result is available in _pixelCount.
        private int _pixelCount;       // The query result.

        /// <summary>
        /// Gets a value indicating whether the occlusion query has completed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the occlusion query has completed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsComplete
        {
            get
            {
                if (_isComplete)
                    return true;

                if (!_queryPerformed || _inBeginEndPair)
                    return false;

                _isComplete = PlatformGetResult(out _pixelCount);

                return _isComplete;
            }
        }

        /// <summary>
        /// Gets the number of visible pixels.
        /// </summary>
        /// <value>The number of visible pixels.</value>
        /// <exception cref="InvalidOperationException">
        /// The occlusion query has not yet completed. Check <see cref="IsComplete"/> before reading
        /// the result!
        /// </exception>
        public int PixelCount
        {
            get
            {
                if (!IsComplete)
                    throw new InvalidOperationException("The occlusion query has not yet completed. Check IsComplete before reading the result.");

                return _pixelCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OcclusionQuery"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The current graphics profile does not support occlusion queries.
        /// </exception>
        public OcclusionQuery(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
                throw new NotSupportedException("The Reach profile does not support occlusion queries.");

            GraphicsDevice = graphicsDevice;

            PlatformConstruct();
        }

        /// <summary>
        /// Begins the occlusion query.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="Begin"/> is called again before calling <see cref="End"/>.
        /// </exception>
        public void Begin()
        {
            if (_inBeginEndPair)
                throw new InvalidOperationException("End() must be called before calling Begin() again.");

            _inBeginEndPair = true;
            _isComplete = false;

            PlatformBegin();
        }

        /// <summary>
        /// Ends the occlusion query.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="End"/> is called before calling <see cref="Begin"/>.
        /// </exception>
        public void End()
        {
            if (!_inBeginEndPair)
                throw new InvalidOperationException("Begin() must be called before calling End().");

            _inBeginEndPair = false;
            _queryPerformed = true;

            PlatformEnd();
        }
    }
}
