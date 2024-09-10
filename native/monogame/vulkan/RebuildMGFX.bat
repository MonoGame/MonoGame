@echo off
setlocal

SET MGFXC="..\..\..\Artifacts\MonoGame.Effect.Compiler\Release\mgfxc.exe"

echo ---------------------------------------------------------------
echo Vulkan
echo ---------------------------------------------------------------
echo.
@for /f %%f IN ('dir /b ..\..\..\MonoGame.Framework\Platform\Graphics\Effect\Resources\*.fx') do (

  echo %%~f
  call %MGFXC% "..\..\..\MonoGame.Framework\Platform\Graphics\Effect\Resources\%%~f" %%~nf.vk.mgfxo /Profile:Vulkan
  echo.
)
echo.

endlocal
pause
