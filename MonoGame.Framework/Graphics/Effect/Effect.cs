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

//For laoding from resources
using System.Reflection;


namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect : GraphicsResource
    {
        public EffectParameterCollection Parameters { get; set; }
		internal List<EffectParameter> _textureMappings = new List<EffectParameter>();

		DXEffectObject effectObject;
		
		protected Effect (GraphicsDevice graphicsDevice)
		{
#if ES11
			throw new NotSupportedException("Programmable shaders unavailable in OpenGL ES 1.1");
#endif
			if (graphicsDevice == null) {
				throw new ArgumentNullException ("Graphics Device Cannot Be Null");
			}
			this.graphicsDevice = graphicsDevice;
			Techniques = new EffectTechniqueCollection ();
			Parameters = new EffectParameterCollection();
		}

        public EffectTechniqueCollection Techniques { get; set; }
		
		
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
		private static Dictionary<byte[], DXEffectObject> effectObjectCache =
			new Dictionary<byte[], DXEffectObject>(new ByteArrayComparer());
		

		protected Effect (Effect cloneSource)
		{

		}

		public Effect (
			GraphicsDevice graphicsDevice,
			byte[] effectCode)
		{
#if ES11
			throw new NotSupportedException("Programmable shaders unavailable in OpenGL ES 1.1");
#endif

			if (graphicsDevice == null) {
				throw new ArgumentNullException ("Graphics Device Cannot Be Null");
			}
			this.graphicsDevice = graphicsDevice;
			
			//try getting a cached effect object
			if (!effectObjectCache.TryGetValue(effectCode, out effectObject))
			{
				effectObject = new DXEffectObject(effectCode);
				effectObjectCache.Add (effectCode, effectObject);
			}
			
			Parameters = new EffectParameterCollection();
			foreach (DXEffectObject.d3dx_parameter parameter in effectObject.parameter_handles) {
				Parameters._parameters.Add (new EffectParameter(parameter));
			}
			
			Techniques = new EffectTechniqueCollection();
			foreach (DXEffectObject.d3dx_technique technique in effectObject.technique_handles) {
				Techniques._techniques.Add (new EffectTechnique(this, technique));
			}
			
			CurrentTechnique = Techniques[0];
			
		}

		public virtual Effect Clone ()
		{
			throw new NotImplementedException();
		}

		public void End ()
		{
		}

		public EffectTechnique CurrentTechnique { 
			get;
			set; 
		}

		protected internal virtual void OnApply ()
		{

		}

		internal static byte[] LoadEffectResource(string name)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			var stream = assembly.GetManifestResourceStream("Microsoft.Xna.Framework.Graphics.Effect."+name+".bin");
			using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

	}
}
