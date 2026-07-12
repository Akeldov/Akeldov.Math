using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Initializes a new instance of the HexCenterMap type.
    /// </summary>
    public sealed class HexCenterMap : IHexMap<PointXY>
    {
        private readonly PointXY[] _values;

        /// <summary>
        /// Initializes a new instance of the HexCenterMap type.
        /// </summary>
        /// <param name="width">The Width value.</param>
        /// <param name="height">The Height value.</param>
        /// <param name="origin">The Origin value.</param>
        /// <param name="radius">The hex radius from center to vertex.</param>
        /// <param name="layout">The Layout value.</param>
        public HexCenterMap(
            int width,
            int height,
            VectorXY origin,
            float radius,
            Layout layout)
            : this(new HexMapGeometry(width, height, origin, radius, layout))
        {
        }

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
            Width = geometry.Topology.Resolution.X;
            Height = geometry.Topology.Resolution.Y;
            Origin = geometry.Origin;
            Apothem = geometry.Apothem;
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
        /// Initializes a new instance of the HexCenterMap type.
        /// </summary>
        /// <param name="radius">The Radius value.</param>
        /// <param name="width">The Width value.</param>
        /// <param name="height">The Height value.</param>
        /// <param name="layout">The Layout value.</param>
        public HexCenterMap(
            int width,
            int height,
            float radius,
            Layout layout)
            : this(new HexMapGeometry(width, height, radius, layout))
        {
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
        /// Gets the Width value.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the Height value.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the Origin value.
        /// </summary>
        public VectorXY Origin { get; }

        /// <summary>
        /// Gets the Apothem value.
        /// </summary>
        public float Apothem { get; }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public PointXY this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Width ||
                    index.Y < 0 || index.Y >= Height)
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
            for (int y = 0; y < Height; y++)
            {
                var rowStart = y * Width;
                var rowIsShifted = ((y & 1) == 0) == evenRowsAreShifted;
                var xShift = GetShiftRelativeToOrigin(rowIsShifted, evenRowsAreShifted);
                var centerY = Origin.Y + 1.5f * radius * y;

                for (int x = 0; x < Width; x++)
                {
                    _values[rowStart + x] = new PointXY(
                        Origin.X + x * 2f * Apothem + xShift,
                        centerY);
                }
            }
        }

        private void FillColumnLayoutCenters(bool evenColumnsAreShifted, float radius)
        {
            for (int y = 0; y < Height; y++)
            {
                var rowStart = y * Width;
                var baseY = Origin.Y + y * 2f * Apothem;

                for (int x = 0; x < Width; x++)
                {
                    var columnIsShifted = ((x & 1) == 0) == evenColumnsAreShifted;
                    var yShift = GetShiftRelativeToOrigin(columnIsShifted, evenColumnsAreShifted);

                    _values[rowStart + x] = new PointXY(
                        Origin.X + 1.5f * radius * x,
                        baseY + yShift);
                }
            }
        }

        private float GetShiftRelativeToOrigin(bool indexIsShifted, bool originIndexIsShifted)
        {
            if (indexIsShifted == originIndexIsShifted)
                return 0f;

            return indexIsShifted ? Apothem : -Apothem;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Width + index.X;

    }
}
