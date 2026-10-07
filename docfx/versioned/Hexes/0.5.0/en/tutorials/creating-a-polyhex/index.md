# Creating a Polyhex

Build a .NET console application that turns a Q/R cell mask into an immutable polyhex and then
into a rasterized Spatial2D region with a hole. Along the way, you will inspect cell vertices and
edges, add a physical hex radius, and export the finished shape as a PNG image.

Install the .NET 6 SDK or later. The steps extend the same `Program.cs` file and should be
completed in order:

1. [Define the mask](defining-the-mask.md) in local Q/R coordinates.
2. [Build the polyhex topology](building-polyhex-topology.md) as an immutable value.
3. [Obtain edges and vertices](obtaining-edges-and-vertices.md) for an occupied cell.
4. [Convert to Spatial2D geometry](converting-to-spatial2d-geometry.md) by assigning a radius.
5. [Create a region](creating-a-region.md) from the complete polyhex boundary.
6. [Rasterize the result](rasterizing-the-result.md) and save `polyhex.png`.

The example deliberately contains an unoccupied cell surrounded by occupied neighbors. It shows
how the even-odd fill rule preserves holes when topology becomes a spatial region.

Start with [Defining the Mask](defining-the-mask.md).
