@echo off
setlocal

SET MGFXC="..\..\..\..\..\artifacts\MonoGame.Effect.Compiler\Release\mgfxc.exe"

@for /f %%f IN ('dir /b *.fx') do (

  call %MGFXC% %%~nf.fx %%~nf.ogl.mgfxo

  call %MGFXC% %%~nf.fx %%~nf.dx11.mgfxo /Profile:DirectX_11

)

endlocal
pause
