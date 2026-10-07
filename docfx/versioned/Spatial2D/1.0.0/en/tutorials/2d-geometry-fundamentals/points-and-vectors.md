# Points and Vectors

In this part of the tutorial, you will describe two positions, calculate the displacement between
them, and use that displacement to construct more points. Continue in the
`Spatial2D.Fundamentals` project created in the previous step.

## Create two points

A <xref:Akeldov.Math.Spatial2D.PointXY> represents a position in continuous two-dimensional
space. Replace the contents of `Program.cs` with:

```csharp
using Akeldov.Math.Spatial2D;

var start = new PointXY(1f, 1f);
var end = new PointXY(5f, 4f);

Console.WriteLine($"Start: {start}");
Console.WriteLine($"End:   {end}");
```

Run the application:

```powershell
dotnet run
```

You should see:

```text
Start: (1, 1)
End:   (5, 4)
```

Spatial2D uses Cartesian coordinates. The coordinate values are not tied to a particular unit;
they may represent pixels, meters, or any other world unit used consistently by your application.

## Calculate a displacement

A <xref:Akeldov.Math.Spatial2D.VectorXY> represents a direction, offset, or size rather than a
position. Subtracting one point from another produces the vector from the right-hand point to the
left-hand point.

Add the following code after the point declarations:

```csharp
VectorXY displacement = end - start;
float distance = start.Distance(end);
VectorXY direction = displacement.Normalize();

Console.WriteLine($"Displacement: {displacement}");
Console.WriteLine($"Distance:     {distance}");
Console.WriteLine($"Direction:    {direction}");
```

The output is:

```text
Displacement: (4, 3)
Distance:     5
Direction:    (0.8, 0.6)
```

`displacement` keeps both the direction and the distance between the points. `direction` has the
same direction but a length of one, which is useful when you want to choose a new travel distance
independently.

## Translate a point

Adding a vector to a point translates the point. Multiplying a vector by a scalar changes its
length, so the midpoint between `start` and `end` can be constructed with half of the displacement:

```csharp
PointXY midpoint = start + displacement * 0.5f;
PointXY oneUnitPastEnd = end + direction;

Console.WriteLine($"Midpoint:          {midpoint}");
Console.WriteLine($"One unit past end: {oneUnitPastEnd}");
```

This prints:

```text
Midpoint:          (3, 2.5)
One unit past end: (5.8, 4.6)
```

The result of translating a point is another point. In contrast, subtracting two points returns a
vector. Keeping those meanings separate makes geometric expressions readable:

```csharp
VectorXY offset = end - start; // position - position = offset
PointXY moved = start + offset; // position + offset = position
```

## Compare directions

The dot product, cross product, and signed angle describe the relationship between two vectors:

```csharp
VectorXY right = VectorXY.BasisX;
VectorXY up = VectorXY.BasisY;

float dot = VectorXY.Dot(right, up);       // 0: the vectors are perpendicular
float cross = VectorXY.Cross(right, up);   // 1: up is to the left of right
float angle = VectorXY.Angle(right, up);   // PI / 2 radians
```

`VectorXY.Angle` returns radians. Positive values indicate a counterclockwise rotation from the
first vector to the second; negative values indicate a clockwise rotation.

You now have the points and directions needed to create geometry. Continue with
[Curves and Transformations](curves-and-transformations.md).
