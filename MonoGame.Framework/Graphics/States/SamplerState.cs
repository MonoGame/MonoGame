// #region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright © 2009 The XnaTouch Team
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

#if MONOMAC
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class SamplerState : GraphicsResource
	{
		static SamplerState () {
			AnisotropicClamp = new SamplerState () {
				Filter = TextureFilter.Anisotropic,

				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
			};
			
			AnisotropicWrap = new SamplerState () {
				Filter = TextureFilter.Anisotropic,

				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
			};
			
			LinearClamp = new SamplerState () {
				Filter = TextureFilter.Linear,

				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
			};
			
			LinearWrap = new SamplerState () {
				Filter = TextureFilter.Linear,

				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
			};
			
			PointClamp = new SamplerState () {
				Filter = TextureFilter.Point,

				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
			};
			
			PointWrap = new SamplerState () {
				Filter = TextureFilter.Point,

				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
			};
		}
		
		public static readonly SamplerState AnisotropicClamp;
		public static readonly SamplerState AnisotropicWrap;
		public static readonly SamplerState LinearClamp;
		public static readonly SamplerState LinearWrap;
		public static readonly SamplerState PointClamp;
		public static readonly SamplerState PointWrap;
		
		public TextureAddressMode AddressU { get; set; }
		public TextureAddressMode AddressV { get; set; }
		public TextureAddressMode AddressW { get; set; }
		public TextureFilter Filter { get; set; }
		
		public int MaxAnisotropy { get; set; }
		public int MaxMipLevel { get; set; }
		public float MipMapLevelOfDetailBias { get; set; }
		
		internal void Activate()
		{
			// Set up texture sample filtering.
			bool useMipmaps = MaxMipLevel > 0;
			switch(Filter)
			{
			case TextureFilter.Point:
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest));
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				break;
			case TextureFilter.Linear:
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				break;
			case TextureFilter.Anisotropic:
				// TODO: Requires EXT_texture_filter_anisotropic. Use linear filtering for now.
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				break;
			}

			// Set up texture addressing.
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GetWrapMode(AddressU));
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GetWrapMode(AddressV));
		}
		
		private int GetWrapMode(TextureAddressMode textureAddressMode)
		{
			switch(textureAddressMode)
			{
			case TextureAddressMode.Clamp:
				return (int)TextureWrapMode.ClampToEdge;
			case TextureAddressMode.Wrap:
				return (int)TextureWrapMode.Repeat;
			case TextureAddressMode.Mirror:
				return (int)TextureWrapMode.MirroredRepeat;
			default:
				throw new NotImplementedException("No support for " + textureAddressMode);
			}
		}
	}
}

