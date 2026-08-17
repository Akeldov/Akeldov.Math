# Defining the Mask

In this part of the tutorial, you will create a console application and describe a shape with a
rectangular Boolean mask. `true` means that the Q/R cell belongs to the polyhex.

## Create the project

Run these commands in the directory that should contain the project:

```powershell
dotnet new console --framework net6.0 --name Polyhex.Tutorial
cd Polyhex.Tutorial
dotnet add package Akeldov.Math.Hexes --version 0.1.0
```

## Add the mask

Replace `Program.cs` with this code:

```csharp
using Akeldov.Math.Hexes.Topology;

bool[,] mask =
{
    { false, true,  true,  false }, // q = 0, r = 0..3
    { true,  true,  true,  true  }, // q = 1, r = 0..3
    { true,  true,  false, true  }, // q = 2, r = 0..3
    { false, true,  true,  false }  // q = 3, r = 0..3
};

Console.WriteLine(
    $"Mask: {mask.GetLength(0)} x {mask.GetLength(1)}");
```

Run the application:

```powershell
dotnet run
```

Expected output:

```text
Mask: 4 x 4
```

The first array dimension is Q and the second is R. For example, `mask[2, 2]` is the false cell
in the interior of the shape. Its derived S coordinate is `-2 - 2`, but S is not a third array
dimension.

This local Q/R mask does not use a <xref:Akeldov.Math.Hexes.Layout>. Layout becomes relevant only
when the shape is placed in two-dimensional coordinate space.

Continue with [Building Polyhex Topology](building-polyhex-topology.md).
