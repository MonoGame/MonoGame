@echo off
setlocal

SET TWOMGFX="..\..\..\..\Tools\2MGFX\bin\Windows\AnyCPU\Release\2mgfx.exe"

@for /f %%f IN ('dir /b *.fx') do (

  call %TWOMGFX% %%~nf.fx %%~nf.ogl.mgfxo

  call %TWOMGFX% %%~nf.fx %%~nf.dx11.mgfxo /Profile:DirectX_11

)

endlocal
