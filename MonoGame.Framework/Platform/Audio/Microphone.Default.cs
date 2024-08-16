// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Provides microphones capture features. 
    /// </summary>	
    public sealed partial class Microphone
    {
        internal void PlatformStart()
        {
			throw new NotImplementedException();
        }

        internal void PlatformStop()
        {
			throw new NotImplementedException();
        }
		
		internal void Update()
		{
			throw new NotImplementedException();
		}
		
		internal int PlatformGetData(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
