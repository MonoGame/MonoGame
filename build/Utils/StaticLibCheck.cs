using Cake.Common.Tools.VSWhere.Latest;
namespace BuildScripts;

/// <summary>
/// Validates dynamic library dependencies for platform-specific builds.
/// </summary>
/// <remarks>
/// <para>
/// This class is used in the build process to ensure that generated native libraries (.dll, .so, .dylib)
/// only link against a set of approved system libraries for each supported platform (Windows, Linux, macOS).
/// It helps prevent accidental linkage to disallowed or non-system libraries, which could cause portability or licensing issues.
/// </para>
/// <para>
/// Usage:
/// Call <see cref="StaticLibCheck.Check"/> during your build pipeline, passing the build context and the folder to check.
/// </para>
/// </remarks>
public class StaticLibCheck
{
    private static readonly string[] ValidWindowsLibs = {
        "WS2_32.dll",
        "KERNEL32.dll",
        "USER32.dll",
        "GDI32.dll",
        "WINMM.dll",
        "IMM32.dll",
        "ole32.dll",
        "OLEAUT32.dll",
        "VERSION.dll",
        "ADVAPI32.dll",
        "SETUPAPI.dll",
        "SHELL32.dll"
    };

    private static readonly string[] ValidLinuxLibs = {
        "linux-vdso.so",
        "libstdc++.so",
        "libgcc_s.so",
        "libc.so",
        "libm.so",
        "libdl.so",
        "libpthread.so",
        "/lib/ld-linux-",
        "/lib64/ld-linux-"
    };

    public void Check(BuildContext context, string folder)
    {
        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                CheckWindows(context, folder);
                break;
            case PlatformFamily.Linux:
                CheckLinux(context, folder);
                break;
            case PlatformFamily.OSX:
                CheckMacOS(context, folder);
                break;
            default:
                throw new NotSupportedException($"Platform {context.Environment.Platform.Family} is not supported for static library checks.");
        }
    }

    public void CheckWindows(BuildContext context, string folder)
    {
        var vswhere = new VSWhereLatest(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools);
        var devcmdPath = vswhere.Latest(new VSWhereLatestSettings()).FullPath + @"\Common7\Tools\vsdevcmd.bat";

        foreach (var filePath in Directory.GetFiles(folder, "*.dll"))
        {
            context.Information($"Checking: {filePath}");
            context.StartProcess(
                devcmdPath,
                new ProcessSettings()
                {
                    Arguments = $"& dumpbin /dependents /nologo {filePath}",
                    RedirectStandardOutput = true
                },
                out IEnumerable<string> processOutput
            );

            var passedTests = true;
            foreach (string output in processOutput)
            {
                var libPath = output.Trim();
                if (!libPath.EndsWith(".dll") || libPath.Contains(' '))
                    continue;

                if (ValidWindowsLibs.Contains(libPath))
                {
                    context.Information($"VALID: {libPath}");
                }
                else
                {
                    context.Information($"INVALID: {libPath}");
                    passedTests = false;
                }
            }

            if (!passedTests)
            {
                throw new Exception("Invalid library linkage detected!");
            }
        }
    }

    public void CheckMacOS(BuildContext context, string folder)
    {
        foreach (var filePath in Directory.GetFiles(folder, "*.dylib"))
        {
            context.Information($"Checking Universal Binary: {filePath}");

            context.StartProcess(
                "lipo",
                new ProcessSettings { Arguments = $"-archs {filePath}", RedirectStandardOutput = true },
                out IEnumerable<string> lipoOutput);

            var archs = (lipoOutput.FirstOrDefault() ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (archs.Length < 2)
            {
                context.Warning($"Warning: '{filePath}' is not a universal binary. Found archs: {string.Join(", ", archs)}");
            }
            else
            {
                context.Information($"Found architectures: {string.Join(", ", archs)}");
            }

            foreach (var arch in archs)
            {
                context.StartProcess(
                    "dyld_info",
                    new ProcessSettings
                    {
                        Arguments = $"-arch {arch} -dependents \"{filePath}\"",
                        RedirectStandardOutput = true
                    },
                    out IEnumerable<string> processOutput);

                var processOutputList = processOutput.ToList();
                var passedTests = true;
                for (int i = 3; i < processOutputList.Count; i++)
                {
                    var libPath = processOutputList[i].Trim().Split(' ')[^1];
                    if (libPath.Contains('['))
                    {
                        i += 2;
                        continue;
                    }

                    if (libPath.StartsWith("/usr/lib/") || libPath.StartsWith("/System/Library/Frameworks/"))
                    {
                        context.Information($"VALID: {libPath}");
                    }
                    else
                    {
                        context.Information($"INVALID: {libPath}");
                        passedTests = false;
                    }
                }

                if (!passedTests)
                {
                    throw new Exception($"Invalid library linkage detected in arch '{arch}' for {filePath}!");
                }
            }

            context.Information("");
        }
    }

    public void CheckLinux(BuildContext context, string folder)
    {
        foreach (var filePath in Directory.GetFiles(folder, "*.so*"))
        {
            context.Information($"Checking: {filePath}");
            context.StartProcess(
                "ldd",
                new ProcessSettings
                {
                    Arguments = $"{filePath}",
                    RedirectStandardOutput = true
                },
                out IEnumerable<string> processOutput);

            var passedTests = true;
            foreach (var line in processOutput)
            {
                var libPath = line.Trim().Split(' ')[0];

                var isValidLib = false;
                foreach (var validLib in ValidLinuxLibs)
                {
                    if (libPath.StartsWith(validLib))
                    {
                        isValidLib = true;
                        break;
                    }
                }

                if (isValidLib)
                {
                    context.Information($"VALID: {libPath}");
                }
                else
                {
                    context.Information($"INVALID: {libPath}");
                    passedTests = false;
                }
            }

            if (!passedTests)
            {
                throw new Exception("Invalid library linkage detected!");
            }

            context.Information("");
        }
    }
}
