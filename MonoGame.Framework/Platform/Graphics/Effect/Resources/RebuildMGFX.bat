@echo off
setlocal

SET MGFXC="..\..\..\..\..\Artifacts\MonoGame.Effect.Compiler\Release\mgfxc.exe"

echo ---------------------------------------------------------------
echo OpenGL
echo ---------------------------------------------------------------
echo.
@for /f %%f IN ('dir /b *.fx') do (

  echo %%~f
  call %MGFXC% %%~f %%~nf.ogl.mgfxo
  echo.

)
echo.

echo ---------------------------------------------------------------
echo DirectX 11
echo ---------------------------------------------------------------
echo.
@for /f %%f IN ('dir /b *.fx') do (

  echo %%~f
  call %MGFXC% %%~f %%~nf.dx11.mgfxo /Profile:DirectX_11
  echo.
)
echo.

endlocal
pause
