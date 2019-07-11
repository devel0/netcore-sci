#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

dotnet build /p:CopyLocalLockFileAssemblies=true
docpal "$exdir"/netcore-sci/bin/Debug/netstandard2.0/netcore-sci.dll -out ./api-doc
