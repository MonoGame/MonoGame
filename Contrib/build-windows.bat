%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /version
:: WindowsDX
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Release /t:Rebuild "../MonoGame.Framework.Windows.sln"
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Debug /t:Rebuild "../MonoGame.Framework.Windows.sln"
:: WindowsGL
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Release /t:Rebuild "../MonoGame.Framework.WindowsGL.sln"
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Debug /t:Rebuild "../MonoGame.Framework.WindowsGL.sln"
:: Windows8
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Release /t:Rebuild "../MonoGame.Framework.Windows8.sln"
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Debug /t:Rebuild "../MonoGame.Framework.Windows8.sln"
:: WindowsPhone - ARM
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Release /p:Platform=ARM /t:Rebuild "../MonoGame.Framework.WindowsPhone.sln"
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Debug /p:Platform=ARM /t:Rebuild "../MonoGame.Framework.WindowsPhone.sln"
:: WindowsPhone - x86
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Release /p:Platform=x86 /t:Rebuild "../MonoGame.Framework.WindowsPhone.sln"
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Debug /p:Platform=x86 /t:Rebuild "../MonoGame.Framework.WindowsPhone.sln"
:: Android
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Release /t:Rebuild "../MonoGame.Framework.Android.sln"
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /ds /p:Configuration=Debug /t:Rebuild "../MonoGame.Framework.Android.sln"