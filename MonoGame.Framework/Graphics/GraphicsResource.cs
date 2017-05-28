#region License
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
#endregion License

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class GraphicsResource : IDisposable
    {
        bool disposed;

        // The GraphicsDevice property should only be accessed in Dispose(bool) if the disposing
        // parameter is true. If disposing is false, the GraphicsDevice may or may not be
        // disposed yet.
        GraphicsDevice graphicsDevice;

        private WeakReference _selfReference;

        internal GraphicsResource()
        {
            
        }

        ~GraphicsResource()
        {
            // Pass false so the managed objects are not released
            Dispose(false);
        }

        /// <summary>
        /// Called before the device is reset. Allows graphics resources to 
        /// invalidate their state so they can be recreated after the device reset.
        /// Warning: This may be called after a call to Dispose() up until
        /// the resource is garbage collected.
        /// </summary>
        internal protected virtual void GraphicsDeviceResetting()
        {

        }

        public void Dispose()
        {
            // Dispose of managed objects as well
            Dispose(true);
            // Since we have been manually disposed, do not call the finalizer on this object
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The method that derived classes should override to implement disposing of managed and native resources.
        /// </summary>
        /// <param name="disposing">True if managed objects should be disposed.</param>
        /// <remarks>Native resources should always be released regardless of the value of the disposing parameter.</remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Release managed objects
                    // ...
                }

                // Release native objects
                // ...

                // Do not trigger the event if called from the finalizer
                if (disposing)
                    EventHelpers.Raise(this, Disposing, EventArgs.Empty);

                // Remove from the global list of graphics resources
                if (graphicsDevice != null)
                    graphicsDevice.RemoveResourceReference(_selfReference);

                _selfReference = null;
                graphicsDevice = null;
                disposed = true;
            }
        }

		public event EventHandler<EventArgs> Disposing;
		
		public GraphicsDevice GraphicsDevice
		{
			get
			{
				return graphicsDevice;
			}

            internal set
            {
                Debug.Assert(value != null);

                if (graphicsDevice == value)
                    return;

                // VertexDeclaration objects can be bound to multiple GraphicsDevice objects
                // during their lifetime. But only one GraphicsDevice should retain ownership.
                if (graphicsDevice != null)
                {
                    graphicsDevice.RemoveResourceReference(_selfReference);
                    _selfReference = null;
                }

                graphicsDevice = value;

                _selfReference = new WeakReference(this);
                graphicsDevice.AddResourceReference(_selfReference);
            }
		}
		
		public bool IsDisposed
		{
			get
			{
				return disposed;
			}
		}
		
		public string Name { get; set; }
		
		public Object Tag { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }
	}
}

