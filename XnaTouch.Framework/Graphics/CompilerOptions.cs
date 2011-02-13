// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public enum CompilerOptions
	{
		AvoidFlowControl	, // Hints to the compiler to avoid using flow-control instructions.
		Debug, //	Inserts debug file name, line numbers, and type and symbol information during shader compile.
		ForcePixelShaderSoftwareNoOptimizations, //	Forces the compiler to compile against the next highest available software target for pixel shaders. This flag also turns optimizations off and debugging on.
		ForceVertexShaderSoftwareNoOptimizations, //	Forces the compiler to compile against the next highest available software target for vertex shaders. This flag also turns optimizations off and debugging on.
		None	, // No options specified.
		NoPreShader, //	Disables preshaders. The compiler will not pull out static expressions for evaluation on the host CPU. Additionally, the compiler will not loft any expressions when compiling stand-alone functions.
		NotCloneable, //	Indicates the effect will be non-cloneable and will not contain any shader binary data. Setting this flag reduces effect memory usage by about 50 percent because it eliminates the need for the effect system to keep a copy of the shaders in memory.
		PackMatrixColumnMajor, //	Unless explicitly specified, matrices will be packed in column major order (each vector will be in a single column) when passed to and from the shader. This is generally more efficient because it allows vector-matrix multiplication to be performed using a series of dot products.
		PackMatrixRowMajor, //Unless explicitly specified, matrices will be packed in row major order (each vector will be in a single row) when passed to or from the shader.
		PartialPrecision	, // Forces all computations in the resulting shader to occur at partial precision. This may result in faster evaluation of shaders on some hardware.
		PreferFlowControl, //	Hints to the compiler to prefer using flow-control instructions.
		SkipOptimization	, // Instructs the compiler to skip optimization steps during code generation. Unless you are trying to isolate a problem in your code and you suspect the compiler, using this option is not recommended.
		SkipValidation, //	Do not validate the generated code against known capabilities and constraints. This option is recommended only when compiling shaders that are known to work (that is, shaders that have compiled before without this option). Shaders are always validated by the runtime before they are set to the device.
	}
}
