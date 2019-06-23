#!/bin/bash
token="$(cat ~/security/sonar/login.txt)"
exdir="$(dirname `readlink -f "$0"`)"
cd "$exdir"
dotnet sonarscanner begin \
	/k:"devel0_netcore-sci" /n:"devel0_netcore-sci" /v:"1.0.10" /o:"devel0-github" \
	/d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="${token}" /d:sonar.language="cs" /d:sonar.exclusions="**/bin/**/*,**/obj/**/*" \
	/d:sonar.cs.opencover.reportsPaths="${exdir}/lcov.opencover.xml"
dotnet restore
dotnet build
dotnet test test/test.csproj \
	/p:CollectCoverage=true /p:CoverletOutputFormat=\"opencover,lcov\" /p:CoverletOutput=../lcov
dotnet sonarscanner end /d:sonar.login="${token}"
