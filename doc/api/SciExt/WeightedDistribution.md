# SearchAThing.SciExt.WeightedDistribution method
## WeightedDistribution(IEnumerable<double>, int)
retrieve a list of N pairs (value,presence)
             with value between min and max of inputs and presence between 0..1 that represents the percent of presence of the value
             
             examples:
            
             inputs = ( 1, 2, 3 ), N = 3
             results: ( (1, .33), (2, .33), (3, .33) )
             
             inputs = ( 1, 2.49, 3), N = 3
             results: ( (1, .33), (2, .169), (3, .497) )
             
             inputs = ( 1, 2, 3), N = 4
             results: ( (1, .33), (1.6, .16), (2.3, .16), (3, .33) )

### Signature
```csharp
public static System.ValueTuple<double, double>[] WeightedDistribution(IEnumerable<double> inputs, int N)
```
