# SearchAThing.SciExt.Thin method
## Thin(IEnumerable<double>, double, bool, bool)
retrieve given input set ordered with only distinct values after comparing through tolerance
            in this case result set contains only values from the input set (default) or rounding to given tol if maintain_original_values is false;
            if keep_ends true (default) min and max already exists at begin/end of returned sequence

### Signature
```csharp
public static System.Collections.Generic.List<double> Thin(IEnumerable<double> input, double tol, bool keep_ends = True, bool maintain_original_values = True)
```
