using System;
using Microsoft.SPOT;

namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : IDisposable
    {
        internal Bitmap Bitmap
        {
            get;
            set;
        }

        internal Texture2D()
        {
        }

        public int Width
        {
            get
            {
                return Bitmap.Width;
            }
        }

        public int Height
        {
            get
            {
                return Bitmap.Height;
            }
        }

        #region IDisposable Members

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        ///     Default initialization for a <see cref="System.Boolean"/> is <c>false</c>.
        /// </remarks>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        ///     Implementation of Dispose according to .NET Framework Design Guidelines.
        /// </summary>
        /// <remarks>
        ///     Do not make this method virtual. A derived class should not be able to override this method.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);

            /* 
             * This object will be cleaned up by the Dispose method. Therefore,
             * you should call GC.SupressFinalize to take this object off the 
             * finalization queue  and prevent finalization code for this 
             * object from executing a second time.
             */

            // Always use SuppressFinalize() in case a subclass of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Overloaded Implementation of Dispose.
        /// </summary>
        /// <param name="isDisposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        /// <remarks>
        ///     <see cref="Shmuelie.Avalon.Framework.ControlPrimitive.Dispose(bool)"/> executes in two distinct scenarios.
        ///     <list type="bulleted">
        ///         <item>
        ///             If <paramref name="isDisposing"/> equals true, the method has been called directly or indirectly by a user's code. Managed and unmanaged resources can be disposed.
        ///         </item>
        ///         <item>
        ///             If <paramref name="isDisposing"/> equals <c>false</c>, the method has been called by the runtime from inside the finalizer and you should not reference other objects. Only unmanaged resources can be disposed.
        ///         </item>
        ///     </list>
        /// </remarks>
        protected virtual void Dispose(bool isDisposing)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    /*
                     * Explicitly set root references to null to expressly tell
                     * the GarbageCollector that the resources have been
                     * disposed of and its ok to release the memory 
                     */
                    if (isDisposing)
                    {
                        // Release all managed resources here
                        if (Bitmap != null)
                        {
                            Bitmap.Dispose();
                            Bitmap = null;
                        }
                    }
                }
            }
            finally
            {
                this.IsDisposed = true;
            }
        }

        #endregion
    }
}
