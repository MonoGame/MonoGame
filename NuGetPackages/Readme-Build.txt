Example commands to build NuGet Packages (requires an output folder called "packages" that exists)

Builds require the -BasePath parameter to point to the MonoGame.Framework\bin directory, as this is where all the NuGet's are based on

NuGet pack MonoGame.Framework.Android.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.iOS.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.Linux.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.MacOS.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.MonoMac.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.Ouya.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.Windows8.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.WindowsDX.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.WindowsGL.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.WindowsPhone8.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.WindowsPhone81.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
NuGet pack MonoGame.Framework.WindowsUniversal.nuspec -BasePath ..\MonoGame.Framework\bin -OutputDirectory packages
