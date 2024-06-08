./build -t 'Build ConsoleCheck'
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE; }

dotnet run --project console_check/ConsoleRecompile.csproj
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE; }

dotnet build console_check/Build/ConsoleCheck.csproj -c Release
exit $LASTEXITCODE;
