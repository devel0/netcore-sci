#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

dotnet build /p:CopyLocalLockFileAssemblies=true
docpal --proptable --mgtable --mgspace -out ./tmp-api-doc "$exdir"/netcore-sci/bin/Debug/netstandard2.0/netcore-sci.dll

rm -fr "$exdir"/doc/api
mv "$exdir"/tmp-api-doc/docs/SearchAThing "$exdir"/doc/api
