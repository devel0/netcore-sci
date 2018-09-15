# netcore-sci

[![devel0 MyGet Build Status](https://www.myget.org/BuildSource/Badge/devel0?identifier=4e07c427-6364-4390-8a07-1c615433e660)](https://www.myget.org/)

.NET core sci

- [Constants](netcore-sci/Constants.cs)
- [PQCollection](netcore-sci/PQCollection.cs), [PhysicalQuantity](netcore-sci/PhysicalQuantity.cs), [MUCollection](netcore-sci/MUCollection.cs), [MUDomain](netcore-sci/MUDomain.cs), [Measure](netcore-sci/Measure.cs), [MeasureUnit](netcore-sci/MeasureUnit.cs), [IModel](netcore-sci/IModel.cs), [SampleModel](netcore-sci/SampleModel.cs), [Project](netcore-sci/Project.cs)
- [Number](netcore-sci/Number.cs), [Tolerance](netcore-sci/Tolerance.cs)
- [BBox3D](netcore-sci/BBox3D.cs), [Geometry](netcore-sci/Geometry.cs), [Vector3D](netcore-sci/Vector3D.cs), [Line3D](netcore-sci/Line3D.cs), [CoordinateSystem3D](netcore-sci/CoordinateSystem3D.cs), [Plane3D](netcore-sci/Plane3D.cs), [Arc3D](netcore-sci/Arc3D.cs), [Circle3D](netcore-sci/Circle3D.cs), [Matrix3D](netcore-sci/Matrix3D.cs), [Transform3D](netcore-sci/Transform3D.cs)
- [DiscreteSpace](netcore-sci/DiscreteSpace.cs), [Int64Map](netcore-sci/Int64Map.cs)
- [Polygon](netcore-sci/Polygon.cs)
- [DxfKit](netcore-sci/DxfKit.cs)
- [Fluent](netcore-sci/Fluent.cs)
- [PythonWrapper](netcore-sci/PythonWrapper.cs)

## install and usage

browse [myget istructions](https://www.myget.org/feed/devel0/package/nuget/netcore-sci)

## usage

check out [unit tests](test)

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
