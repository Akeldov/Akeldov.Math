using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    internal static class HexVertexTripletGridBuilder
    {
        private const float Sqrt3Over3 = 0.5773502588f;
        private const float OneThird = 0.3333333333f;
        private const float TwoThirds = 0.6666666666f;

        public static void Fill(
            HexGridDefinition grid,
            Triplet<VectorXYInt>[] indexTriplets,
            Triplet<float>[] barycentricCoordinates,
            Triplet<byte>[] chromaticIndices,
            bool[] hasHex)
        {
            switch (grid.Layout)
            {
                case Layout.OddR:
                    FillRowLayout(grid, false, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex);
                    break;
                case Layout.EvenR:
                    FillRowLayout(grid, true, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex);
                    break;
                case Layout.OddQ:
                    FillColumnLayout(grid, false, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex);
                    break;
                case Layout.EvenQ:
                    FillColumnLayout(grid, true, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(grid.Layout));
            }
        }

        private static void FillRowLayout(
            HexGridDefinition grid,
            bool evenRowsAreShifted,
            Triplet<VectorXYInt>[] indexTriplets,
            Triplet<float>[] barycentricCoordinates,
            Triplet<byte>[] chromaticIndices,
            bool[] hasHex)
        {
            int resolutionX = grid.ResolutionX;
            int resolutionY = grid.ResolutionY;
            int hexWidth = grid.HexResolution.X;
            int hexHeight = grid.HexResolution.Y;
            float originX = grid.Origin.X;
            float originY = grid.Origin.Y;
            float hexOriginX = grid.HexOrigin.X;
            float hexOriginY = grid.HexOrigin.Y;
            float cellSizeX = grid.CellSize.X;
            float cellSizeY = grid.CellSize.Y;
            float hexRadius = grid.HexRadius;
            VectorXY[] normalizedHexVertexes = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertexes(grid.Layout);
            var pointyQNumeratorByX = new float[resolutionX];

            for (int x = 0; x < resolutionX; x++)
            {
                float shiftedX = originX + (x + 0.5f) * cellSizeX - hexOriginX;
                pointyQNumeratorByX[x] = Sqrt3Over3 * shiftedX;
            }

            for (int y = 0; y < resolutionY; y++)
            {
                int rowStart = y * resolutionX;
                float pointY = originY + (y + 0.5f) * cellSizeY;
                float shiftedY = pointY - hexOriginY;
                float qYNumerator = OneThird * shiftedY;
                float r = TwoThirds * shiftedY / hexRadius;

                for (int x = 0; x < resolutionX; x++)
                {
                    float q = (pointyQNumeratorByX[x] - qYNumerator) / hexRadius;
                    RoundPointyTopAxial(q, r, out int qInt, out int rInt);

                    int hexX = evenRowsAreShifted
                        ? qInt + ((rInt + (rInt & 1)) / 2)
                        : qInt + ((rInt - (rInt & 1)) / 2);

                    int flatIndex = rowStart + x;

                    if ((uint)hexX >= (uint)hexWidth || (uint)rInt >= (uint)hexHeight)
                        continue;

                    VectorXY point = new VectorXY(originX + (x + 0.5f) * cellSizeX, pointY);
                    WriteHit(grid, flatIndex, point, new VectorXYInt(hexX, rInt), normalizedHexVertexes, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex);
                }
            }
        }

        private static void FillColumnLayout(
            HexGridDefinition grid,
            bool evenColumnsAreShifted,
            Triplet<VectorXYInt>[] indexTriplets,
            Triplet<float>[] barycentricCoordinates,
            Triplet<byte>[] chromaticIndices,
            bool[] hasHex)
        {
            int resolutionX = grid.ResolutionX;
            int resolutionY = grid.ResolutionY;
            int hexWidth = grid.HexResolution.X;
            int hexHeight = grid.HexResolution.Y;
            float originX = grid.Origin.X;
            float originY = grid.Origin.Y;
            float hexOriginX = grid.HexOrigin.X;
            float hexOriginY = grid.HexOrigin.Y;
            float cellSizeX = grid.CellSize.X;
            float cellSizeY = grid.CellSize.Y;
            float hexRadius = grid.HexRadius;
            VectorXY[] normalizedHexVertexes = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertexes(grid.Layout);
            var flatQNumeratorByX = new float[resolutionX];
            var flatRNumeratorByX = new float[resolutionX];

            for (int x = 0; x < resolutionX; x++)
            {
                float shiftedX = originX + (x + 0.5f) * cellSizeX - hexOriginX;
                flatQNumeratorByX[x] = TwoThirds * shiftedX;
                flatRNumeratorByX[x] = OneThird * shiftedX;
            }

            for (int y = 0; y < resolutionY; y++)
            {
                int rowStart = y * resolutionX;
                float pointY = originY + (y + 0.5f) * cellSizeY;
                float shiftedY = pointY - hexOriginY;
                float rYNumerator = Sqrt3Over3 * shiftedY;

                for (int x = 0; x < resolutionX; x++)
                {
                    float q = flatQNumeratorByX[x] / hexRadius;
                    float r = (rYNumerator - flatRNumeratorByX[x]) / hexRadius;
                    RoundFlatTopAxial(q, r, out int qInt, out int rInt);

                    int hexY = evenColumnsAreShifted
                        ? rInt + ((qInt + (qInt & 1)) / 2)
                        : rInt + ((qInt - (qInt & 1)) / 2);

                    int flatIndex = rowStart + x;

                    if ((uint)qInt >= (uint)hexWidth || (uint)hexY >= (uint)hexHeight)
                        continue;

                    VectorXY point = new VectorXY(originX + (x + 0.5f) * cellSizeX, pointY);
                    WriteHit(grid, flatIndex, point, new VectorXYInt(qInt, hexY), normalizedHexVertexes, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex);
                }
            }
        }

        private static void WriteHit(
            HexGridDefinition grid,
            int flatIndex,
            VectorXY point,
            VectorXYInt mainIndex,
            VectorXY[] normalizedHexVertexes,
            Triplet<VectorXYInt>[] indexTriplets,
            Triplet<float>[] barycentricCoordinates,
            Triplet<byte>[] chromaticIndices,
            bool[] hasHex)
        {
            VectorXY mainCenter = mainIndex.GetHexCenter(grid.HexApothem, grid.HexRadius, grid.HexOrigin, grid.Layout);
            HexVertex hexVertex = (HexVertex)GetClosestVertexIndex(point, grid.HexRadius, mainCenter, normalizedHexVertexes);
            Triplet<VectorXYInt> indexTriplet = mainIndex.GetAdjacentTriplet(hexVertex, grid.Layout);
            bool hasLeft = ContainsHex(grid, indexTriplet.Left);
            bool hasRight = ContainsHex(grid, indexTriplet.Right);
            Triplet<VectorXYInt> boundedIndexTriplet = new Triplet<VectorXYInt>(
                indexTriplet.Main,
                hasLeft ? indexTriplet.Left : indexTriplet.Main,
                hasRight ? indexTriplet.Right : indexTriplet.Main);

            hasHex[flatIndex] = true;

            if (indexTriplets != null)
                indexTriplets[flatIndex] = boundedIndexTriplet;

            if (chromaticIndices != null)
                chromaticIndices[flatIndex] = boundedIndexTriplet.GetChromaticTriplet(grid.Layout);

            if (barycentricCoordinates != null)
            {
                barycentricCoordinates[flatIndex] = GetBoundedBarycentricCoordinates(
                    grid,
                    point,
                    indexTriplet,
                    mainCenter,
                    hasLeft,
                    hasRight);
            }
        }

        private static Triplet<float> GetBoundedBarycentricCoordinates(
            HexGridDefinition grid,
            VectorXY point,
            Triplet<VectorXYInt> indexTriplet,
            VectorXY mainCenter,
            bool hasLeft,
            bool hasRight)
        {
            VectorXY leftCenter = indexTriplet.Left.GetHexCenter(grid.HexApothem, grid.HexRadius, grid.HexOrigin, grid.Layout);
            VectorXY rightCenter = indexTriplet.Right.GetHexCenter(grid.HexApothem, grid.HexRadius, grid.HexOrigin, grid.Layout);
            Triplet<float> barycentric = point.BarycentricCoordinates(mainCenter, leftCenter, rightCenter);

            if (hasLeft && hasRight)
                return barycentric;

            if (hasLeft)
            {
                float sum = barycentric.Main + barycentric.Left;
                return sum <= GeometryConstants.GeometryEpsilon
                    ? new Triplet<float>(1f, 0f, 0f)
                    : new Triplet<float>(barycentric.Main / sum, barycentric.Left / sum, 0f);
            }

            if (hasRight)
            {
                float sum = barycentric.Main + barycentric.Right;
                return sum <= GeometryConstants.GeometryEpsilon
                    ? new Triplet<float>(1f, 0f, 0f)
                    : new Triplet<float>(barycentric.Main / sum, 0f, barycentric.Right / sum);
            }

            return new Triplet<float>(1f, 0f, 0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsHex(HexGridDefinition grid, VectorXYInt index)
        {
            return (uint)index.X < (uint)grid.HexResolution.X &&
                (uint)index.Y < (uint)grid.HexResolution.Y;
        }

        private static int GetClosestVertexIndex(
            VectorXY point,
            float radius,
            VectorXY hexCenter,
            VectorXY[] normalizedHexVertexes)
        {
            float minSquaredDistance = float.MaxValue;
            int closestVertexIndex = 0;

            for (int i = 0; i < 6; i++)
            {
                VectorXY vertex = hexCenter + normalizedHexVertexes[i] * radius;
                float squaredDistance = SquaredDistance(point, vertex);

                if (squaredDistance < minSquaredDistance)
                {
                    minSquaredDistance = squaredDistance;
                    closestVertexIndex = i;
                }
            }

            return closestVertexIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SquaredDistance(VectorXY left, VectorXY right)
        {
            float x = left.X - right.X;
            float y = left.Y - right.Y;
            return x * x + y * y;
        }

        private static void RoundPointyTopAxial(float q, float r, out int qInt, out int rInt)
        {
            float s = -q - r;

            qInt = (int)MathF.Round(q);
            rInt = (int)MathF.Round(r);
            int sInt = (int)MathF.Round(s);

            float qDiff = MathF.Abs(qInt - q);
            float rDiff = MathF.Abs(rInt - r);
            float sDiff = MathF.Abs(sInt - s);

            if (qDiff > rDiff && qDiff > sDiff)
                qInt = -rInt - sInt;
            else if (rDiff > sDiff)
                rInt = -qInt - sInt;
        }

        private static void RoundFlatTopAxial(float q, float r, out int qInt, out int rInt)
        {
            float s = -q - r;

            qInt = (int)MathF.Round(q);
            rInt = (int)MathF.Round(r);
            int sInt = (int)MathF.Round(s);

            float qDiff = MathF.Abs(qInt - q);
            float rDiff = MathF.Abs(rInt - r);
            float sDiff = MathF.Abs(sInt - s);

            if (rDiff > qDiff && rDiff > sDiff)
                rInt = -qInt - sInt;
            else if (qDiff > sDiff)
                qInt = -rInt - sInt;
        }
    }
}
