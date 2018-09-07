# netcore-sci

[![devel0 MyGet Build Status](https://www.myget.org/BuildSource/Badge/devel0?identifier=4e07c427-6364-4390-8a07-1c615433e660)](https://www.myget.org/)

.NET core sci

## install and usage

browse [myget istructions](https://www.myget.org/feed/devel0/package/nuget/netcore-sci)

## debugging unit tests

- from vscode just run debug test from code lens balloon

## how this project was built

```sh
mkdir -p netcore-sci/{src,test}
cd netcore-sci
dotnet new sln
cd src
dotnet new classlib
cd ../test
dotnet new xunit
dotnet add reference ../src/src.csproj
dotnet sln netcore-sci.sln add src/src.csproj
dotnet sln netcore-sci.sln add test/test.csproj 
dotnet restore
dotnet build
dotnet test test/test.csproj
```
