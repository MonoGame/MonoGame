// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;

namespace MonoGame.Effect.TPGParser
{
    public class ShaderInfo
	{
		public List<TechniqueInfo> Techniques = new List<TechniqueInfo>();

        public Dictionary<string, SamplerStateInfo> SamplerStates = new Dictionary<string, SamplerStateInfo>();
	}
}
