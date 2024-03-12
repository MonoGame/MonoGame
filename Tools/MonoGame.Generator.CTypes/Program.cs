using System.Reflection;
using MonoGame.Generator.CTypes;

var repoDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../");
var monogamePlatformDir = Path.Combine(repoDirectory, "src/monogame/include");
var monogameFrameworkPath = Path.Combine(repoDirectory, "Artifacts/MonoGame.Framework/Native/Debug/MonoGame.Framework.dll");
var assembly = Assembly.LoadFile(monogameFrameworkPath);
var enumWritter = new EnumWritter();
var structWrittter = new StructWritter(enumWritter);

foreach (var type in assembly.GetTypes())
{
    if (type.FullName!.Contains("MonoGame.Interop"))
    {
        // Console.WriteLine(enumType.FullName!);

        
        //Console.WriteLine(type.Name + ": " + StructWritter.IsValid(type));

        if (!type.IsClass)
            continue;

        foreach (var method in type.GetMethods())
        {
            if (!method.IsStatic)
                continue;

            Console.WriteLine(method.Name);

            foreach (var parm in method.GetParameters())
            {
                Console.WriteLine(parm.ParameterType.Name + " " + parm.Name);
                //Console.WriteLine(parm.ParameterType.Name + ": " + StructWritter.IsValid(parm.ParameterType));
            }
        }
    }

    if (EnumWritter.IsValid(type))
    {
        enumWritter.Append(type);
    }
}

if (!Directory.Exists(monogamePlatformDir))
    Directory.CreateDirectory(monogamePlatformDir);

enumWritter.Flush(Path.Combine(monogamePlatformDir));
