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
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;

#if PSM
using Sce.PlayStation.Core.Graphics;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect : GraphicsResource
    {
        public EffectParameterCollection Parameters { get; private set; }

        public EffectTechniqueCollection Techniques { get; private set; }

        public EffectTechnique CurrentTechnique { get; set; }
  
        internal ConstantBuffer[] ConstantBuffers { get; private set; }

        private Shader[] _shaders;

	    private readonly bool _isClone;

        internal Effect(GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
				throw new ArgumentNullException ("Graphics Device Cannot Be Null");

			this.GraphicsDevice = graphicsDevice;
		}
			
		protected Effect(Effect cloneSource)
            : this(cloneSource.GraphicsDevice)
		{
            _isClone = true;
            Clone(cloneSource);
		}

        public Effect (GraphicsDevice graphicsDevice, byte[] effectCode)
            : this(graphicsDevice)
		{
			// By default we currently cache all unique byte streams
			// and use cloning to populate the effect with parameters,
			// techniques, and passes.
			//
			// This means all the immutable types in an effect:
			//
			//  - Shaders
			//  - Annotations
			//  - Names
			//  - State Objects
			//
			// Are shared for every instance of an effect while the 
			// parameter values and constant buffers are copied.
			//
			// This might need to change slightly if/when we support
			// shared constant buffers as 'new' should return unique
			// effects without any shared instance state.
            

            // First look for it in the cache.
            //
            // TODO: We could generate a strong and unique signature
            // offline during content processing and just read it from 
            // the front of the effectCode instead of computing a fast
            // hash here at runtime.
            //
            var effectKey = MonoGame.Utilities.Hash.ComputeHash(effectCode);
            Effect cloneSource;
            if (!EffectCache.TryGetValue(effectKey, out cloneSource))
            {
                // Create one.
                cloneSource = new Effect(graphicsDevice);
                using (var stream = new MemoryStream(effectCode))
                using (var reader = new BinaryReader(stream))
                    cloneSource.ReadEffect(reader);

                // Cache the effect for later in its original unmodified state.
                EffectCache.Add(effectKey, cloneSource);
            }

            // Clone it.
            _isClone = true;
            Clone(cloneSource);
        }

        /// <summary>
        /// Clone the source into this existing object.
        /// </summary>
        /// <remarks>
        /// Note this is not overloaded in derived classes on purpose.  This is
        /// only a reason this exists is for caching effects.
        /// </remarks>
        /// <param name="cloneSource">The source effect to clone from.</param>
        private void Clone(Effect cloneSource)
        {
            Debug.Assert(_isClone, "Cannot clone into non-cloned effect!");

            // Copy the mutable members of the effect.
            Parameters = cloneSource.Parameters.Clone();
            Techniques = cloneSource.Techniques.Clone(this);

            // Make a copy of the immutable constant buffers.
            ConstantBuffers = new ConstantBuffer[cloneSource.ConstantBuffers.Length];
            for (var i = 0; i < cloneSource.ConstantBuffers.Length; i++)
                ConstantBuffers[i] = new ConstantBuffer(cloneSource.ConstantBuffers[i]);

            // Find and set the current technique.
            for (var i = 0; i < cloneSource.Techniques.Count; i++)
            {
                if (cloneSource.Techniques[i] == cloneSource.CurrentTechnique)
                {
                    CurrentTechnique = Techniques[i];
                    break;
                }
            }

            // Take a reference to the original shader list.
            _shaders = cloneSource._shaders;
        }

        /// <summary>
        /// Returns a deep copy of the effect where immutable types 
        /// are shared and mutable data is duplicated.
        /// </summary>
        /// <remarks>
        /// See "Cloning an Effect" in MSDN:
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ff476138(v=vs.85).aspx
        /// </remarks>
        /// <returns>The cloned effect.</returns>
		public virtual Effect Clone()
		{
            return new Effect(this);
		}

        protected internal virtual bool OnApply()
        {
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (!_isClone)
                    {
                        // Only the clone source can dispose the shaders.
                        if (_shaders != null)
                        {
                            foreach (var shader in _shaders)
                                shader.Dispose();
                        }
                    }

                    if (ConstantBuffers != null)
                    {
                        foreach (var buffer in ConstantBuffers)
                            buffer.Dispose();
                        ConstantBuffers = null;
                    }
                }
            }

            base.Dispose(disposing);
        }

        internal protected override void GraphicsDeviceResetting()
        {
            for (var i = 0; i < ConstantBuffers.Length; i++)
                ConstantBuffers[i].Clear();
        }

        #region Effect File Reader

        internal static byte[] LoadEffectResource(string name)
        {
#if WINRT
            var assembly = typeof(Effect).GetTypeInfo().Assembly;
#else
            var assembly = typeof(Effect).Assembly;
#endif
            var stream = assembly.GetManifestResourceStream(name);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// The MonoGame Effect file format header identifier.
        /// </summary>
        private const string MGFXHeader = "MGFX";

        /// <summary>
        /// The current MonoGame Effect file format versions
        /// used to detect old packaged content.
        /// </summary>
        /// <remarks>
        /// We should avoid supporting old versions for very long if at all 
        /// as users should be rebuilding content when packaging their game.
        /// </remarks>
        private const int MGFXVersion = 5;

#if !PSM

		private void ReadEffect (BinaryReader reader)
		{
			// Check the header to make sure the file and version is correct!
			var header = new string (reader.ReadChars (MGFXHeader.Length));
			var version = (int)reader.ReadByte ();
			if (header != MGFXHeader)
				throw new Exception ("The MGFX file is corrupt!");
            if (version != MGFXVersion)
                throw new Exception("Wrong MGFX file version!");

			var profile = reader.ReadByte ();
#if DIRECTX
            if (profile != 1)
#else
			if (profile != 0)
#endif
				throw new Exception("The MGFX effect is the wrong profile for this platform!");

			// TODO: Maybe we should be reading in a string 
			// table here to save some bytes in the file.

			// Read in all the constant buffers.
			var buffers = (int)reader.ReadByte ();
			ConstantBuffers = new ConstantBuffer[buffers];
			for (var c = 0; c < buffers; c++) 
            {
				
#if OPENGL
				string name = reader.ReadString ();               
#else
				string name = null;
#endif

				// Create the backing system memory buffer.
				var sizeInBytes = (int)reader.ReadInt16 ();

				// Read the parameter index values.
				var parameters = new int[reader.ReadByte ()];
				var offsets = new int[parameters.Length];
				for (var i = 0; i < parameters.Length; i++) 
                {
					parameters [i] = (int)reader.ReadByte ();
					offsets [i] = (int)reader.ReadUInt16 ();
				}

                var buffer = new ConstantBuffer(GraphicsDevice,
				                                sizeInBytes,
				                                parameters,
				                                offsets,
				                                name);
                ConstantBuffers[c] = buffer;
            }

            // Read in all the shader objects.
            var shaders = (int)reader.ReadByte();
            _shaders = new Shader[shaders];
            for (var s = 0; s < shaders; s++)
                _shaders[s] = new Shader(GraphicsDevice, reader);

            // Read in the parameters.
            Parameters = ReadParameters(reader);

            // Read the techniques.
            var techniqueCount = (int)reader.ReadByte();
            var techniques = new EffectTechnique[techniqueCount];
            for (var t = 0; t < techniqueCount; t++)
            {
                var name = reader.ReadString();

                var annotations = ReadAnnotations(reader);

                var passes = ReadPasses(reader, this, _shaders);

                techniques[t] = new EffectTechnique(this, name, passes, annotations);
            }

            Techniques = new EffectTechniqueCollection(techniques);
            CurrentTechnique = Techniques[0];
        }

        private static EffectAnnotationCollection ReadAnnotations(BinaryReader reader)
        {
            var count = (int)reader.ReadByte();
            if (count == 0)
                return EffectAnnotationCollection.Empty;

            var annotations = new EffectAnnotation[count];

            // TODO: Annotations are not implemented!

            return new EffectAnnotationCollection(annotations);
        }

        private static EffectPassCollection ReadPasses(BinaryReader reader, Effect effect, Shader[] shaders)
        {
            var count = (int)reader.ReadByte();
            var passes = new EffectPass[count];

            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                var annotations = ReadAnnotations(reader);

                // Get the vertex shader.
                Shader vertexShader = null;
                var shaderIndex = (int)reader.ReadByte();
                if (shaderIndex != 255)
                    vertexShader = shaders[shaderIndex];

                // Get the pixel shader.
                Shader pixelShader = null;
                shaderIndex = (int)reader.ReadByte();
                if (shaderIndex != 255)
                    pixelShader = shaders[shaderIndex];

				BlendState blend = null;
				DepthStencilState depth = null;
				RasterizerState raster = null;
				if (reader.ReadBoolean())
				{
					blend = new BlendState
					{
						AlphaBlendFunction = (BlendFunction)reader.ReadByte(),
						AlphaDestinationBlend = (Blend)reader.ReadByte(),
						AlphaSourceBlend = (Blend)reader.ReadByte(),
						BlendFactor = new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
						ColorBlendFunction = (BlendFunction)reader.ReadByte(),
						ColorDestinationBlend = (Blend)reader.ReadByte(),
						ColorSourceBlend = (Blend)reader.ReadByte(),
						ColorWriteChannels = (ColorWriteChannels)reader.ReadByte(),
						ColorWriteChannels1 = (ColorWriteChannels)reader.ReadByte(),
						ColorWriteChannels2 = (ColorWriteChannels)reader.ReadByte(),
						ColorWriteChannels3 = (ColorWriteChannels)reader.ReadByte(),
						MultiSampleMask = reader.ReadInt32(),
					};
				}
				if (reader.ReadBoolean())
				{
					depth = new DepthStencilState
					{
						CounterClockwiseStencilDepthBufferFail = (StencilOperation)reader.ReadByte(),
						CounterClockwiseStencilFail = (StencilOperation)reader.ReadByte(),
						CounterClockwiseStencilFunction = (CompareFunction)reader.ReadByte(),
						CounterClockwiseStencilPass = (StencilOperation)reader.ReadByte(),
						DepthBufferEnable = reader.ReadBoolean(),
						DepthBufferFunction = (CompareFunction)reader.ReadByte(),
						DepthBufferWriteEnable = reader.ReadBoolean(),
						ReferenceStencil = reader.ReadInt32(),
						StencilDepthBufferFail = (StencilOperation)reader.ReadByte(),
						StencilEnable = reader.ReadBoolean(),
						StencilFail = (StencilOperation)reader.ReadByte(),
						StencilFunction = (CompareFunction)reader.ReadByte(),
						StencilMask = reader.ReadInt32(),
						StencilPass = (StencilOperation)reader.ReadByte(),
						StencilWriteMask = reader.ReadInt32(),
						TwoSidedStencilMode = reader.ReadBoolean(),
					};
				}
				if (reader.ReadBoolean())
				{
					raster = new RasterizerState
					{
						CullMode = (CullMode)reader.ReadByte(),
						DepthBias = reader.ReadSingle(),
						FillMode = (FillMode)reader.ReadByte(),
						MultiSampleAntiAlias = reader.ReadBoolean(),
						ScissorTestEnable = reader.ReadBoolean(),
						SlopeScaleDepthBias = reader.ReadSingle(),
					};
				}

                passes[i] = new EffectPass(effect, name, vertexShader, pixelShader, blend, depth, raster, annotations);
			}

            return new EffectPassCollection(passes);
		}

		private static EffectParameterCollection ReadParameters(BinaryReader reader)
		{
			var count = (int)reader.ReadByte();			
            if (count == 0)
                return EffectParameterCollection.Empty;

            var parameters = new EffectParameter[count];
			for (var i = 0; i < count; i++)
			{
				var class_ = (EffectParameterClass)reader.ReadByte();				
                var type = (EffectParameterType)reader.ReadByte();
				var name = reader.ReadString();
				var semantic = reader.ReadString();
				var annotations = ReadAnnotations(reader);
				var rowCount = (int)reader.ReadByte();
				var columnCount = (int)reader.ReadByte();

				var elements = ReadParameters(reader);
				var structMembers = ReadParameters(reader);

				object data = null;
				if (elements.Count == 0 && structMembers.Count == 0)
				{
					switch (type)
					{						
                        case EffectParameterType.Bool:
                        case EffectParameterType.Int32:
#if DIRECTX
                            // Under DirectX we properly store integers and booleans
                            // in an integer type.
                            //
                            // MojoShader on the otherhand stores everything in float
                            // types which is why this code is disabled under OpenGL.
					        {
					            var buffer = new int[rowCount * columnCount];								
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = reader.ReadInt32();
                                data = buffer;
                                break;
					        }
#endif

						case EffectParameterType.Single:
							{
								var buffer = new float[rowCount * columnCount];
								for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = reader.ReadSingle();
                                data = buffer;
                                break;							
                            }
						case EffectParameterType.String:
							throw new NotImplementedException();
					}
                }

				parameters[i] = new EffectParameter(
					class_, type, name, rowCount, columnCount, semantic, 
					annotations, elements, structMembers, data);
			}

			return new EffectParameterCollection(parameters);
		}
