using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    internal readonly struct HexGridDefinition
    {
        private const float ThreeHalves = 1.5f;

        private HexGridDefinition(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            float hexRadius,
            VectorXY gridOrigin,
            VectorXY gridSize,
            VectorXYInt resolution)
        {
            HexResolution = new VectorXYInt(hexWidth, hexHeight);
            Layout = layout;
            HexOrigin = hexOrigin;
            HexApothem = hexApothem;
            HexRadius = hexRadius;
            Origin = gridOrigin;
            Size = gridSize;
            CellSize = new VectorXY(gridSize.X / resolution.X, gridSize.Y / resolution.Y);
            Resolution = resolution;
            ResolutionX = resolution.X;
            ResolutionY = resolution.Y;
            Count = checked(resolution.X * resolution.Y);
        }

        public VectorXYInt HexResolution { get; }
        public Layout Layout { get; }
        public VectorXY HexOrigin { get; }
        public float HexApothem { get; }
        public float HexRadius { get; }
        public VectorXY Origin { get; }
        public VectorXY Size { get; }
        public VectorXY CellSize { get; }
        public VectorXYInt Resolution { get; }
        public int ResolutionX { get; }
        public int ResolutionY { get; }
        public int Count { get; }

        public static HexGridDefinition Create(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            VectorXYInt resolution)
        {
            ValidateHexGrid(hexWidth, hexHeight, hexOrigin, hexApothem, resolution);

            float hexRadius = hexApothem.ConvertHexApothemToRadius();
            HexGridBounds bounds = GetBounds(hexWidth, hexHeight, layout, hexOrigin, hexApothem, hexRadius);

            return new HexGridDefinition(
                hexWidth,
                hexHeight,
                layout,
                hexOrigin,
                hexApothem,
                hexRadius,
                new VectorXY(bounds.MinX, bounds.MinY),
                new VectorXY(bounds.Width, bounds.Height),
                resolution);
        }

        public static HexGridDefinition Create(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            VectorXY gridOrigin,
            VectorXY gridSize,
            VectorXYInt resolution)
        {
            ValidateHexGrid(hexWidth, hexHeight, hexOrigin, hexApothem, resolution);

            if (!gridOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(gridOrigin), gridOrigin, "Grid origin components must be finite.");

            if (!gridSize.IsFinite || gridSize.X <= 0f || gridSize.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(gridSize), gridSize, "Grid size components must be finite and positive.");

            return new HexGridDefinition(
                hexWidth,
                hexHeight,
                layout,
                hexOrigin,
                hexApothem,
                hexApothem.ConvertHexApothemToRadius(),
                gridOrigin,
                gridSize,
                resolution);
        }

        public VectorXY GetCellCenterUnchecked(int x, int y)
        {
            return new VectorXY(
                Origin.X + (x + 0.5f) * CellSize.X,
                Origin.Y + (y + 0.5f) * CellSize.Y);
        }

        public int GetFlatIndex(VectorXYInt index) => index.Y * ResolutionX + index.X;

        private static void ValidateHexGrid(
            int hexWidth,
            int hexHeight,
            VectorXY hexOrigin,
            float hexApothem,
            VectorXYInt resolution)
        {
            ThrowIfHexDimensionIsNotPositive(hexWidth, nameof(hexWidth));
            ThrowIfHexDimensionIsNotPositive(hexHeight, nameof(hexHeight));

            if (!hexOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexOrigin), hexOrigin, "Hex origin components must be finite.");

            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");
        }

        private static void ThrowIfHexDimensionIsNotPositive(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, "Hex grid dimensions must be positive.");
        }

        private static HexGridBounds GetBounds(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            float hexRadius)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return GetOddRowLayoutBounds(hexWidth, hexHeight, hexOrigin, hexApothem, hexRadius);
                case Layout.EvenR:
                    return GetEvenRowLayoutBounds(hexWidth, hexHeight, hexOrigin, hexApothem, hexRadius);
                case Layout.OddQ:
                    return GetOddColumnLayoutBounds(hexWidth, hexHeight, hexOrigin, hexApothem, hexRadius);
                case Layout.EvenQ:
                    return GetEvenColumnLayoutBounds(hexWidth, hexHeight, hexOrigin, hexApothem, hexRadius);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static HexGridBounds GetOddRowLayoutBounds(
            int hexWidth,
            int hexHeight,
            VectorXY origin,
            float apothem,
            float radius)
        {
            float shiftedRowExtra = hexHeight > 1 ? apothem : 0f;
            float halfWidth = Geometry.Constants.Cos30Deg * radius;

            return new HexGridBounds(
                origin.X - halfWidth,
                origin.Y - radius,
                origin.X + 2f * apothem * (hexWidth - 1) + shiftedRowExtra + halfWidth,
                origin.Y + ThreeHalves * radius * (hexHeight - 1) + radius);
        }

        private static HexGridBounds GetEvenRowLayoutBounds(
            int hexWidth,
            int hexHeight,
            VectorXY origin,
            float apothem,
            float radius)
        {
            float shiftedRowExtra = hexHeight > 1 ? apothem : 0f;
            float halfWidth = Geometry.Constants.Cos30Deg * radius;

            return new HexGridBounds(
                origin.X - shiftedRowExtra - halfWidth,
                origin.Y - radius,
                origin.X + 2f * apothem * (hexWidth - 1) + halfWidth,
                origin.Y + ThreeHalves * radius * (hexHeight - 1) + radius);
        }

        private static HexGridBounds GetOddColumnLayoutBounds(
            int hexWidth,
            int hexHeight,
            VectorXY origin,
            float apothem,
            float radius)
        {
            float shiftedColumnExtra = hexWidth > 1 ? apothem : 0f;
            float halfHeight = Geometry.Constants.Sin60Deg * radius;

            return new HexGridBounds(
                origin.X - radius,
                origin.Y - halfHeight,
                origin.X + ThreeHalves * radius * (hexWidth - 1) + radius,
                origin.Y + 2f * apothem * (hexHeight - 1) + shiftedColumnExtra + halfHeight);
        }

        private static HexGridBounds GetEvenColumnLayoutBounds(
            int hexWidth,
            int hexHeight,
            VectorXY origin,
            float apothem,
            float radius)
        {
            float shiftedColumnExtra = hexWidth > 1 ? apothem : 0f;
            float halfHeight = Geometry.Constants.Sin60Deg * radius;

            return new HexGridBounds(
                origin.X - radius,
                origin.Y - shiftedColumnExtra - halfHeight,
                origin.X + ThreeHalves * radius * (hexWidth - 1) + radius,
                origin.Y + 2f * apothem * (hexHeight - 1) + halfHeight);
        }

        private readonly struct HexGridBounds
        {
            public HexGridBounds(float minX, float minY, float maxX, float maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }

            public float MinX { get; }
            public float MinY { get; }
            public float MaxX { get; }
            public float MaxY { get; }
            public float Width => MaxX - MinX;
            public float Height => MaxY - MinY;
        }
    }
}
