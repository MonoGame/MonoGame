using System;

namespace Microsoft.Xna.Framework.Utilities
{
    internal class ObjectFactoryWithReset<T> where T : class, IDisposable
    {
        private T value;
        private readonly Func<T> createAction;
        private readonly object valueGuard = new object();

        public T Value
        {
            get
            {
                if (value == null)
                {
                    lock (valueGuard)
                    {
                        if (value == null)
                            value = createAction();
                    }
                }

                return value;
            }
        }

        public ObjectFactoryWithReset(Func<T> createAction)
        {
            this.createAction = createAction;
            value = createAction(); // Immediately create instrance, as suggested by tomspilman
        }

        public void Reset()
        {
#if DIRECTX
            lock (valueGuard)
            {
                SharpDX.Utilities.Dispose(ref value);
            }
#endif
        }
    }
}
