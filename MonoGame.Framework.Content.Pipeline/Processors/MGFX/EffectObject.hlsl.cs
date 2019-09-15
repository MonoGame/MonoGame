using System;
using System.Linq;

namespace TwoMGFX
{
    partial class EffectObject
    {
        public static byte[] CompileHLSL(ShaderResult shaderResult, string shaderFunction, string shaderProfile, ref string errorsAndWarnings)
        {
            SharpDX.D3DCompiler.ShaderBytecode shaderByteCode;
            try
            {
                SharpDX.D3DCompiler.ShaderFlags shaderFlags = 0;

                // While we never allow preshaders, this flag is invalid for
                // the DX11 shader compiler which doesn't allow preshaders
                // in the first place.
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.NoPreshader;

                if (shaderResult.Profile == ShaderProfile.DirectX_11)
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.EnableBackwardsCompatibility;

                if (shaderResult.Debug)
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
                    shaderResult.FileContent,
                    shaderFunction,
                    shaderProfile,
                    shaderFlags,
                    0,
                    null,
                    null,
                    shaderResult.FilePath);

                // Store all the errors and warnings to log out later.
                errorsAndWarnings += result.Message;

                if (result.Bytecode == null)
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
