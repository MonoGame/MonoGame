@echo off
@echo.
@echo === Debug ========================================================================
xmake f -m debug && xmake -r

@echo.
@echo === Release ======================================================================
xmake f -m release && xmake -r