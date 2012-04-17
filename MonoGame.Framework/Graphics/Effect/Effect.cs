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
using System.Linq;
using System.Reflection;

#if IPHONE || ANDROID
using OpenTK.Graphics.ES20;
using ShaderType = OpenTK.Graphics.ES20.All;
using ActiveUniformType = OpenTK.Graphics.ES20.All;
using ProgramParameter = OpenTK.Graphics.ES20.All;
using ShaderParameter = OpenTK.Graphics.ES20.All;
#elif MONOMAC
using MonoMac.OpenGL;
#elif !WINRT
using OpenTK.Graphics.OpenGL;

#endif


namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect : GraphicsResource
    {
        public EffectParameterCollection Parameters { get; set; }

        public EffectTechniqueCollection Techniques { get; set; }

        public EffectTechnique CurrentTechnique { get; set; }

        internal protected EffectParameter shaderIndexParam;

		DXEffectObject effectObject;

        internal static Dictionary<byte[], DXEffectObject> effectObjectCache =
			new Dictionary<byte[], DXEffectObject>(new ByteArrayComparer());

		//cache effect objects so we don't create a bunch of instances
		//of the same shader
		//(Some programs create heaps of instances of BasicEffect,
		// which was causing ridiculous memory usage)
		private class ByteArrayComparer : IEqualityComparer<byte[]> {
			public bool Equals(byte[] left, byte[] right) {
				if ( left == null || right == null ) {
					return left == right;
				}
				return left.SequenceEqual(right);
			}
			public int GetHashCode(byte[] key) {
				if (key == null)
					throw new ArgumentNullException("key");
				return key.Sum(b => b);
			}
		}

#if !WINRT
        internal int CurrentProgram = 0;
#endif

        protected Effect (GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
				throw new ArgumentNullException ("Graphics Device Cannot Be Null");

			this.graphicsDevice = graphicsDevice;
			Techniques = new EffectTechniqueCollection ();
			Parameters = new EffectParameterCollection();

            shaderIndexParam = new EffectParameter(ActiveUniformType.Int, "ShaderIndex");
		}
			
		protected Effect (Effect cloneSource)
		{
		}

		public Effect (GraphicsDevice graphicsDevice, byte[] effectCode)
            : this(graphicsDevice)
		{
			// Try getting a cached effect object.
			if (!effectObjectCache.TryGetValue(effectCode, out effectObject))
			{
                effectObject = DXEffectObject.FromCompiledD3DXEffect(effectCode);
				effectObjectCache.Add (effectCode, effectObject);
			}
	
			foreach (var parameter in effectObject.Parameters)
				Parameters._parameters.Add (new EffectParameter(parameter));

            foreach (var technique in effectObject.Techniques)
				Techniques._techniques.Add (new EffectTechnique(this, technique));

            CurrentTechnique = Techniques[0];			
		}

		internal static byte[] LoadEffectResource(string name)
		{
            var assembly = typeof(Effect).Assembly;

#if GLSL_EFFECTS
            name += "GLSL.bin";
#else
            name += ".bin";
#endif
            var stream = assembly.GetManifestResourceStream("Microsoft.Xna.Framework.Graphics.Effect." + name);
            using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

        internal virtual void Initialize()
        {
        }

		public virtual Effect Clone ()
		{
			throw new NotImplementedException();
		}

		public void End ()
		{
		}

		protected internal virtual void OnApply ()
		{
		}
	}
}
