// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class SamplerStateCollection
	{
        private SamplerState[] _samplers;

		internal SamplerStateCollection( int maxSamplers )
        {
            _samplers = new SamplerState[maxSamplers];
		    Clear();
        }
		
		public SamplerState this [int index] 
        {
			get 
            { 
                return _samplers[index]; 
            }

			set 
            {
                if (_samplers[index] == value)
                    return;

                _samplers[index] = value;

                PlatformSetSamplerState(index);
            }
		}

        internal void Clear()
        {
            for (var i = 0; i < _samplers.Length; i++)
                _samplers[i] = SamplerState.LinearWrap;

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
