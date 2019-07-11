# MUDomain Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → MUDomain

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
- [Equals](MUDomain/Equals.md)
- [GetHashCode](MUDomain/GetHashCode.md)
- [GetType](MUDomain/GetType.md)
- [SetupItem](MUDomain/SetupItem.md)
- [ToString](MUDomain/ToString.md)
## Properties
- [_All](MUDomain/_All.md)
- [Acceleration](MUDomain/Acceleration.md)
- [Adimensional](MUDomain/Adimensional.md)
- [AmountOfSubstance](MUDomain/AmountOfSubstance.md)
- [BendingMoment](MUDomain/BendingMoment.md)
- [ElectricalConductance](MUDomain/ElectricalConductance.md)
- [ElectricalConductivity](MUDomain/ElectricalConductivity.md)
- [ElectricCurrent](MUDomain/ElectricCurrent.md)
- [Energy](MUDomain/Energy.md)
- [Force](MUDomain/Force.md)
- [Frequency](MUDomain/Frequency.md)
- [Length](MUDomain/Length.md)
- [Length2](MUDomain/Length2.md)
- [Length3](MUDomain/Length3.md)
- [Length4](MUDomain/Length4.md)
- [LuminousIntensity](MUDomain/LuminousIntensity.md)
- [Mass](MUDomain/Mass.md)
- [PlaneAngle](MUDomain/PlaneAngle.md)
- [Power](MUDomain/Power.md)
- [Pressure](MUDomain/Pressure.md)
- [Speed](MUDomain/Speed.md)
- [Temperature](MUDomain/Temperature.md)
- [Time](MUDomain/Time.md)
- [Turbidity](MUDomain/Turbidity.md)
- [VolumetricFlowRate](MUDomain/VolumetricFlowRate.md)
## Conversions
