namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal interface ISpatialRasterSampler<TValue>
    {
        TValue Sample(PointXY point);
    }
}
