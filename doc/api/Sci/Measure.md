# Measure Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → Measure

(No Description)

## Signature
```csharp
public class Measure
```
## Constructors
|**Name**|**Summary**|
|---|---|
|[Measure(double, MeasureUnit)](Measure/ctors.md)||
## Methods
|**Name**|**Summary**|
|---|---|
|[Convert](Measure/Convert.md) (static)|convert given value from to measure units|
|[Convert](Measure/Convert.md#convertdouble-measureunit-imudomain) (static)|convert given value from to measure units<br/>            to measure unit is given from the correspondent physical quantity measure unit in the given domain|
|[Convert](Measure/Convert.md#convertdouble-imudomain-measureunit) (static)|convert given value from to measure units<br/>            from measure unit is given from the correspondent physical quantity measure unit in the given domain|
|[ConvertTo](Measure/ConvertTo.md)|Convert to the implicit measure of the given mu domain|
|[ConvertTo](Measure/ConvertTo.md#converttomeasureunit)||
|[Equals](Measure/Equals.md)||
|[GetHashCode](Measure/GetHashCode.md)||
|[GetType](Measure/GetType.md)||
|[MRound](Measure/MRound.md)|return this measure rounded by the given tol<br/>            this will not change current MU|
|[ToString](Measure/ToString.md)||
|[ToString](Measure/ToString.md#tostringint)||
|[ToString](Measure/ToString.md#tostringbool-nullableint)||
|[TryParse](Measure/TryParse.md) (static)||
## Properties
|**Name**|**Summary**|
|---|---|
|[ExpPref](Measure/ExpPref.md)|use of exponential pref<br/>            eg. <br/>            120 with ExpPref=2 -> 1.2e2<br/>            120 with ExpPref=-1 -> 1200e-1
|[MU](Measure/MU.md)|
|[Value](Measure/Value.md)|
## Operators
- [*](Measure/op_Multiply.md)
- [*](Measure/op_Multiply.md)
- [/](Measure/op_Division.md)
- [-](Measure/op_UnaryNegation.md)
## Conversions
