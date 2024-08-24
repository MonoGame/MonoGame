// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a collection of <see cref="SamplerState"/> objects,
    /// </summary>
    public sealed partial class SamplerStateCollection
	{
        private readonly GraphicsDevice _graphicsDevice;

        private readonly SamplerState _samplerStateAnisotropicClamp;
        private readonly SamplerState _samplerStateAnisotropicWrap;
        private readonly SamplerState _samplerStateLinearClamp;
        private readonly SamplerState _samplerStateLinearWrap;
        private readonly SamplerState _samplerStatePointClamp;
        private readonly SamplerState _samplerStatePointWrap;

        private readonly SamplerState[] _samplers;
        private readonly SamplerState[] _actualSamplers;
        private readonly bool _applyToVertexStage;

		internal SamplerStateCollection(GraphicsDevice device, int maxSamplers, bool applyToVertexStage)
		{
		    _graphicsDevice = device;

            _samplerStateAnisotropicClamp = SamplerState.AnisotropicClamp.Clone();
            _samplerStateAnisotropicWrap = SamplerState.AnisotropicWrap.Clone();
            _samplerStateLinearClamp = SamplerState.LinearClamp.Clone();
            _samplerStateLinearWrap = SamplerState.LinearWrap.Clone();
            _samplerStatePointClamp = SamplerState.PointClamp.Clone();
            _samplerStatePointWrap = SamplerState.PointWrap.Clone();

            _samplers = new SamplerState[maxSamplers];
            _actualSamplers = new SamplerState[maxSamplers];
            _applyToVertexStage = applyToVertexStage;

		    Clear();
        }

        /// <summary>
        /// Gets or sets the <see cref="SamplerState"/> at the specified index in the collection.
        /// </summary>
        public SamplerState this [int index]
        {
			get
            {
                return _samplers[index];
            }

			set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (_samplers[index] == value)
                    return;

                _samplers[index] = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newSamplerState = value;
                if (ReferenceEquals(value, SamplerState.AnisotropicClamp))
                    newSamplerState = _samplerStateAnisotropicClamp;
                else if (ReferenceEquals(value, SamplerState.AnisotropicWrap))
                    newSamplerState = _samplerStateAnisotropicWrap;
                else if (ReferenceEquals(value, SamplerState.LinearClamp))
                    newSamplerState = _samplerStateLinearClamp;
                else if (ReferenceEquals(value, SamplerState.LinearWrap))
                    newSamplerState = _samplerStateLinearWrap;
                else if (ReferenceEquals(value, SamplerState.PointClamp))
                    newSamplerState = _samplerStatePointClamp;
                else if (ReferenceEquals(value, SamplerState.PointWrap))
                    newSamplerState = _samplerStatePointWrap;

                newSamplerState.BindToGraphicsDevice(_graphicsDevice);

                _actualSamplers[index] = newSamplerState;

                PlatformSetSamplerState(index);
            }
		}

        internal void Clear()
        {
            for (var i = 0; i < _samplers.Length; i++)
            {
                _samplers[i] = SamplerState.LinearWrap;

                _samplerStateLinearWrap.BindToGraphicsDevice(_graphicsDevice);
                _actualSamplers[i] = _samplerStateLinearWrap;
            }

            PlatformClear();
        }

        /// <summary>
        /// Mark all the sampler slots as dirty.
        /// </summary>
        internal void Dirty()
        {
            PlatformDirty();
        }
	}
}
