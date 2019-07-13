# MUDomain Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → MUDomain

[list of physical quantities](https://en.wikipedia.org/wiki/List_of_physical_quantities)
            
            Measures here contains information about implicit measure unit
            and value of the tolerance.
            
            Note that all measure must be dimensionally equivalent.
            For example:
            [length] = m
            [length2] = [length] * [length] = m2
            [time] = s
            [time2] = [time] * [time] = s2
            [speed] = [length] / [time] = m/s
            [acceleration] = [length] / [time2] = m/s2
            [mass] = kg
            [force] = [mass] * [acceleration] = kg * m/s2 = N
            [pressure] = [force] / [length2] = N / m2 = Pa
            
            This will ensure measure comparision without further conversion, for example
            m1 = 1 [kg]
            a1 = 2 [m/s2]
            f1 = 4 [N]
            
            test = m1 * a1 > f1

## Signature
```csharp
public class MUDomain : SearchAThing.Sci.IMUDomain
```
## Methods
|**Name**|**Summary**|
|---|---|
|[Equals](MUDomain/Equals.md)||
|[GetHashCode](MUDomain/GetHashCode.md)||
|[GetType](MUDomain/GetType.md)||
|[SetupItem](MUDomain/SetupItem.md)|allow to set programmatically the associated measure unit and tolerance in the model of a given physical quantity<br/>            if given defaulttolreance is null, then current default tolerance will be converted to given measure unit|
|[ToString](MUDomain/ToString.md)||
## Properties
|**Name**|**Summary**|
|---|---|
|[_All](MUDomain/_All.md)|
|[Acceleration](MUDomain/Acceleration.md)|[L T−2]
|[Adimensional](MUDomain/Adimensional.md)|
|[AmountOfSubstance](MUDomain/AmountOfSubstance.md)|[N]
|[AngularAcceleration](MUDomain/AngularAcceleration.md)|[T−2]
|[AngularSpeed](MUDomain/AngularSpeed.md)|[T−1]
|[BendingMoment](MUDomain/BendingMoment.md)|[M L2 T-2]
|[ElectricalConductance](MUDomain/ElectricalConductance.md)|[L−2 M−1 T3 I2]
|[ElectricalConductivity](MUDomain/ElectricalConductivity.md)|[L−3 M−1 T3 I2]
|[ElectricCurrent](MUDomain/ElectricCurrent.md)|[I]
|[Energy](MUDomain/Energy.md)|[M L2 T−2]
|[Force](MUDomain/Force.md)|[M L T−2]
|[Frequency](MUDomain/Frequency.md)|[T-1]
|[Length](MUDomain/Length.md)|[L]
|[Length2](MUDomain/Length2.md)|[L^2]
|[Length3](MUDomain/Length3.md)|[L^3]
|[Length4](MUDomain/Length4.md)|[L^4]
|[LuminousIntensity](MUDomain/LuminousIntensity.md)|[J]
|[Mass](MUDomain/Mass.md)|[M]
|[PlaneAngle](MUDomain/PlaneAngle.md)|[1]
|[Power](MUDomain/Power.md)|[M L2 T−3]
|[Pressure](MUDomain/Pressure.md)|[M L−1 T−2]
|[Speed](MUDomain/Speed.md)|[L T−1]
|[Temperature](MUDomain/Temperature.md)|[K]
|[Time](MUDomain/Time.md)|[T]
|[Turbidity](MUDomain/Turbidity.md)|
|[VolumetricFlowRate](MUDomain/VolumetricFlowRate.md)|[L3 T−1]
## Conversions
