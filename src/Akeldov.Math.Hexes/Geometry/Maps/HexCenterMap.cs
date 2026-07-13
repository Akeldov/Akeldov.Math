using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Initializes a new instance of the HexCenterMap type.
    /// </summary>
    public sealed class HexCenterMap : ISpatialHexMap<PointXY>
    {
        private readonly PointXY[] _values;

        /// <summary>
        /// Initializes a new instance of the HexCenterMap type.
        /// </summary>
        /// <param name="geometry">The geometry value.</param>
        public HexCenterMap(HexMapGeometry geometry)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            float radius = geometry.Radius;

            Geometry = geometry;
            _values = new PointXY[geometry.Topology.Count];

            switch (geometry.Topology.Layout)
            {
                case Layout.OddR:
                    FillRowLayoutCenters(false, radius);
                    break;
                case Layout.EvenR:
                    FillRowLayoutCenters(true, radius);
                    break;
                case Layout.OddQ:
                    FillColumnLayoutCenters(false, radius);
                    break;
                case Layout.EvenQ:
                    FillColumnLayoutCenters(true, radius);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry));
            }
        }

        /// <summary>
        /// Gets the Geometry value.
        /// </summary>
        public HexMapGeometry Geometry { get; }

        /// <summary>
        /// Gets the map topology.
        /// </summary>
        public HexMapTopology Topology => Geometry.Topology;

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public PointXY this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Topology.Resolution.X ||
                    index.Y < 0 || index.Y >= Topology.Resolution.Y)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _values[GetFlatIndex(index)];
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public PointXY this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        private void FillRowLayoutCenters(bool evenRowsAreShifted, float radius)
        {
            for (int y = 0; y < Topology.Resolution.Y; y++)
            {
                var rowStart = y * Topology.Resolution.X;
                var rowIsShifted = ((y & 1) == 0) == evenRowsAreShifted;
                var xShift = GetShiftRelativeToOrigin(rowIsShifted, evenRowsAreShifted);
                var centerY = Geometry.Origin.Y + 1.5f * radius * y;

                for (int x = 0; x < Topology.Resolution.X; x++)
                {
                    _values[rowStart + x] = new PointXY(
                        Geometry.Origin.X + x * 2f * Geometry.Apothem + xShift,
                        centerY);
                }
            }
        }

        private void FillColumnLayoutCenters(bool evenColumnsAreShifted, float radius)
        {
            for (int y = 0; y < Topology.Resolution.Y; y++)
            {
                var rowStart = y * Topology.Resolution.X;
                var baseY = Geometry.Origin.Y + y * 2f * Geometry.Apothem;

                for (int x = 0; x < Topology.Resolution.X; x++)
                {
                    var columnIsShifted = ((x & 1) == 0) == evenColumnsAreShifted;
                    var yShift = GetShiftRelativeToOrigin(columnIsShifted, evenColumnsAreShifted);

                    _values[rowStart + x] = new PointXY(
                        Geometry.Origin.X + 1.5f * radius * x,
                        baseY + yShift);
                }
            }
        }

        private float GetShiftRelativeToOrigin(bool indexIsShifted, bool originIndexIsShifted)
        {
            if (indexIsShifted == originIndexIsShifted)
                return 0f;

            return indexIsShifted ? Geometry.Apothem : -Geometry.Apothem;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Topology.Resolution.X + index.X;

    }
}
