@echo off
setlocal

SET MGFXC="..\..\..\Artifacts\MonoGame.Effect.Compiler\Release\mgfxc.exe"
SET ROOTDIR="..\..\.."

echo ---------------------------------------------------------------
echo Vulkan
echo ---------------------------------------------------------------
echo.
@for /f %%f IN ('dir /b ..\..\..\MonoGame.Framework\Platform\Graphics\Effect\Resources\*.fx') do (

  echo %%~f
  call %MGFXC% "..\..\..\MonoGame.Framework\Platform\Graphics\Effect\Resources\%%~f" %%~nf.mgfxo.h /Profile:Vulkan /RelativeRoot:%ROOTDIR%
  echo.
)
echo.

endlocal
pause
