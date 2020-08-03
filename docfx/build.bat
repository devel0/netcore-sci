cd %~dp0

rd ..\docs /q /s

docfx

mkdir ..\docs\test

xcopy /s ..\test ..\docs\test /exclude:exclude.txt

mkdir ..\docs\data
mkdir ..\docs\data\techdocs

xcopy /s ..\data\techdocs ..\docs\data\techdocs /exclude:exclude.txt
