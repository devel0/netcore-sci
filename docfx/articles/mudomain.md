# Measure Unit Domain

## quickstart

```csharp
using System;
using SearchAThing.Sci;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            var mud = new MUDomain();

            Console.WriteLine($"In this MUD length mu is: {mud.Length}");

            var y = MUCollection.Length.yard;
            var m = MUCollection.Length.m;
            
            Console.WriteLine($"1 yard is {(1 * y).ConvertTo(m)} meters");               
        }
    }
}
```

output is:

```
In this MUD length mu is: pq=[Length] mu=[m] deftol=[0.0001]
1 yard is 0.9143999999999999 m meters
```

## limitations

- current implementation not allow multiply Measure between ; an static method in MUCollection.*PhysicalQuantity*.Auto is available for some convenience

```csharp
using SearchAThing.Sci;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            var mud = new MUDomain();

            var force_mu = MUCollection.Force.kN;
            var length_mu = MUCollection.Length.m;
            var pressure_mu = MUCollection.Pressure.Auto(force_mu, length_mu);

            System.Console.WriteLine($"force:[{force_mu}] length:[{length_mu}] -> pressure:[{pressure_mu}]");
        }
    }
}
```

output is:

```
force:[kN] length:[m] -> pressure:[kPa]
```

## usage guidelines

- The default measure unit domain uses International System of units foreach physical quantity [code](https://github.com/devel0/netcore-sci/blob/ec63f8fd4e4d06becd831e109fec17e4870f391d/netcore-sci/MUDomain.cs#L329)
- MUCollection contains all of the measure unit actually implemented by the library regardless the domain used and are organized by physical quantities
- As general rule when a MUDomain disambiguate your MU environment its suggested to work with double numeric instead of Measure to get maximum performance in calculations; to accomplish this without pitfall in conversion keep in mind that every time a variable describing a physical quantity measure exits from the code where a MUDomain relates its measure unit its suggested to save them with the measure unit ( Measure is [serializable](https://github.com/devel0/netcore-sci/blob/69f9f5c76fbdca353f9e874cfebdc1dc969130e3/netcore-sci/Measure.cs#L15) and simple [tostring](https://github.com/devel0/netcore-sci/blob/69f9f5c76fbdca353f9e874cfebdc1dc969130e3/netcore-sci/Measure.cs#L127-L172) and [parse](https://github.com/devel0/netcore-sci/blob/69f9f5c76fbdca353f9e874cfebdc1dc969130e3/netcore-sci/Measure.cs#L76) from string is provided). In general Measure should used every time a variable go out ( eg. toward disk ) or get in ( eg. load from disk ) or when used in binding ( eg. [wpf SciTextBox](https://github.com/devel0/SearchAThing.Wpf/blob/c8a21b6b54296e8b2f2957619fff337744438a96/SearchAThing.Wpf/SciTextBox.cs) )

## conversion between measure units and MUDomain

```csharp
using SearchAThing.Sci;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            var mud = new MUDomain();

            var force_mu = MUCollection.Force.kN;
            var length_mu = MUCollection.Length.m;
            var pressure_mu = MUCollection.Pressure.Auto(force_mu, length_mu);

            System.Console.WriteLine($"force:[{force_mu}] length:[{length_mu}] -> pressure:[{pressure_mu}]");

            System.Console.WriteLine($"in SI that pressure is:[{pressure_mu.Related(mud)}]");

            var pressure = 12.3 * pressure_mu;

            System.Console.WriteLine($"so that for example [{pressure}] = [{pressure.ConvertTo(mud)}] in SI");
        }
    }
}
```

output is:

```
force:[kN] length:[m] -> pressure:[kPa]
in SI that pressure is:[Pa]
so that for example [12.3 kPa] = [12300 Pa] in SI
```

conversion between measure unit of the same physical quantity can be done either with a Measure object and a MeasureUnit to convert to or directly within double in the case a MUD disambiguate its real meaning:

```csharp
using System;
using SearchAThing.Sci;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            var mud = new MUDomain();

            // suppose I loaded from a file some quantities accompanied with their mu
            var mass_loaded_from_file = (1200) * MUCollection.Mass.g;
            var acceleration_loaded_from_file = (9800) * MUCollection.Acceleration.mm_s2;

            Console.WriteLine($"loaded from external mass=[{mass_loaded_from_file}] and accel=[{acceleration_loaded_from_file}]");

            // now i translate to my SI
            var mass = mass_loaded_from_file.ConvertTo(mud); // it will convert to kg
            var accel = acceleration_loaded_from_file.ConvertTo(mud); // it will convert to m/s2            

            Console.WriteLine($"in my MUD mass=[{mass}] and accel=[{accel}]");

            // now mass and accel are able to expect it will return a force in my SI when multiplied
            // F = mass * acceleration where [N] = [kg] * [m/s2]            

            // warning: .Value property get the numeric value without any conversion
            // this is ok here because mass and accel are Measure already converted to my MUD
            var F_val = mass.Value * accel.Value;            

            var F = F_val * mud.Force.MU;
            // a safer approach could be
            // var F = F_val * MUCollection.Force.Auto(mass.MU, accel.MU);
            // but currently not yet implemented

            Console.WriteLine($"output to external file F = mass * accel = {F}");
        }
    }
}
```

output is:

```
loaded from external mass=[1200 g] and accel=[9800 mm_s2]
in my MUD mass=[1.2 kg] and accel=[9.8 m_s2]
output to external file F = mass * accel = 11.76 N
```

## possible alternatives

- [UnitsNet](https://github.com/angularsen/UnitsNet)
- [csunits](https://github.com/cureos/csunits)
