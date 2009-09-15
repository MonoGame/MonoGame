#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using XnaTouch.Framework.Input;

namespace XnaTouch.Framework
{
	public class IphoneWindow : GameWindow
	{
		private readonly Rectangle clientBounds;
		internal Game game;
		
		public IphoneWindow () : base(UIScreen.MainScreen.Bounds)
		{
			RectangleF rect = UIScreen.MainScreen.Bounds;
			clientBounds = new Rectangle(0,0,(int) rect.Width,(int) rect.Height);
			
			// Enable multi-touch
			MultipleTouchEnabled = true;
		}
		
		#region UIVIew Methods
		
		public override void LayoutSubviews ()
		{
			game.GraphicsDevice.Reset();
		}
		
		[Export ("layerClass")]
		public static Class LayerClass ()
		{
			return new Class (typeof (CAEAGLLayer));
		}
		
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			GamePad.Instance.TouchesBegan(touches,evt);
			base.TouchesBegan (touches, evt);
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			GamePad.Instance.TouchesEnded(touches,evt);
			base.TouchesEnded (touches, evt);
		}
		
		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			GamePad.Instance.TouchesMoved(touches,evt);
			base.TouchesMoved (touches, evt);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			GamePad.Instance.TouchesCancelled(touches,evt);
			base.TouchesCancelled (touches, evt);
		}

		#endregion
				
		#region GameWindow Methods
		
		public override string ScreenDeviceName 
		{
			get 
			{
				throw new System.NotImplementedException ();
			}
		}

		public override Rectangle ClientBounds 
		{
			get 
			{
				return clientBounds;
			}
		}
		
		public override bool AllowUserResizing 
		{
			get 
			{
				return false;
			}
			set {
				throw new System.NotSupportedException ();
			}
		}
		
		protected override void SetTitle (string title)
		{
			throw new System.NotImplementedException ();
		}

		
		#endregion
	}
	
}
