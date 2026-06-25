# Contours

Contours are closed boundaries made from bounded parameterized curves.
They live in the `Akeldov.Math.Spatial2D.Contours` namespace.

Each curve must continue from the previous curve, and the final curve must close the contour.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var contour = new CompositeContour(new IFinitePath[]
{
    new ParameterizedArc(
        center: new PointXY(0f, 0f),
        radius: 5f,
        startAngle: 0f,
        endAngle: 2f * MathF.PI,
        angularDirection: AngularDirection.Counterclockwise)
});

bool isInside = contour.Encloses(new PointXY(3f, 0f));
```

Use contours for boundaries. Use [regions](../regions/index.md) when you need filled area membership.

## Smoothing and Fillets

Contour smoothing helpers produce rounded contour variants while preserving the original contour as input.
Use smoothing when a region or rasterized boundary should avoid hard corners but still be constructed from curve primitives.

Fillets replace a sharp corner with a tangent arc.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

PointXY firstSidePoint = new PointXY(1f, 0f);
PointXY vertex = new PointXY(0f, 0f);
PointXY secondSidePoint = new PointXY(0f, 1f);

Circle tangentCircle = CornerExtensions.CreateCornerTangentCircle(
    firstSidePoint,
    vertex,
    secondSidePoint,
    radius: 0.25f);

Arc filletArc = CornerExtensions.CreateFilletArc(
    firstSidePoint,
    vertex,
    secondSidePoint,
    radius: 0.25f);
```

Fillet construction uses world coordinate units for radius and radians for any angle-based values.
