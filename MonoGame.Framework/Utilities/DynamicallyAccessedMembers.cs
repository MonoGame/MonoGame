// fake attribute for backward compatibility with .NET 4.5

#if NET45
namespace System.Diagnostics.CodeAnalysis
{
    [Flags]
    public enum DynamicallyAccessedMemberTypes
    {
        All = -1,
        None = 0,
        PublicParameterlessConstructor = 1,
        PublicConstructors = 3,
        NonPublicConstructors = 4,
        PublicMethods = 8,
        NonPublicMethods = 16,
        PublicFields = 32,
        NonPublicFields = 64,
        PublicNestedTypes = 128,
        NonPublicNestedTypes = 256,
        PublicProperties = 512,
        NonPublicProperties = 1024,
        PublicEvents = 2048,
        NonPublicEvents = 4096,
        Interfaces = 8192
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false)]
    public sealed class DynamicallyAccessedMembersAttribute : Attribute
    {
        private DynamicallyAccessedMemberTypes _memberTypes;

        public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
        {
            _memberTypes = memberTypes;
        }

        public DynamicallyAccessedMemberTypes MemberTypes { get { return _memberTypes; } }
    }
}
#endif
