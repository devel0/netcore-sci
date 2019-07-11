# netcore-sci

[![NuGet Badge](https://buildstats.info/nuget/netcore-sci)](https://www.nuget.org/packages/netcore-sci/)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=devel0_netcore-sci&metric=alert_status)](https://sonarcloud.io/dashboard?id=devel0_netcore-sci)

.NET core sci

## documentation

- [api reference](api-doc/docs/SearchAThing)

## install

- [nuget package](https://www.nuget.org/packages/netcore-sci/)

## usage

check out [unit tests](test)

## unit tests

- debugging unit tests
  - from vscode just run debug test from code lens balloon
- executing all tests
  - from solution root folder `dotnet test`
- testing coverage
  - from vscode run task ( ctrl+shift+p ) `Tasks: Run Task` then `test with coverage` ( `.NET Core Test Explorer` extension required then move to some file eg. Vector3D.cs and click on `Add Watch` from bottom bar )

## how this project was built

```sh
mkdir netcore-sci
cd netcore-sci

dotnet new sln
dotnet new classlib -n netcore-sci

cd netcore-sci
dotnet add package netcore-util --version 1.0.2
dotnet add package netcore-psql-util --version 1.0.3
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
