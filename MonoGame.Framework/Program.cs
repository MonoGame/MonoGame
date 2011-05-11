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

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Microsoft.Xna;

namespace Microsoft.Xna.Framework
{
	public class MonoGameProgram : UIApplicationDelegate 
	{
		private int ourTask;
		
		public Game MonoGameGame
        {
            get;
			set;
        }
		
		/// <summary>
		/// GC is required to allocate a memory buffer.
		/// </summary> 
		public override void ReceiveMemoryWarning(UIApplication application)
		{
			GC.Collect();
		} 
		
		public override void DidEnterBackground (UIApplication application)
		{
			ourTask = application.BeginBackgroundTask(delegate
			{    //this is the action that will run when the task expires
				if (ourTask != 0) //this check is because we want to avoid ending the same task twice
				{
				    application.EndBackgroundTask(ourTask); //end the task
				    ourTask = 0; //reset the id
				}
			});

		    //we start an asynchronous operation
		    //so that we make sure that DidEnterBackground
		    //executes normally
		    new System.Action(delegate
		    {
		        MonoGameGame.EnterBackground();
		
		        //Since we are in an asynchronous method,
		        //we have to make sure that EndBackgroundTask
		        //will run on the application's main thread
		        //or we might have unexpected behavior.
		        application.BeginInvokeOnMainThread(delegate
		        {
			            if (ourTask != 0) //same as above
			            {
			                application.EndBackgroundTask(ourTask);
			                ourTask = 0;
			            }
			       });
			}).BeginInvoke(null, null);	
		}
		
		public override void WillEnterForeground (UIApplication application)
		{
			ourTask = application.BeginBackgroundTask(delegate
			{    //this is the action that will run when the task expires
				if (ourTask != 0) //this check is because we want to avoid ending the same task twice
				{
				    application.EndBackgroundTask(ourTask); //end the task
				    ourTask = 0; //reset the id
				}
			});

		    //we start an asynchronous operation
		    //so that we make sure that DidEnterBackground
		    //executes normally
		    new System.Action(delegate
		    {
		        MonoGameGame.EnterForeground();
		
		        //Since we are in an asynchronous method,
		        //we have to make sure that EndBackgroundTask
		        //will run on the application's main thread
		        //or we might have unexpected behavior.
		        application.BeginInvokeOnMainThread(delegate
		        {
			            if (ourTask != 0) //same as above
			            {
			                application.EndBackgroundTask(ourTask);
			                ourTask = 0;
			            }
			       });
			}).BeginInvoke(null, null);	
		}
	}
}

