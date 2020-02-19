using System;
using System.Collections.Generic;
using MonoGame.Effect.TPGParser;

namespace MonoGame.Effect
{
    internal partial class ShaderData
    {
        public static ShaderData CreatePSSL(byte[] byteCode, bool isVertexShader, List<ConstantBufferData> cbuffers, int sharedIndex, Dictionary<string, SamplerStateInfo> samplerStates, bool debug)
        {
            // This is only part of the private PS4 repository.
            throw new NotImplementedException();
        }
    }
}