#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

dotnet build /p:CopyLocalLockFileAssemblies=true
docpal --mgtable "$exdir"/netcore-sci/bin/Debug/netstandard2.0/netcore-sci.dll -out ./tmp-api-doc

rm -fr "$exdir"/doc/api
mv "$exdir"/tmp-api-doc/docs/SearchAThing "$exdir"/doc/api
