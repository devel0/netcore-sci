# SearchAThing.SciExt.AngleInRange method
## AngleInRange(double, double, double, double)
states if given angle is contained in from, to angle range;
            multiturn angles are supported because test will normalize to [0,2pi) automatically.

### Signature
```csharp
public static bool AngleInRange(double pt_angle, double tol_rad, double angle_from, double angle_to)
```
### Parameters
- `pt_angle`: angle(rad) to test
- `tol_rad`: angle(rad) tolerance
- `angle_from`: angle(rad) from
- `angle_to`: angle(rad) to

