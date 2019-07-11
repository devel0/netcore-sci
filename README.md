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
    - [Acceleration](doc/api/Sci/PQCollection/Acceleration.md)
    - [Adimensional](doc/api/Sci/PQCollection/Adimensional.md)
    - [AmountOfSubstance](doc/api/Sci/PQCollection/AmountOfSubstance.md)
    - [AngularAcceleration](doc/api/Sci/PQCollection/AngularAcceleration.md)
    - [AngularSpeed](doc/api/Sci/PQCollection/AngularSpeed.md)
    - [BendingMoment](doc/api/Sci/PQCollection/BendingMoment.md)
    - [ElectricalConductance](doc/api/Sci/PQCollection/ElectricalConductance.md)
    - [ElectricalConductivity](doc/api/Sci/PQCollection/ElectricalConductivity.md)
    - [ElectricCurrent](doc/api/Sci/PQCollection/ElectricCurrent.md)
    - [Energy](doc/api/Sci/PQCollection/Energy.md)
    - [Force](doc/api/Sci/PQCollection/Force.md)
    - [Frequency](doc/api/Sci/PQCollection/Frequency.md)
    - [Length](doc/api/Sci/PQCollection/Length.md)
    - [Length2](doc/api/Sci/PQCollection/Length2.md)
    - [Length3](doc/api/Sci/PQCollection/Length3.md)
    - [Length4](doc/api/Sci/PQCollection/Length4.md)
    - [LuminousIntensity](doc/api/Sci/PQCollection/LuminousIntensity.md)
    - [Mass](doc/api/Sci/PQCollection/Mass.md)
    - [PlaneAngle](doc/api/Sci/PQCollection/PlaneAngle.md)
    - [Power](doc/api/Sci/PQCollection/Power.md)
    - [Pressure](doc/api/Sci/PQCollection/Pressure.md)
    - [Speed](doc/api/Sci/PQCollection/Speed.md)
    - [Temperature](doc/api/Sci/PQCollection/Temperature.md)
    - [Time](doc/api/Sci/PQCollection/Time.md)
    - [Turbidity](doc/api/Sci/PQCollection/Turbidity.md)
    - [VolumetricFlowRate](doc/api/Sci/PQCollection/VolumetricFlowRate.md)
  - [MeasureUnit](doc/api/Sci/MeasureUnit.md)
  - [MUDomain](doc/api/Sci/MUDomain.md)
  - [MUCollection](doc/api/Sci/MUCollection.md)
    - [Adimensional](doc/api/Sci/Adimensional.md)
    - [Frequency](doc/api/Sci/Frequency.md)
    - [Length](doc/api/Sci/Length.md)
    - [Length2](doc/api/Sci/Length2.md)
    - [Length3](doc/api/Sci/Length3.md)
    - [Length4](doc/api/Sci/Length4.md)
    - [VolumetricFlowRate](doc/api/Sci/VolumetricFlowRate.md)
    - [Mass](doc/api/Sci/Mass.md)
    - [Time](doc/api/Sci/Time.md)
    - [ElectricCurrent](doc/api/Sci/ElectricCurrent.md)
    - [Temperature](doc/api/Sci/Temperature.md)
    - [AmountOfSubstance](doc/api/Sci/AmountOfSubstance.md)
    - [LuminousIntensity](doc/api/Sci/LuminousIntensity.md)
    - [PlaneAngle](doc/api/Sci/PlaneAngle.md)
    - [AngularSpeed](doc/api/Sci/AngularSpeed.md)
    - [AngularAcceleration](doc/api/Sci/AngularAcceleration.md)
    - [Pressure](doc/api/Sci/Pressure.md)
    - [Power](doc/api/Sci/Power.md)
    - [Acceleration](doc/api/Sci/Acceleration.md)
    - [Turbidity](doc/api/Sci/Turbidity.md)
    - [Force](doc/api/Sci/Force.md)
    - [Speed](doc/api/Sci/Speed.md)
    - [BendingMoment](doc/api/Sci/BendingMoment.md)
    - [Energy](doc/api/Sci/Energy.md)
    - [ElectricalConductance](doc/api/Sci/ElectricalConductance.md)
    - [ElectricalConductivity](doc/api/Sci/ElectricalConductivity.md)
    

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
