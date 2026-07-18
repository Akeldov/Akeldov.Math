# Akeldov.Math.Spatial2D

Akeldov.Math.Spatial2D is a .NET library for two-dimensional geometry, curves, contours, regions, rasterization, spatial sampling, partitioning, and influence fields.

## Features

- [Concepts](concepts/index.md)
    - [Angles and Units](concepts/angles-and-units.md)
    - [Floating-Point Tolerances](concepts/floating-point-tolerances.md)
    - [Collection Ownership](concepts/collection-ownership.md)
- [Points](points/index.md)
    - [PointXY](points/pointxy.md)
- [Vectors](vectors/index.md)
    - [VectorXY](vectors/vectorxy.md)
    - [VectorXYInt](vectors/vectorxyint.md)
- [Curves](curves/index.md)
    - [Curve Interfaces](curves/curve-interfaces.md)
    - [Projections and Distances](curves/projections-and-distances.md)
    - [Intersections](curves/intersections.md)
    - [Linear Curves](curves/linear/index.md)
        - [Line](curves/linear/line.md)
        - [Ray](curves/linear/ray.md)
        - [Segment](curves/linear/segment.md)
        - [ParameterizedLine](curves/linear/parameterized-line.md)
        - [ParameterizedSegment](curves/linear/parameterized-segment.md)
        - [ParameterizedSegmentChain](curves/linear/parameterized-segment-chain.md)
    - [Circular Curves](curves/circular/index.md)
        - [Circle](curves/circular/circle.md)
        - [Arc](curves/circular/arc.md)
        - [ParameterizedArc](curves/circular/parameterized-arc.md)
    - [Bezier Curves](curves/bezier/index.md)
        - [QuadraticBezier](curves/bezier/quadratic-bezier.md)
        - [CubicBezier](curves/bezier/cubic-bezier.md)
        - [BezierCurve](curves/bezier/bezier-curve.md)
- [Contours](contours/index.md)
    - [Circular](contours/circular/index.md)
        - [Circle](contours/circle.md)
        - [ParameterizedCircle](contours/parameterized-circle.md)
    - [Rectangular](contours/rectangular/index.md)
        - [RectangleContour](contours/rectangle-contour.md)
        - [ParameterizedRectangleContour](contours/parameterized-rectangle-contour.md)
        - [OrientedRectangleContour](contours/oriented-rectangle-contour.md)
        - [ParameterizedOrientedRectangleContour](contours/parameterized-oriented-rectangle-contour.md)
    - [Composite](contours/composite/index.md)
        - [CompositeContour](contours/composite-contour.md)
        - [ParameterizedCompositeContour](contours/parameterized-composite-contour.md)
- [Regions](regions/index.md)
    - [Circular](regions/circular/index.md)
        - [Disk](regions/disk.md)
    - [Rectangular](regions/rectangular/index.md)
        - [Rectangle](regions/rectangle.md)
        - [OrientedRectangle](regions/oriented-rectangle.md)
    - [Contour Based](regions/contour-based/index.md)
        - [ContourBasedRegion](regions/contour-based-regions.md)
- [Fields](fields/index.md)
- [Sampling](sampling/index.md)
- [Partitioning](partitioning/index.md)
- [Rasterization and Imaging](rasterization/index.md)
- [Recipes](recipes/index.md)
- [Performance](performance/index.md)
- [Testing and Robustness](testing/index.md)

## Installation

```powershell
dotnet add package Akeldov.Math.Spatial2D --version 0.7.0
```

## Target Frameworks

- .NET Standard 2.1
- .NET 6.0
