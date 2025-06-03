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
        /// The relative shader source file.
        /// </summary>
        public string SourceFile { get; private set; }

        /// <summary>
        /// The shader entrypoint function.
        /// </summary>
        public string Entrypoint { get; private set; }

        /// <summary>
        /// The shader stage.
        /// </summary>
        public ShaderStage Stage { get; private set; }

        /// <summary>
        /// The error logging from the shader compiler.
        /// </summary>
        public string Errors { get; private set; }

        /// <summary>
        /// The source code being compiled.
        /// </summary>
        public string SourceCode { get; private set; }

        private static string GetMessage(string sourceFile, string entrypoint, ShaderStage stage)
        {
            switch (stage)
            {
                case ShaderStage.Pixel:
                    return $"Failed to compile pixel shader {entrypoint}. See {sourceFile}.";

                default:
                case ShaderStage.Vertex:
                    return $"Failed to compile vertex shader {entrypoint}. See {sourceFile}.";
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceFile">The relative shader source file.</param>
        /// <param name="entrypoint">The shader entrypoint function.</param>
        /// <param name="stage">The shader stage.</param>
        /// <param name="errors">The error logging from the shader compiler.</param>
        /// <param name="sourceCode">The source code that was being compiled.</param>
        public ShaderCompilerException(string sourceFile, string entrypoint, ShaderStage stage, string errors, string sourceCode)
            : base(GetMessage(sourceFile, entrypoint, stage))
        {
            SourceFile = sourceFile;
            Entrypoint = entrypoint;
            Stage = stage;
            Errors = errors;
            SourceCode = sourceCode;
        }

        public override string ToString()
        {
            // Return all the data we got by default.
            return $"""
{base.ToString()}

---------------------------------------------------------------------------------------------------
Source File: {SourceFile}
Stage: {Stage}
Entrypoint: {Entrypoint}

{Errors}


Source Code:

{SourceCode}

---------------------------------------------------------------------------------------------------
""";                    
        }
    }
}
