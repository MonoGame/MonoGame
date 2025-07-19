@echo off
setlocal

SET MGFXC="..\..\..\Artifacts\MonoGame.Effect.Compiler\Release\mgfxc.exe"
SET DXC="..\..\..\Artifacts\MonoGame.Effect.Compiler\Release\dxc.exe"
SET ROOTDIR="..\..\.."

echo ---------------------------------------------------------------
echo DirectX 12
echo ---------------------------------------------------------------
echo.
@for /f %%f IN ('dir /b ..\..\..\MonoGame.Framework\Platform\Graphics\Effect\Resources\*.fx') do (

  echo %%~f
  call %MGFXC% "..\..\..\MonoGame.Framework\Platform\Graphics\Effect\Resources\%%~f" %%~nf.dx12.mgfxo.h /Debug /Profile:DirectX_12 /RelativeRoot:%ROOTDIR%
  echo.
)
echo.

call %DXC% -T cs_6_0 -O3 -Vn GenerateMips_main -Fh GenerateMips_Desktop.h GenerateMips.hlsl

endlocal
pause
