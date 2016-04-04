// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;

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
            {
                throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            }
            this.GraphicsDevice = graphicsDevice;
		}
			
		protected Effect(Effect cloneSource)
            : this(cloneSource.GraphicsDevice)
		{
            _isClone = true;
            Clone(cloneSource);
		}

        public Effect(GraphicsDevice graphicsDevice, byte[] effectCode)
            : this(graphicsDevice, effectCode, 0, effectCode.Length)
        {
        }


	    public Effect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count)
	        : this(graphicsDevice, new BinaryEffectReader(effectCode, index, count))
	    {

	    }

        public Effect(GraphicsDevice graphicsDevice, IEffectReader effectReader) : this(graphicsDevice)
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
 
            //Read the header
			var effectKey = effectReader.GetEffectKey();

            // First look for it in the cache.
            //
            Effect cloneSource;
            if (!graphicsDevice.EffectCache.TryGetValue(effectKey, out cloneSource))
            {
                // Create one.
                cloneSource = new Effect(graphicsDevice);
                cloneSource.ReadEffect(effectReader);

                // Cache the effect for later in its original unmodified state.
                graphicsDevice.EffectCache.Add(effectKey, cloneSource);
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

		private void ReadEffect (IEffectReader reader)
		{
			// TODO: Maybe we should be reading in a string 
			// table here to save some bytes in the file.

			// Read in all the constant buffers.
		    var buffers = reader.GetConstantBufferCount();
			ConstantBuffers = new ConstantBuffer[buffers];
			for (var c = 0; c < buffers; c++) 
            {
                string name = reader.GetConstantBufferName(c);

				// Create the backing system memory buffer.
                var sizeInBytes = reader.GetConstantBufferSize(c);

				// Read the parameter index values.
				var parameters = new int[reader.GetConstantBufferParameterCount(c)];
				var offsets = new int[parameters.Length];
				for (var i = 0; i < parameters.Length; i++) 
                {
					parameters [i] = reader.GetConstantBufferParameterValue(c, i);
                    offsets[i] = reader.GetConstantBufferParameterOffset(c, i);
                }

                var buffer = new ConstantBuffer(GraphicsDevice,
				                                sizeInBytes,
				                                parameters,
				                                offsets,
				                                name);
                ConstantBuffers[c] = buffer;
            }

            // Read in all the shader objects.
		    var shaders = reader.GetShaderCount();
            _shaders = new Shader[shaders];
            for (var s = 0; s < shaders; s++)
                _shaders[s] = new Shader(GraphicsDevice, reader.GetShaderReader(s));

            // Read in the parameters.
            Parameters = ReadParameters(reader, null);

            // Read the techniques.
		    var techniqueCount = reader.GetTechniqueCount();
            var techniques = new EffectTechnique[techniqueCount];
            for (var t = 0; t < techniqueCount; t++)
            {
                var name = reader.GetTechniqueName(t);

                var annotations = ReadTechniqueAnnotations(reader, t);

                var passes = ReadPasses(reader, this, _shaders, t);

                techniques[t] = new EffectTechnique(this, name, passes, annotations);
            }

            Techniques = new EffectTechniqueCollection(techniques);
            CurrentTechnique = Techniques[0];
        }

        private static EffectAnnotationCollection ReadParameterAnnotations(IEffectReader reader, object parameterContext, int parameterIndex)
        {
            var count = reader.GetParameterAnnotationCount(parameterContext, parameterIndex);
            if (count == 0)
                return EffectAnnotationCollection.Empty;

            var annotations = new EffectAnnotation[count];

            // TODO: Annotations are not implemented!

            return new EffectAnnotationCollection(annotations);
        }

        private static EffectAnnotationCollection ReadTechniqueAnnotations(IEffectReader reader, int techniqueIndex)
        {
            var count = reader.GetTechniqueAnnotationCount(techniqueIndex);
            if (count == 0)
                return EffectAnnotationCollection.Empty;

            var annotations = new EffectAnnotation[count];

            // TODO: Annotations are not implemented!

            return new EffectAnnotationCollection(annotations);
        }

        private static EffectAnnotationCollection ReadPassAnnotations(IEffectReader reader, int techniqueIndex, int passIndex)
        {
            var count = reader.GetPassAnnotationCount(techniqueIndex, passIndex);
            if (count == 0)
                return EffectAnnotationCollection.Empty;

            var annotations = new EffectAnnotation[count];

            // TODO: Annotations are not implemented!

            return new EffectAnnotationCollection(annotations);
        }

        private static EffectPassCollection ReadPasses(IEffectReader reader, Effect effect, Shader[] shaders, int techniqueIndex)
        {
            var count = reader.GetPassCount(techniqueIndex);
            var passes = new EffectPass[count];

            for (var i = 0; i < count; i++)
            {
                var name = reader.GetPassName(techniqueIndex, i);
                var annotations = ReadPassAnnotations(reader, techniqueIndex, i);

                // Get the vertex shader.
                Shader vertexShader = null;
                var shaderIndex = reader.GetPassVertexShaderIndex(techniqueIndex, i);
                if (shaderIndex != null)
                    vertexShader = shaders[shaderIndex.Value];

                // Get the pixel shader.
                Shader pixelShader = null;
                shaderIndex = reader.GetPassPixelShaderIndex(techniqueIndex, i);
                if (shaderIndex != null)
                    pixelShader = shaders[shaderIndex.Value];

				BlendState blend = reader.GetPassBlendState(techniqueIndex, i);
				DepthStencilState depth = reader.GetPassDepthStencilState(techniqueIndex, i);
				RasterizerState raster = reader.GetPassRasterizerState(techniqueIndex, i);

                passes[i] = new EffectPass(effect, name, vertexShader, pixelShader, blend, depth, raster, annotations);
			}

            return new EffectPassCollection(passes);
		}

		private static EffectParameterCollection ReadParameters(IEffectReader reader, object context)
		{
		    var count = reader.GetParameterCount(context);
            if (count == 0)
                return EffectParameterCollection.Empty;

            var parameters = new EffectParameter[count];
			for (var i = 0; i < count; i++)
			{
			    var class_ = reader.GetParameterClass(context, i);
			    var type = reader.GetParameterType(context, i);
			    var name = reader.GetParameterName(context, i);
			    var semantic = reader.GetParameterSemantic(context, i);
				var annotations = ReadParameterAnnotations(reader, context, i);
			    var rowCount = reader.GetParameterRowCount(context, i);
			    var columnCount = reader.GetParameterColumnCount(context, i);
			    var elementsContext = reader.GetParameterElementsContext(context, i);
                var structMembersContext = reader.GetParameterStructMembersContext(context, i);

                var elements = ReadParameters(reader, elementsContext);
				var structMembers = ReadParameters(reader, structMembersContext);

				object data = null;
				if (elements.Count == 0 && structMembers.Count == 0)
				{
					switch (type)
					{						
                        case EffectParameterType.Bool:
                        case EffectParameterType.Int32:
#if !OPENGL
                            // Under most platforms we properly store integers and 
                            // booleans in an integer type.
                            //
                            // MojoShader on the otherhand stores everything in float
                            // types which is why this code is disabled under OpenGL.
					        {
					            var buffer = new int[rowCount * columnCount];								
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = reader.GetParameterInt32Buffer(context, i, j);
                                data = buffer;
                                break;
					        }
#endif

						case EffectParameterType.Single:
							{
								var buffer = new float[rowCount * columnCount];
							    for (var j = 0; j < buffer.Length; j++)
							        buffer[j] = reader.GetParameterFloatBuffer(context, i, j);
                                data = buffer;
                                break;							
                            }

                        case EffectParameterType.String:
                            // TODO: We have not investigated what a string
                            // type should do in the parameter list.  Till then
                            // throw to let the user know.
							throw new NotSupportedException();

                        default:
                            // NOTE: We skip over all other types as they 
                            // don't get added to the constant buffer.
					        break;
					}
                }

				parameters[i] = new EffectParameter(
					class_, type, name, rowCount, columnCount, semantic, 
					annotations, elements, structMembers, data);
			}

			return new EffectParameterCollection(parameters);
		}
        #endregion // Effect File Reader
	}
}
