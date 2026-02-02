using System;

namespace MonoGame.Framework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method |
                    AttributeTargets.Property | AttributeTargets.Field,
                    AllowMultiple = true)]
    public sealed class UnsupportedOnAttribute : Attribute
    {
        public string MonoGamePlatforms { get; }
        public string Message { get; }

        public UnsupportedOnAttribute(string monogamePlatforms, string message)
        {
            MonoGamePlatforms = monogamePlatforms;
            Message = message;
        }
    }
}
