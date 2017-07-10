using System;
using System.Linq;

namespace TwoMGFX
{
    partial class EffectObject
    {
        public static byte[] CompileHLSL(ShaderInfo shaderInfo, string shaderFunction, string shaderProfile, ref string errorsAndWarnings)
        {
            SharpDX.D3DCompiler.ShaderBytecode shaderByteCode;
            try
            {
                SharpDX.D3DCompiler.ShaderFlags shaderFlags = 0;

                // While we never allow preshaders, this flag is invalid for
                // the DX11 shader compiler which doesn't allow preshaders
                // in the first place.
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.NoPreshader;

                if (shaderInfo.Profile == ShaderProfile.DirectX_11)
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.EnableBackwardsCompatibility;

                if (shaderInfo.Debug)
                {
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.SkipOptimization;
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.Debug;
                }
                else
                {
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.OptimizationLevel3;
                }

                // Compile the shader into bytecode.                
                var result = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                    shaderInfo.FileContent,
                    shaderFunction,
                    shaderProfile,
                    shaderFlags,
                    0,
                    null,
                    null,
                    shaderInfo.FilePath);

                // Apparently, there's a bug in SharpDX 4.0.1 in which invalid shaders
                // don't report HasErrors == true, but the bytecode appears to be null.                
                if (result.Bytecode == null)
                {
                    errorsAndWarnings += "ByteCode is NULL";
                    throw new ShaderCompilerException();
                }

                // Store all the errors and warnings to log out later.
                errorsAndWarnings += result.Message;

                if (result.HasErrors)
                    throw new ShaderCompilerException();
                
                shaderByteCode = result.Bytecode;
                //var source = shaderByteCode.Disassemble();
            }
            catch (SharpDX.CompilationException ex)
            {
                errorsAndWarnings += ex.Message;
                throw new ShaderCompilerException();
            }

            // Return a copy of the shader bytecode.
            return shaderByteCode.Data.ToArray();
        }
    }
}
