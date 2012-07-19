@echo off
setlocal

SET TWOMGFX="..\..\..\..\Tools\bin\Windows\2mgfx.exe"

@for /f %%f IN ('dir /b *.fx') do (

  call %TWOMGFX% %%~nf.fx %%~nf.ogl.mgfxo

  call %TWOMGFX% %%~nf.fx %%~nf.dx11.mgfxo /DX11

)

endlocal
