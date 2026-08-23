using System;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal readonly struct PoissonDiskPointSampleCollectionDistanceRasterSampler<TValue> : ISpatialRasterSampler<TValue>
    {
        private readonly PoissonDiskPointSample[] _samples;
        private readonly Func<PoissonDiskPointSample, float, TValue> _sampleDistanceToValue;

        public PoissonDiskPointSampleCollectionDistanceRasterSampler(
            PoissonDiskPointSample[] samples,
            Func<PoissonDiskPointSample, float, TValue> sampleDistanceToValue)
        {
            _samples = samples;
            _sampleDistanceToValue = sampleDistanceToValue;
        }

        public TValue Sample(PointXY point)
        {
            PoissonDiskPointSample nearestSample = FindNearestSample(point, out float distance);
            return _sampleDistanceToValue(nearestSample, distance);
        }

        private PoissonDiskPointSample FindNearestSample(PointXY point, out float distance)
        {
            PoissonDiskPointSample nearestSample = _samples[0];
            float nearestDistanceSquared = DistanceSquared(point, nearestSample.Point);

            for (int i = 1; i < _samples.Length; i++)
            {
                PoissonDiskPointSample sample = _samples[i];
                float distanceSquared = DistanceSquared(point, sample.Point);
                if (distanceSquared < nearestDistanceSquared)
                {
                    nearestSample = sample;
                    nearestDistanceSquared = distanceSquared;
                }
            }

            distance = MathF.Sqrt(nearestDistanceSquared);
            return nearestSample;
        }

        private static float DistanceSquared(PointXY left, PointXY right)
        {
            float dx = left.X - right.X;
            float dy = left.Y - right.Y;
            return dx * dx + dy * dy;
        }
    }
}
