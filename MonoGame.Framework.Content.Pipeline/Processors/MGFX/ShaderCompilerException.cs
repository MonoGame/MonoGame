// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TwoMGFX
{
	public class ShaderCompilerException : Exception
	{
	    public ShaderCompilerException()
	        : base("A shader failed to compile!")
	    {	        
	    }
	}
}

