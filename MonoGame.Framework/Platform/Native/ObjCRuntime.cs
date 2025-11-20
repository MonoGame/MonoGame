using System;

namespace ObjCRuntime;

[AttributeUsage(AttributeTargets.Method)]
class MonoPInvokeCallbackAttribute : Attribute
{
    public MonoPInvokeCallbackAttribute(Type t)
    {

    }
}