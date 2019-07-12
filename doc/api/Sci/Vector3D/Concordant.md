# SearchAThing.Sci.Vector3D.Concordant method
## Concordant(double, Vector3D)
states if this vector concord to the given one
            
            **NOTE**: it does not test two vectors are parallels ( precondition must meet )

### Signature
```csharp
public bool Concordant(double tol, Vector3D other)
```
### Parameters
- `tol`: geometric tolerance ( Constants.NormalizedLengthTolerance if comparing normalized vectors )
- `other`: other vector

