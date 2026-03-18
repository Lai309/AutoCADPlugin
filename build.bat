@echo off
set acad="C:\Program Files\Autodesk\AutoCAD 2023"

csc /target:library ^
/reference:%acad%\acdbmgd.dll ^
/reference:%acad%\acmgd.dll ^
/out:RBLayout.dll ^
LayoutManager.cs

pause