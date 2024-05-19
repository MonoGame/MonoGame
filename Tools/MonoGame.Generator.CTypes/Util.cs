
namespace MonoGame.Generator.CTypes;

class Util
{
    public static string GetCEnumType(string cstype) => cstype switch
    {
        "System.Byte" => "csbyte",
        "System.Int16" => "csshort",
        "System.UInt16" => "csushort",
        "System.Int32" => "csint",
        "System.UInt32" => "csuint",
        "System.Int64" => "cslong",
        "System.UInt64" => "csulong",
        _ => "CS" + cstype
    };
}
