@echo off
setlocal

SET TWOMGFX="..\..\..\..\Tools\bin\2mgfx.exe"

@for /f %%f IN ('dir /b *.fx') do (

  call %TWOMGFX% %%f

)

endlocal
