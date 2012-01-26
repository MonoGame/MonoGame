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
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Graphics.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public class Effect : GraphicsResource
    {
        public EffectParameterCollection Parameters { get; set; }

        public EffectTechniqueCollection Techniques { get; set; }

        internal List<EffectParameter> _textureMappings = new List<EffectParameter>();

        private int fragment_handle;
        private int vertex_handle;
        private bool fragment;
        private bool vertex;
        private string filename;

        internal List<int> vertexShaders = new List<int>();
        internal List<int> fragmentShaders = new List<int>();

        public Effect(
            GraphicsDevice graphicsDevice,
            byte[] effectCode,
            CompilerOptions options,
            EffectPool pool)
        {

            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
            this.graphicsDevice = graphicsDevice;

            if (pool == null)
            {
                return;
                // TODO throw new ArgumentNullException("Effect Pool Cannot Be Null");
            }

            int fragmentblocklength = BitConverter.ToInt32(effectCode, 0);

            int vertexblocklength = BitConverter.ToInt32(effectCode, fragmentblocklength + 4);

            if (fragmentblocklength != 0)
            {
                fragment_handle = GL.CreateShader(ShaderType.FragmentShader);
                fragment = true;
            }

            if (vertexblocklength != 0)
            {
                vertex_handle = GL.CreateShader(ShaderType.VertexShader);
                vertex = true;
            }

            if (fragment)
            {
                string[] fragmentstring = new string[1] { Encoding.UTF8.GetString(effectCode, 4, fragmentblocklength) };
                //int[] fragmentLength = new int[1] { fragmentstring[0].Length };
                int fragmentLength = fragmentstring[0].Length;
                GL.ShaderSource(fragment_handle, 1, fragmentstring, ref fragmentLength);
            }

            if (vertex)
            {
                string[] vertexstring = new string[1] { Encoding.UTF8.GetString(effectCode, fragmentblocklength + 8, vertexblocklength) };
                // int[] vertexLength = new int[1] { vertexstring[0].Length };
                int vertexLength = vertexstring[0].Length;
                GL.ShaderSource(vertex_handle, 1, vertexstring, ref vertexLength);
            }

            int compiled = 0;

            if (fragment)
            {
                GL.CompileShader(fragment_handle);

                GL.GetShader(fragment_handle, ShaderParameter.CompileStatus, out compiled);
                if (compiled == (int)All.False)
                {
                    Console.Write("Fragment Compilation Failed!");
                }
            }

            if (vertex)
            {
                GL.CompileShader(vertex_handle);
                GL.GetShader(vertex_handle, ShaderParameter.CompileStatus, out compiled);
                if (compiled == (int)All.False)
                {
                    Console.Write("Vertex Compilation Failed!");
                }
            }

        }

        protected Effect(GraphicsDevice graphicsDevice, Effect cloneSource)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
            this.graphicsDevice = graphicsDevice;
        }

        protected Effect(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
            this.graphicsDevice = graphicsDevice;
            Techniques = new EffectTechniqueCollection();
            Parameters = new EffectParameterCollection();
        }

        internal Effect(GraphicsDevice aGraphicsDevice, string aFileName)
            : this(aGraphicsDevice)
        {
            StreamReader streamReader = new StreamReader(aFileName);
            string text = streamReader.ReadToEnd();
            streamReader.Close();

            if (aFileName.ToLower().Contains("fsh"))
            {
                CreateFragmentShaderFromSource(text);
            }
            else
            {
                CreateVertexShaderFromSource(text);
            }

            DefineTechnique("Technique1", "Pass1", 0, 0);
            CurrentTechnique = Techniques["Technique1"];
        }

        protected Effect(Effect cloneSource)
        {

        }

        public Effect(
            GraphicsDevice graphicsDevice,
            byte[] effectCode)
        {

            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
            this.graphicsDevice = graphicsDevice;

            int fragmentblocklength = BitConverter.ToInt32(effectCode, 0);

            int vertexblocklength = BitConverter.ToInt32(effectCode, fragmentblocklength + 4);

            if (fragmentblocklength != 0)
            {
                fragment_handle = GL.CreateShader(ShaderType.FragmentShader);
                fragment = true;
            }

            if (vertexblocklength != 0)
            {
                vertex_handle = GL.CreateShader(ShaderType.VertexShader);
                vertex = true;
            }

            if (fragment)
            {
                string[] fragmentstring = new string[1] { Encoding.UTF8.GetString(effectCode, 4, fragmentblocklength) };
                //int[] fragmentLength = new int[1] { fragmentstring[0].Length };
                int fragmentLength = fragmentstring[0].Length;
                GL.ShaderSource(fragment_handle, 1, fragmentstring, ref fragmentLength);
            }

            if (vertex)
            {
                string[] vertexstring = new string[1] { Encoding.UTF8.GetString(effectCode, fragmentblocklength + 8, vertexblocklength) };
                // int[] vertexLength = new int[1] { vertexstring[0].Length };
                int vertexLength = vertexstring[0].Length;
                GL.ShaderSource(vertex_handle, 1, vertexstring, ref vertexLength);
            }

            int compiled = 0;

            if (fragment)
            {
                GL.CompileShader(fragment_handle);

                GL.GetShader(fragment_handle, ShaderParameter.CompileStatus, out compiled);
                if (compiled == (int)All.False)
                {
                    Console.Write("Fragment Compilation Failed!");
                }
            }

            if (vertex)
            {
                GL.CompileShader(vertex_handle);
                GL.GetShader(vertex_handle, ShaderParameter.CompileStatus, out compiled);
                if (compiled == (int)All.False)
                {
                    Console.Write("Vertex Compilation Failed!");
                }
            }

        }

        public void Begin()
        {
        }

        public void Begin(SaveStateMode saveStateMode)
        {

        }

        public virtual Effect Clone(GraphicsDevice device)
        {
            Effect f = new Effect(graphicsDevice, this);
            return f;
        }

        public virtual Effect Clone()
        {
            return Clone(graphicsDevice);
        }

        public void End()
        {
        }

        internal static string Normalize(string FileName)
        {
            if (File.Exists(FileName))
                return FileName;

            // Check the file extension
            if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
            {
                return null;
            }

            // Concat the file name with valid extensions
            if (File.Exists(FileName + ".fsh"))
                return FileName + ".fsh";
            if (File.Exists(FileName + ".vsh"))
                return FileName + ".vsh";

            return null;
        }

        public EffectTechnique CurrentTechnique
        {
            get;
            set;
        }

        internal virtual void Apply()
        {
            //if (graphicsDevice.Textures.)
            Techniques[0].Passes[0].Apply();
        }

        protected internal virtual void OnApply()
        {

        }

        protected void CreateVertexShaderFromSource(string source)
        {
            int shader = GL.CreateShader(ShaderType.VertexShader);
            // Attach the loaded source string to the shader object
            GL.ShaderSource(shader, source);
            // Compile the shader
            GL.CompileShader(shader);

            vertexShaders.Add(shader);
        }


        protected void CreateFragmentShaderFromSource(string source)
        {
            int shader = GL.CreateShader(ShaderType.FragmentShader);
            // Attach the loaded source string to the shader object
            GL.ShaderSource(shader, source);
            // Compile the shader
            GL.CompileShader(shader);

            fragmentShaders.Add(shader);
        }

        protected void DefineTechnique(string techniqueName, string passName, int vertexIndex, int fragmentIndex)
        {
            EffectTechnique tech = new EffectTechnique(this);
            tech.Name = techniqueName;
            EffectPass pass = new EffectPass(tech);
            pass.Name = passName;
            pass.VertexIndex = vertexIndex;
            pass.FragmentIndex = fragmentIndex;
            pass.ApplyPass();
            tech.Passes._passes.Add(pass);
            Techniques._techniques.Add(tech);
            LogShaderParameters(String.Format("Technique {0} - Pass {1} :", tech.Name, pass.Name), pass.shaderProgram);

        }

        // Output the log of an object
        private void LogShaderParameters(string whichObj, int obj)
        {
            int actUnis = 0;
            Parameters._parameters.Clear();

            GL.GetProgram(obj, ProgramParameter.ActiveUniforms, out actUnis);

            Console.WriteLine("{0} {1}", whichObj, actUnis);

            int size;
            ActiveUniformType type;
            StringBuilder name = new StringBuilder(100);
            int length;

            for (int x = 0; x < actUnis; x++)
            {
                name = new StringBuilder(100);
                GL.GetActiveUniform(obj, x, 100, out length, out size, out type, name);
                Console.WriteLine("{0}: {1} {2} {3}", x, name, type, length);
                EffectParameter efp = new EffectParameter(this, name.ToString(), x, -1, -1, type.ToString(), length);
                Parameters._parameters.Add(efp.Name, efp);
                if (efp.ParameterType == EffectParameterType.Texture2D)
                {
                    _textureMappings.Add(efp);
                }
            }

        }
    }
}
