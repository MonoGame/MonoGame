// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

