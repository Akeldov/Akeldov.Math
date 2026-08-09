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
