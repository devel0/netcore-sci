# netcore-sci

[![devel0 MyGet Build Status](https://www.myget.org/BuildSource/Badge/devel0?identifier=4e07c427-6364-4390-8a07-1c615433e660)](https://www.myget.org/)

.NET core sci

## install and usage

browse [myget istructions](https://www.myget.org/feed/devel0/package/nuget/netcore-sci)

## debugging unit tests

- from vscode just run debug test from code lens balloon

## how this project was built

```sh
mkdir netcore-sci
cd netcore-sci

dotnet new sln
dotnet new classlib -n netcore-sci

cd netcore-sci
dotnet add package netcore-util --version 1.0.0-CI00039 --source https://www.myget.org/F/devel0/api/v3/index.json
dotnet add package netcore-psql-util --version 1.0.0-CI00003 --source https://www.myget.org/F/devel0/api/v3/index.json
dotnet add package netDXF.Standard --version 2.1.1
cd ..

dotnet new xunit -n test
cd test
dotnet add reference ../netcore-sci/netcore-sci.csproj
cd ..

dotnet sln netcore-sci.sln add netcore-sci/netcore-sci.csproj
dotnet sln netcore-sci.sln add test/test.csproj 
dotnet restore
dotnet build
dotnet test test/test.csproj
```
