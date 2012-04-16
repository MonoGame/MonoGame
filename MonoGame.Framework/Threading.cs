#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

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
using System.Collections.Generic;
using System.Threading;
#if ANDROID
using OpenTK.Graphics;
using OpenTK.Platform;
#endif
#if IPHONE
using MonoTouch.Foundation;
using MonoTouch.OpenGLES;
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework
{
    internal class Threading
    {
        static int mainThreadId;
#if IPHONE
		public static EAGLContext BackgroundContext;
#elif ANDROID
        public static GraphicsContext BackgroundContext;
        public static IWindowInfo WindowInfo;
#endif
        static Mutex contextMutex = new Mutex();

        static Threading()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// To be called prior to any GL calls that may happen on a secondary thread. If called on a A shared context
        /// </summary>
        internal static void Begin()
        {
            if (mainThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                contextMutex.WaitOne();
#if IPHONE
			    if (EAGLContext.CurrentContext != BackgroundContext)
				    EAGLContext.SetCurrentContext(BackgroundContext);
#elif ANDROID
                // FIXME: To be implemented
                throw new NotImplementedException("Threaded creation of GPU resources is not currently supported on Android");
#else
                // FIXME: To be implemented
                throw new NotImplementedException("Threaded creation of GPU resources is not currently supported on this platform");
#endif
            }
        }

        /// <summary>
        /// To be called after the GL calls that may happen on a secondary thread.
        /// </summary>
        internal static void End()
        {
            if (mainThreadId != Thread.CurrentThread.ManagedThreadId)
                contextMutex.ReleaseMutex();
        }
    }
}
