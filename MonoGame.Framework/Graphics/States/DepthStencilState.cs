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

        /// <summary>
        /// Gets or Sets a reference value to use for the stencil test.
        /// The default is 0.
        /// </summary>
        /// <remarks>
        /// The reference value is compared, by the comparison function specified by the <see cref="StencilFunction"/>
        /// property, to the stencil buffer entry of a pixel.  This can be illustrated by a simple equation:
        ///
        /// <code>
        /// ReferenceStencil StencilFunction (stencil buffer entry)
        /// </code>
        ///
        /// This comparison applies only to the bits in the reference value and stencil buffer entry that are set in
        /// the stencil mask by this property.  If the comparison is true, the stencil test passes and the pass
        /// operation (specified by the <see cref="StencilPass"/> property) is performed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public int ReferenceStencil
        {
            get { return _referenceStencil; }
            set
            {
                ThrowIfBound();
                _referenceStencil = value;
            }
        }

        /// <summary>
        /// Gets or Sets the stencil operation to perform if the stencil test passes and the depth-test fails.
        /// The default is <see cref="StencilOperation.Keep">StencilOperation.Keep</see>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public StencilOperation StencilDepthBufferFail
        {
            get { return _stencilDepthBufferFail; }
            set
            {
                ThrowIfBound();
                _stencilDepthBufferFail = value;
            }
        }

        /// <summary>
        /// Gets or Sets a value that indicates whether stenciling is enabled.
        /// The default is false.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public bool StencilEnable
        {
            get { return _stencilEnable; }
            set
            {
                ThrowIfBound();
                _stencilEnable = value;
            }
        }

        /// <summary>
        /// Gets or Sets the stencil operation to perform if the stencil test fails.
        /// The default is <see cref="StencilOperation.Keep">StencilOperation.Keep</see>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public StencilOperation StencilFail
        {
            get { return _stencilFail; }
            set
            {
                ThrowIfBound();
                _stencilFail = value;
            }
        }

        /// <summary>
        /// Gets or Sets the comparison function for the stencil test.
        /// The default is <see cref="CompareFunction.Always">CompareFunction.Always</see>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public CompareFunction StencilFunction
        {
            get { return _stencilFunction; }
            set
            {
                ThrowIfBound();
                _stencilFunction = value;
            }
        }

        /// <summary>
        /// Gets or Sets the mask applied to the reference value and each stencil buffer entry to determine the
        /// significant bits for the stencil test.
        /// The default mask is <see cref="Int32.MaxValue">Int32.MaxValue</see>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public int StencilMask
        {
            get { return _stencilMask; }
            set
            {
                ThrowIfBound();
                _stencilMask = value;
            }
        }

        /// <summary>
        /// Gets or Sets the stencil operation to perform if the stencil test passes.
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
        public StencilOperation StencilPass
        {
            get { return _stencilPass; }
            set
            {
                ThrowIfBound();
                _stencilPass = value;
            }
        }

        /// <summary>
        /// Gets or Sets the write mask applied to values written into the stencil buffer.
        /// The default mask is <see cref="Int32.MaxValue">Int32.MaxValue</see>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
        public int StencilWriteMask
        {
            get { return _stencilWriteMask; }
            set
            {
                ThrowIfBound();
                _stencilWriteMask = value;
            }
        }

        /// <summary>
        /// Gets or Sets a value that indicates whether two-sided stenciling is enabled.
        /// The default value is false.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// When setting this value for one of the default DepthStencilState instances; <see cref="Default"/>,
        /// <see cref="DepthRead"/>, or <see cref="None"/>.
        ///
        /// -or-
        ///
        /// When setting this value after this DepthStencilState has already been bound to the graphics device.
        /// </exception>
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

        /// <summary>
        /// Creates a new instance of the DepthStencilState class with default values.
        /// </summary>
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

        /// <summary>
        /// A built-in state object with default settings for using a depth stencil buffer.
        /// </summary>
        /// <remarks>
        /// This built-in state object has the following settings:
        ///
        /// <code>
        /// DepthBufferEnable = true,
        /// DepthBufferWriteEnable = true
        /// </code>
        /// </remarks>
        public static readonly DepthStencilState Default;

        /// <summary>
        /// A built-int state object with settings for enabling a read-only depth stencil buffer.
        /// </summary>
        /// <remarks>
        /// This built-in state object has the following settings:
        ///
        /// <code>
        /// DepthBufferEnable = true,
        /// DepthBufferWriteEnable = false
        /// </code>
        /// </remarks>
        public static readonly DepthStencilState DepthRead;

        /// <summary>
        /// A built in state object with settings for not using a depth stencil buffer.
        /// </summary>
        /// <remarks>
        /// This built-int state object has the following settings:
        ///
        /// <code>
        /// DepthBufferEnable = false,
        /// DepthBufferWriteEnable = false
        /// </code>
        /// </remarks>
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

        /// <inheritdoc />
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

