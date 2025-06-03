// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;


namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// This exception occurs when a shader compiled at runtime has errors.
    /// It should provide enough information to help diagnose the bug.
    /// </summary>
    [DataContract]
    public sealed class ShaderCompilerException : Exception
    {
        /// <summary>
        /// The error logging from the shader compiler.
        /// </summary>
        public string Errors { get; }

        /// <summary>
        /// The source code being compiled.
        /// </summary>
        public string SourceCode { get; }

        private static string GetMessage(ShaderStage stage)
        {
            switch (stage)
            {
                case ShaderStage.Pixel:
                    return "Failed to compile pixel shader.";

                default:
                case ShaderStage.Vertex:
                    return "Failed to compile vertex shader.";
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stage">The stage that failed to compile.</param>
        /// <param name="errors">The error logging from the shader compiler.</param>
        /// <param name="sourceCode">The source code that was being compiled.</param>
        public ShaderCompilerException(ShaderStage stage, string errors, string sourceCode)
            : base(GetMessage(stage))
        {
            Errors = errors;
            SourceCode = sourceCode;
        }

        public override string ToString()
        {
            // Return all the data we got by default.
            return $"""
{base.ToString()}

Shader Compiler Errors:
{Errors}

Shader Source Code:
{SourceCode}
""";                    
        }
    }
}
