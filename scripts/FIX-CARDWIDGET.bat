@echo off
cd /d "%~dp0.."
echo Fixing CardWidget.cs...
git fetch origin cursor/working-up-to-variable-100-3fe3-7-14-26 2>nul
git show origin/cursor/working-up-to-variable-100-3fe3-7-14-26:scripts/CardWidget.clean.cs > Assets\DragonBoxAlgebra\Scripts\UI\CardWidget.cs
git show origin/cursor/working-up-to-variable-100-3fe3-7-14-26:scripts/CardWidget.clean.cs > dropins\CardWidget.cs 2>nul
if exist scripts\CardWidget.clean.cs copy /Y scripts\CardWidget.clean.cs Assets\DragonBoxAlgebra\Scripts\UI\CardWidget.cs
if exist scripts\CardWidget.clean.cs copy /Y scripts\CardWidget.clean.cs dropins\CardWidget.cs
echo.
echo DONE. Open Unity, wait for compile, press Play.
pause
