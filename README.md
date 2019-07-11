# netcore-sci

[![NuGet Badge](https://buildstats.info/nuget/netcore-sci)](https://www.nuget.org/packages/netcore-sci/)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=devel0_netcore-sci&metric=alert_status)](https://sonarcloud.io/dashboard?id=devel0_netcore-sci)

.NET core sci

## documentation

- [**FULL API REFERENCE LIST**](doc/api/)

- **3D Geometry**
  - [Vector3D](doc/api/Sci/Vector3D.md)
  - [Line3D](doc/api/Sci/Line3D.md)  
  - [Transform3D](doc/api/Sci/Transform3D.md)  
  - [CoordinateSystem3D](doc/api/Sci/CoordinateSystem3D.md)
  - [Plane3D](doc/api/Sci/Plane3D.md)
  - [Arc3D](doc/api/Sci/Arc3D.md)
  - [Circle3D](doc/api/Sci/Circle3D.md)
  - [Geometry](doc/api/Sci/Geometry.md)
  - [BBox3D](doc/api/Sci/BBox3D.md)  
  - [DiscreteSpace](doc/api/Sci/DiscreteSpace-1.md)
  - [DxfKit](doc/api/Sci/DxfKit.md)
  - [Polygon](doc/api/Sci/Polygon.md)
- [**Extension methods**](doc/api/SciExt.md)
- [Python wrapper](doc/api/PythonPipe.md)
- **Measure unit and physical quantities**
  - [Measure](doc/api/Sci/Measure.md)
  - [PhysicalQuantity](doc/api/Sci/PhysicalQuantity.md)
  - [PQCollection](doc/api/Sci/PQCollection.md)
  - [MeasureUnit](doc/api/Sci/MeasureUnit.md)
  - [MUDomain](doc/api/Sci/MUDomain.md)
  - [MUCollection](doc/api/Sci/MUCollection.md)
    

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
