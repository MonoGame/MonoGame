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

using MonoMac.OpenGL;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
	public sealed class RenderState
	{

		private bool _alphaBlendEnable;
		private bool _alphaTestEnable;
		private Blend _destinationBlend = Blend.One;
		private Blend _sourceBlend = Blend.One;

		public bool AlphaBlendEnable { 
			get {
				return _alphaBlendEnable;
			}
			set {
				if (_alphaBlendEnable != value) {
					_alphaBlendEnable = value;

					if (_alphaBlendEnable) {
						GL.Enable (EnableCap.Blend);
					} else {
						GL.Disable (EnableCap.Blend);
					}
				}
			}
		}

		BlendFunction alphaBlendOperation;

		public BlendFunction AlphaBlendOperation { 
			get { return alphaBlendOperation; } 
			set { alphaBlendOperation = value; }
		}

		CompareFunction _alphaFunction = CompareFunction.Always;

		public CompareFunction AlphaFunction {
			get {
				return _alphaFunction;
			}

			set {
				_alphaFunction = value;
				AlphaFunction af = MonoMac.OpenGL.AlphaFunction.Always;
				switch (value) {
				case CompareFunction.Always:
					af = MonoMac.OpenGL.AlphaFunction.Always;
					break;
				case CompareFunction.Equal:
					af = MonoMac.OpenGL.AlphaFunction.Equal;
					break;
				case CompareFunction.Greater:
					af = MonoMac.OpenGL.AlphaFunction.Greater;
					break;
				case CompareFunction.GreaterEqual:
					af = MonoMac.OpenGL.AlphaFunction.Gequal;
					break;
				case CompareFunction.Less:
					af = MonoMac.OpenGL.AlphaFunction.Less;
					break;
				case CompareFunction.LessEqual:
					af = MonoMac.OpenGL.AlphaFunction.Lequal;
					break;
				case CompareFunction.Never:
					af = MonoMac.OpenGL.AlphaFunction.Never;
					break;
				case CompareFunction.NotEqual:
					af = MonoMac.OpenGL.AlphaFunction.Notequal;
					break;
				}

				GL.AlphaFunc (af, _referenceAlpha);
			}

		}

		public bool AlphaTestEnable { 
			get {
				return _alphaTestEnable;
			}
			set {
				if (_alphaTestEnable != value) {
					_alphaTestEnable = value;

					if (_alphaTestEnable) {
						GL.Enable (EnableCap.AlphaTest);

					} else {
						GL.Disable (EnableCap.AlphaTest);
					}
				}
			}
		}

		bool _depthBufferEnable = true;
		public bool DepthBufferEnable {
			get {
				return _depthBufferEnable;
			}
			
			set {
				if (_depthBufferEnable != value) {
					_depthBufferEnable = value;

					if (_depthBufferEnable) {
						GL.Enable (EnableCap.DepthTest);

					} else {
						GL.Disable (EnableCap.DepthTest);
					}
				}				
			}
			
		}
		public Blend DestinationBlend { 
			get {
				return _destinationBlend;
			} 
			set {
				if (_destinationBlend != value) {
					_destinationBlend = value;
				}
			}
		}

		public Blend SourceBlend { 
			get {
				return _sourceBlend;
			} 
			set {
				if (_sourceBlend != value) {
					_sourceBlend = value;
				}
			}
		}

		float _pointSize = 64;

		public float PointSize { 
			get {

				return _pointSize;
			}
			set {
				if (_pointSize != value) {
					_pointSize = Math.Max (Math.Min (value, _pointSizeMax), PointSizeMin);
					GL.PointSize (_pointSize);
				}

			}
		}

		float _pointSizeMax = 64;

		public float PointSizeMax {

			get {
				return _pointSizeMax;
			}
			set {
				_pointSizeMax = value;
			}
		}

		float _pointSizeMin = 1;

		public float PointSizeMin {

			get {
				return _pointSizeMin;
			}
			set {
				_pointSizeMin = value;
			}
		}

		bool _pointSpriteEnable = false;
		public bool PointSpriteEnable {
			get {
				return _pointSpriteEnable;
			}
			
			set {
				_pointSpriteEnable = value;
			}
			
		}
		int _referenceAlpha = 0;

		public int ReferenceAlpha {
			get {
				return _referenceAlpha;
			}

			set {
				_referenceAlpha = value;
			}

		}

		bool _scissorTestEnable = false;

		public bool ScissorTestEnable { 
			get {
				return _scissorTestEnable;
			}
			set {
				if (_scissorTestEnable != value) {
					_scissorTestEnable = value;
					if (!_scissorTestEnable) {
						GL.Disable (EnableCap.ScissorTest);	
					}
				}
			}
		}


	}
}
