using Akeldov.Math.Spatial2D.Fields;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk
{
    internal sealed class ConstantFloatField : IFloatField
    {
        private readonly float _value;

        public ConstantFloatField(float value)
        {
            _value = value;
            DistinctValues = new[] { value };
        }

        public float Min => _value;

        public float Max => _value;

        /// <summary>
        /// Gets the read-only distinct source values used to define this field range.
        /// </summary>
        public IReadOnlyList<float> DistinctValues { get; }

        public float Sample(PointXY point)
        {
            return _value;
        }
    }
}
