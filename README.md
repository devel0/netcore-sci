# netcore-sci

.NET core sci

## install and usage

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
cd ..
dotnet sln netcore-util.sln add src/src.csproj
dotnet sln netcore-util.sln add test/test.csproj 
dotnet restore
dotnet build
dotnet test test/test.csproj
```
