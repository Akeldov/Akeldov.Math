# Field Interfaces

`IField<TValue>` samples a value at a `PointXY`.

`IFloatField` and `IIntField` specialize this model for floating-point and integer fields.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

public sealed class HorizontalDistanceField : IFloatField
{
    public float Min => 6f;
    public float Max => 14f;

    public float Sample(PointXY point)
    {
        float t = point.X / 120f;
        return Min + (Max - Min) * t;
    }
}
```

Fields are used directly by sampling and rasterization workflows.

## Group Bound Fields

`FloatFieldRange` and `IntFieldRange` keep the fields that provide pointwise minimum and maximum
bounds together. This is useful when a downstream operation must sample both bounds at the same
positions:

```csharp
IFloatField minimumField = new HorizontalDistanceField();
IFloatField maximumField = new HorizontalDistanceField();

var range = new FloatFieldRange(minimumField, maximumField);
var (minField, maxField) = range;

float min = minField.Sample(new PointXY(10f, 20f));
float max = maxField.Sample(new PointXY(10f, 20f));
```

The constructor rejects `null` field references. It does not compare the sampled bounds: callers
remain responsible for ensuring that the minimum field does not exceed the maximum field at the
points relevant to their workflow.
