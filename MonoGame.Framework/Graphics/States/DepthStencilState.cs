// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Contains depth-stencil state for the device.
    /// </summary>
    public partial class DepthStencilState : GraphicsResource
    {
        private readonly bool _defaultStateObject;

        private bool _depthBufferEnable;
        private bool _depthBufferWriteEnable;
        private StencilOperation _counterClockwiseStencilDepthBufferFail;
        private StencilOperation _counterClockwiseStencilFail;
        private CompareFunction _counterClockwiseStencilFunction;
        private StencilOperation _counterClockwiseStencilPass;
        private CompareFunction _depthBufferFunction;
        private int _referenceStencil;
        private StencilOperation _stencilDepthBufferFail;
        private bool _stencilEnable;
        private StencilOperation _stencilFail;
        private CompareFunction _stencilFunction;
        private int _stencilMask;
        private StencilOperation _stencilPass;
        private int _stencilWriteMask;
        private bool _twoSidedStencilMode;

        /// <summary>
        /// Gets or Sets a value that indicates if depth buffering is enabled.
        /// The default value is true.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public bool DepthBufferEnable
        {
            get { return _depthBufferEnable; }
            set
            {
                ThrowIfBound();
                _depthBufferEnable = value;
            }
        }

        /// <summary>
        /// Gets or Sets a value that indicates if writing to the depth buffer is enabled.
        /// The default value is true.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This property enables an application to prevent the system from updating the depth buffer with new
        ///         values.
        ///     </para>
        ///     <para>
        ///         if false, depth comparisons are still made according to the render state
        ///         <see cref="DepthBufferFunction"/>, assuming that depth buffering is taking place, but depth values
        ///         are not written to the buffer.
        ///     </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public bool DepthBufferWriteEnable
        {
            get { return _depthBufferWriteEnable; }
            set
            {
                ThrowIfBound();
                _depthBufferWriteEnable = value;
            }
        }

        /// <summary>
        /// Gets or Sets the stencil operation to perform if the stencil test passes and the depth-buffer test fails
        /// for a counterclockwise triangle.
        /// The default value is <see cref="StencilOperation.Keep">StencilOperation.Keep</see>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public StencilOperation CounterClockwiseStencilDepthBufferFail
        {
            get { return _counterClockwiseStencilDepthBufferFail; }
            set
            {
                ThrowIfBound();
                _counterClockwiseStencilDepthBufferFail = value;
            }
        }

        /// <summary>
        /// Gets or Sets the stencil operation to perform if the stencil test fails for the counterclockwise triangle.
        /// The default value is <see cref="StencilOperation.Keep">StencilOperation.Keep</see>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public StencilOperation CounterClockwiseStencilFail
        {
            get { return _counterClockwiseStencilFail; }
            set
            {
                ThrowIfBound();
                _counterClockwiseStencilFail = value;
            }
        }

        /// <summary>
        /// Gets or Sets the comparison function to use for counterclockwise stencil tests.
        /// The default value is <see cref="CompareFunction.Always">CompareFunction.Always</see>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public CompareFunction CounterClockwiseStencilFunction
        {
            get { return _counterClockwiseStencilFunction; }
            set
            {
                ThrowIfBound();
                _counterClockwiseStencilFunction = value;
            }
        }

        /// <summary>
        /// Gets or Sets the stencil operation to perform if the stencil and depth-tests pass for a counterclockwise
        /// triangle.
        /// The default value is <see cref="StencilOperation.Keep">StencilOperation.Keep</see>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public StencilOperation CounterClockwiseStencilPass
        {
            get { return _counterClockwiseStencilPass; }
            set
            {
                ThrowIfBound();
                _counterClockwiseStencilPass = value;
            }
        }

        /// <summary>
        /// Gets or Sets the comparison function for the depth-buffer test.
        /// The default value is <see cref="CompareFunction.LessEqual">CompareFunction.LessEqual</see>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public CompareFunction DepthBufferFunction
        {
            get { return _depthBufferFunction; }
            set
            {
                ThrowIfBound();
                _depthBufferFunction = value;
            }
        }

        public int ReferenceStencil
        {
            get { return _referenceStencil; }
            set
            {
                ThrowIfBound();
                _referenceStencil = value;
            }
        }

        public StencilOperation StencilDepthBufferFail
        {
            get { return _stencilDepthBufferFail; }
            set
            {
                ThrowIfBound();
                _stencilDepthBufferFail = value;
            }
        }

        public bool StencilEnable
        {
            get { return _stencilEnable; }
            set
            {
                ThrowIfBound();
                _stencilEnable = value;
            }
        }

        public StencilOperation StencilFail
        {
            get { return _stencilFail; }
            set
            {
                ThrowIfBound();
                _stencilFail = value;
            }
        }

        public CompareFunction StencilFunction
        {
            get { return _stencilFunction; }
            set
            {
                ThrowIfBound();
                _stencilFunction = value;
            }
        }

        public int StencilMask
        {
            get { return _stencilMask; }
            set
            {
                ThrowIfBound();
                _stencilMask = value;
            }
        }

        public StencilOperation StencilPass
        {
            get { return _stencilPass; }
            set
            {
                ThrowIfBound();
                _stencilPass = value;
            }
        }

        public int StencilWriteMask
        {
            get { return _stencilWriteMask; }
            set
            {
                ThrowIfBound();
                _stencilWriteMask = value;
            }
        }

        public bool TwoSidedStencilMode
        {
            get { return _twoSidedStencilMode; }
            set
            {
                ThrowIfBound();
                _twoSidedStencilMode = value;
            }
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot bind a default state object.");
            if (GraphicsDevice != null && GraphicsDevice != device)
                throw new InvalidOperationException("This depth stencil state is already bound to a different graphics device.");
            GraphicsDevice = device;
        }

        internal void ThrowIfBound()
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
            if (GraphicsDevice != null)
                throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");
        }

        public DepthStencilState ()
		{
            DepthBufferEnable = true;
            DepthBufferWriteEnable = true;
			DepthBufferFunction = CompareFunction.LessEqual;
			StencilEnable = false;
			StencilFunction = CompareFunction.Always;
			StencilPass = StencilOperation.Keep;
			StencilFail = StencilOperation.Keep;
			StencilDepthBufferFail = StencilOperation.Keep;
			TwoSidedStencilMode = false;
			CounterClockwiseStencilFunction = CompareFunction.Always;
			CounterClockwiseStencilFail = StencilOperation.Keep;
			CounterClockwiseStencilPass = StencilOperation.Keep;
			CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep;
			StencilMask = Int32.MaxValue;
			StencilWriteMask = Int32.MaxValue;
			ReferenceStencil = 0;
		}

        private DepthStencilState(string name, bool depthBufferEnable, bool depthBufferWriteEnable)
            : this()
	    {
	        Name = name;
            _depthBufferEnable = depthBufferEnable;
            _depthBufferWriteEnable = depthBufferWriteEnable;
	        _defaultStateObject = true;
	    }

        private DepthStencilState(DepthStencilState cloneSource)
	    {
	        Name = cloneSource.Name;
            _depthBufferEnable = cloneSource._depthBufferEnable;
            _depthBufferWriteEnable = cloneSource._depthBufferWriteEnable;
            _counterClockwiseStencilDepthBufferFail = cloneSource._counterClockwiseStencilDepthBufferFail;
            _counterClockwiseStencilFail = cloneSource._counterClockwiseStencilFail;
            _counterClockwiseStencilFunction = cloneSource._counterClockwiseStencilFunction;
            _counterClockwiseStencilPass = cloneSource._counterClockwiseStencilPass;
            _depthBufferFunction = cloneSource._depthBufferFunction;
            _referenceStencil = cloneSource._referenceStencil;
            _stencilDepthBufferFail = cloneSource._stencilDepthBufferFail;
            _stencilEnable = cloneSource._stencilEnable;
            _stencilFail = cloneSource._stencilFail;
            _stencilFunction = cloneSource._stencilFunction;
            _stencilMask = cloneSource._stencilMask;
            _stencilPass = cloneSource._stencilPass;
            _stencilWriteMask = cloneSource._stencilWriteMask;
            _twoSidedStencilMode = cloneSource._twoSidedStencilMode;
	    }

        public static readonly DepthStencilState Default;
        public static readonly DepthStencilState DepthRead;
        public static readonly DepthStencilState None;

		static DepthStencilState ()
		{
		    Default = new DepthStencilState("DepthStencilState.Default", true, true);
			DepthRead = new DepthStencilState("DepthStencilState.DepthRead", true, false);
		    None = new DepthStencilState("DepthStencilState.None", false, false);
		}

        internal DepthStencilState Clone()
        {
            return new DepthStencilState(this);
        }

        partial void PlatformDispose();

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                PlatformDispose();
            }
            base.Dispose(disposing);
        }
    }
}