#else //PSM
		internal void ReadEffect(BinaryReader reader)
		{
			var effectPass = new EffectPass(this, "Pass", null, null, BlendState.AlphaBlend, DepthStencilState.Default, RasterizerState.CullNone, EffectAnnotationCollection.Empty);
			effectPass._shaderProgram = new ShaderProgram(reader.ReadBytes((int)reader.BaseStream.Length));
			var shaderProgram = effectPass._shaderProgram;
            
            EffectParameter[] parametersArray = new EffectParameter[shaderProgram.UniformCount+4];
			for (int i = 0; i < shaderProgram.UniformCount; i++)
			{	
                parametersArray[i]= EffectParameterForUniform(shaderProgram, i);
			}
			
			#warning Hacks for BasicEffect as we don't have these parameters yet
            parametersArray[shaderProgram.UniformCount] = new EffectParameter(
                EffectParameterClass.Vector, EffectParameterType.Single, "SpecularColor",
                3, 1, "float3",EffectAnnotationCollection.Empty, EffectParameterCollection.Empty, EffectParameterCollection.Empty, new float[3]);
            parametersArray[shaderProgram.UniformCount+1] = new EffectParameter(
                EffectParameterClass.Scalar, EffectParameterType.Single, "SpecularPower",
                1, 1, "float",EffectAnnotationCollection.Empty, EffectParameterCollection.Empty, EffectParameterCollection.Empty, 0.0f);
            parametersArray[shaderProgram.UniformCount+2] = new EffectParameter(
                EffectParameterClass.Vector, EffectParameterType.Single, "FogVector",
                4, 1, "float4",EffectAnnotationCollection.Empty, EffectParameterCollection.Empty, EffectParameterCollection.Empty, new float[4]);
            parametersArray[shaderProgram.UniformCount+3] = new EffectParameter(
                EffectParameterClass.Vector, EffectParameterType.Single, "DiffuseColor",
                4, 1, "float4",EffectAnnotationCollection.Empty, EffectParameterCollection.Empty, EffectParameterCollection.Empty, new float[4]);

            Parameters = new EffectParameterCollection(parametersArray);
                       
            EffectPass []effectsPassArray = new EffectPass[1];
            effectsPassArray[0] = effectPass;
            var effectPassCollection = new EffectPassCollection(effectsPassArray);            
            
            EffectTechnique []effectTechniqueArray = new EffectTechnique[1]; 
            effectTechniqueArray[0] = new EffectTechnique(this, "Name", effectPassCollection, EffectAnnotationCollection.Empty);
            Techniques = new EffectTechniqueCollection(effectTechniqueArray);
            
            ConstantBuffers = new ConstantBuffer[0];            
            CurrentTechnique = Techniques[0];
        }
        
        internal EffectParameter EffectParameterForUniform(ShaderProgram shaderProgram, int index)
        {
            //var b = shaderProgram.GetUniformBinding(i);
            var name = shaderProgram.GetUniformName(index);
            //var s = shaderProgram.GetUniformSize(i);
            //var x = shaderProgram.GetUniformTexture(i);
            var type = shaderProgram.GetUniformType(index);
            
            //EffectParameter.Semantic => COLOR0 / POSITION0 etc
   
            //FIXME: bufferOffset in below lines is 0 but should probably be something else
            switch (type)
            {
            case ShaderUniformType.Float4x4:
                return new EffectParameter(
                    EffectParameterClass.Matrix, EffectParameterType.Single, name,
                    4, 4, "float4x4",EffectAnnotationCollection.Empty, EffectParameterCollection.Empty, EffectParameterCollection.Empty, new float[4 * 4]);
            case ShaderUniformType.Float4:
                return new EffectParameter(
                    EffectParameterClass.Vector, EffectParameterType.Single, name,
                    4, 1, "float4",EffectAnnotationCollection.Empty, EffectParameterCollection.Empty, EffectParameterCollection.Empty, new float[4]);
            case ShaderUniformType.Sampler2D:
                return new EffectParameter(
                    EffectParameterClass.Object, EffectParameterType.Texture2D, name,
                    1, 1, "texture2d",EffectAnnotationCollection.Empty, EffectParameterCollection.Empty, EffectParameterCollection.Empty, null);
            default:
                throw new Exception("Uniform Type " + type + " Not yet implemented (" + name + ")");
            }
        }
        
#endif
        #endregion // Effect File Reader


        #region Effect Cache        

        /// <summary>
        /// The cache of effects from unique byte streams.
        /// </summary>
        private static readonly Dictionary<int, Effect> EffectCache = new Dictionary<int, Effect>();

        internal static void FlushCache()
        {
            // Dispose all the cached effects.
            foreach (var effect in EffectCache)
                effect.Value.Dispose();

            // Clear the cache.
            EffectCache.Clear();
        }

        #endregion // Effect Cache

	}
}
