namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal static class SpatialRasterizationCore<TValue>
    {
        internal static SpatialRaster<TValue> Rasterize<TSampler>(RasterGeometry grid, TSampler sampler, string gridParameterName)
            where TSampler : struct, ISpatialRasterSampler<TValue>
        {
            int cellCount = RasterGeometryValidation.ValidateAndGetCellCount(grid, gridParameterName);
            var values = new TValue[cellCount];
            VectorXY cellSize = grid.CellSize;
            float firstX = grid.Origin.X + cellSize.X * 0.5f;
            float firstY = grid.Origin.Y + cellSize.Y * 0.5f;

            int valueIndex = 0;
            for (int y = 0; y < grid.Resolution.Y; y++)
            {
                float pointY = firstY + y * cellSize.Y;
                for (int x = 0; x < grid.Resolution.X; x++)
                {
                    PointXY point = new PointXY(firstX + x * cellSize.X, pointY);
                    values[valueIndex++] = sampler.Sample(point);
                }
            }

            return new SpatialRaster<TValue>(grid, values);
        }
    }
}
