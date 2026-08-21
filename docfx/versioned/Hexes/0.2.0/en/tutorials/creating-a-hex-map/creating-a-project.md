# Creating a Project

In this part of the tutorial, you will create a .NET 6 console application and reference
Akeldov.Math.Hexes 0.2.0. Every later step will extend the same `Program.cs` file.

## Create the console application

Run these commands in the directory that should contain the project:

```powershell
dotnet new console --framework net6.0 --name HexMap.Tutorial
cd HexMap.Tutorial
```

The command creates a project with implicit namespace imports and nullable analysis enabled.

## Install Hexes

Add the specified package version:

```powershell
dotnet add package Akeldov.Math.Hexes --version 0.2.0
```

The package restores a compatible Akeldov.Math.Spatial2D version, whose types are used for
two-dimensional indexes and geometry.

Replace `Program.cs` with a minimal check:

```csharp
using Akeldov.Math.Hexes;

Console.WriteLine($"Hexes layout: {Layout.OddR}");
```

Run the application:

```powershell
dotnet run
```

Expected output:

```text
Hexes layout: OddR
```

If the application builds and prints the <xref:Akeldov.Math.Hexes.Layout> name, the package is
referenced correctly. Continue with [Choosing a Layout](choosing-a-layout.md).
