# Creating a Terrain Map

In this part of the tutorial, you will create a console application and store a 7-by-5 terrain
map. A period represents a plain, `F` a forest, and `W` water.

## Create the project

Run these commands in the directory that should contain the project:

```powershell
dotnet new console --framework net6.0 --name HexPathfinding.Tutorial
cd HexPathfinding.Tutorial
dotnet add package Akeldov.Math.Hexes --version 0.2.0
```

Akeldov.Math.Hexes brings in the compatible Akeldov.Math.Spatial2D package, which supplies the
`VectorXYInt` index type used below.

## Store the terrain

Replace `Program.cs` with this code:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

string[] terrainRows =
{
    ".......",
    "..FFF..",
    "..FWF..",
    "..FFF..",
    "......."
};

var topology = new HexMapTopology(
    width: terrainRows[0].Length,
    height: terrainRows.Length,
    layout: Layout.OddR);

var terrain = new HexMap<char>(
    topology,
    string.Concat(terrainRows).ToCharArray());

var start = new VectorXYInt(0, 2);
var goal = new VectorXYInt(6, 2);

Console.WriteLine(
    $"Map: {topology.Resolution.X} x {topology.Resolution.Y}; " +
    $"route: {start} -> {goal}");
```

Run the application:

```powershell
dotnet run
```

Expected output:

```text
Map: 7 x 5; route: (0, 2) -> (6, 2)
```

`HexMap<char>` stores the concatenated rows in row-major order: `X` advances first, followed by
`Y`. The topology owns the map dimensions and neighbor rules, while `start` and `goal` identify
the endpoints of the future route.

Keep these declarations in `Program.cs` and continue with
[Assigning Transfer Costs](assigning-transfer-costs.md).
