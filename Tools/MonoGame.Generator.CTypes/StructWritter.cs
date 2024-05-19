using System.Text;

namespace MonoGame.Generator.CTypes;

class StructWritter
{
    private EnumWritter _enumWritter;

    public StructWritter(EnumWritter enumWritter)
    {
        _enumWritter = enumWritter;
    }

    public static bool IsValid(Type type)
    {
        return type.IsValueType && !type.IsPrimitive && !type.IsNested;
    }

    public bool Append(Type type)
    {
        if (!IsValid(type))
            return false;

        return true;
    }
}
