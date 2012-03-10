using System;

#if WINRT
using System.Reflection;
#endif

namespace Microsoft.Xna.Framework.Content
{
    public static class ContentExtensions
    {
        public static bool GetIsValueType(this Type type)
        {
#if WINRT
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }
    }
}
