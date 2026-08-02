# Creating a Project

In this tutorial, you will build a small console application with
`Akeldov.Math.Spatial2D`. Install a .NET 6 or later SDK before starting.

## Create the console project

Run these commands in a terminal:

```powershell
dotnet new console --name Spatial2D.Fundamentals
Set-Location Spatial2D.Fundamentals
dotnet add package Akeldov.Math.Spatial2D --version 0.8.0
```

The package supports .NET 6 and .NET Standard 2.1, so it can also be used from other compatible
.NET projects.

## Verify the setup

Replace the contents of `Program.cs` with:

```csharp
using Akeldov.Math.Spatial2D;

var point = new PointXY(2f, 3f);

Console.WriteLine($"Spatial2D is ready: ({point.X}, {point.Y})");
```

Run the application:

```powershell
dotnet run
```

The output should be:

```text
Spatial2D is ready: (2, 3)
```

The project is ready. Continue with [Points and Vectors](points-and-vectors.md).
