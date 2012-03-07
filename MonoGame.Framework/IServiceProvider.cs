using System;

namespace System
{
    public interface IServiceProvider
    {
        object GetService(Type serviceType);
    }
}
