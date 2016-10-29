// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace TwoMGFX
{
    public class ShaderInfo
	{
	    public ParseTree ParseTree { get; set; }
		public List<TechniqueInfo> Techniques = new List<TechniqueInfo>();
        public Dictionary<string, SamplerStateInfo> SamplerStates = new Dictionary<string, SamplerStateInfo>();
	    // Contains GLSL vertex shader inputs. Not used for HLSL shaders
	    public Dictionary<string, VsInputVariableInfo> VsInputVariables = new Dictionary<string, VsInputVariableInfo>();
	    // Contains GLSL entry point fuctions. Not used for HLSL shaders
	    public Dictionary<string, ParseNode> Functions = new Dictionary<string, ParseNode>();
	}
}
