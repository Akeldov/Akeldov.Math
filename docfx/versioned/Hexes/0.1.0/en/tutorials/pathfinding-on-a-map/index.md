# Pathfinding on a Map

Build a .NET console application that finds a minimum-cost route across a small terrain map with
Akeldov.Math.Hexes. You will assign different movement costs to plains and forests, make
water impassable, handle a missing route, and render the result in the terminal.

Install the .NET 6 SDK or later. The steps extend the same `Program.cs` file and should be
completed in order:

1. [Create a terrain map](creating-a-terrain-map.md) and choose the route endpoints.
2. [Assign transfer costs](assigning-transfer-costs.md) to plains, forests, and water.
3. [Add impassable hexes](adding-impassable-hexes.md) with positive infinity.
4. [Find a path](finding-a-path.md) with Dijkstra's algorithm.
5. [Handle an unreachable target](handling-an-unreachable-target.md) without dereferencing
   `null`.
6. [Visualize the route](visualizing-the-route.md) on the terrain map.

The tutorial uses a rectangular `OddR` topology. Pathfinding depends on topology and costs, not
on a hex radius or world-space geometry.

Start with [Creating a Terrain Map](creating-a-terrain-map.md).
