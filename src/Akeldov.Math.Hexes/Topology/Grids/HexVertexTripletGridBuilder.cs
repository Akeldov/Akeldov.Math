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
            bool[] hasHex,
            HexVertexTripletGridFillMode fillMode)
        {
            ThrowIfFillModeIsUnsupported(fillMode);

            switch (grid.Layout)
            {
                case Layout.OddR:
                    FillRowLayout(grid, false, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex, fillMode);
                    break;
                case Layout.EvenR:
                    FillRowLayout(grid, true, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex, fillMode);
                    break;
                case Layout.OddQ:
                    FillColumnLayout(grid, false, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex, fillMode);
                    break;
                case Layout.EvenQ:
                    FillColumnLayout(grid, true, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex, fillMode);
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
            bool[] hasHex,
            HexVertexTripletGridFillMode fillMode)
        {
            int resolutionX = grid.ResolutionX;
            int resolutionY = grid.ResolutionY;
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

                    bool hasMain = ContainsHex(grid, hexX, rInt);

                    if (!hasMain && fillMode == HexVertexTripletGridFillMode.HitHexesOnly)
                        continue;

                    VectorXY point = new VectorXY(originX + (x + 0.5f) * cellSizeX, pointY);
                    WriteCandidate(
                        grid,
                        flatIndex,
                        point,
                        new VectorXYInt(hexX, rInt),
                        hasMain,
                        normalizedHexVertexes,
                        indexTriplets,
                        barycentricCoordinates,
                        chromaticIndices,
                        hasHex,
                        fillMode);
                }
            }
        }

        private static void FillColumnLayout(
            HexGridDefinition grid,
            bool evenColumnsAreShifted,
            Triplet<VectorXYInt>[] indexTriplets,
            Triplet<float>[] barycentricCoordinates,
            Triplet<byte>[] chromaticIndices,
            bool[] hasHex,
            HexVertexTripletGridFillMode fillMode)
        {
            int resolutionX = grid.ResolutionX;
            int resolutionY = grid.ResolutionY;
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

                    bool hasMain = ContainsHex(grid, qInt, hexY);

                    if (!hasMain && fillMode == HexVertexTripletGridFillMode.HitHexesOnly)
                        continue;

                    VectorXY point = new VectorXY(originX + (x + 0.5f) * cellSizeX, pointY);
                    WriteCandidate(
                        grid,
                        flatIndex,
                        point,
                        new VectorXYInt(qInt, hexY),
                        hasMain,
                        normalizedHexVertexes,
                        indexTriplets,
                        barycentricCoordinates,
                        chromaticIndices,
                        hasHex,
                        fillMode);
                }
            }
        }

        private static void WriteCandidate(
            HexGridDefinition grid,
            int flatIndex,
            VectorXY point,
            VectorXYInt mainIndex,
            bool hasMain,
            VectorXY[] normalizedHexVertexes,
            Triplet<VectorXYInt>[] indexTriplets,
            Triplet<float>[] barycentricCoordinates,
            Triplet<byte>[] chromaticIndices,
            bool[] hasHex,
            HexVertexTripletGridFillMode fillMode)
        {
            if (TryCreateCandidate(grid, point, mainIndex, hasMain, normalizedHexVertexes, barycentricCoordinates != null, out Candidate candidate))
            {
                WriteCandidate(flatIndex, candidate, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex, grid.Layout);
                return;
            }

            if (fillMode == HexVertexTripletGridFillMode.FillEmptyCells &&
                TryCreateNearbyCandidate(grid, point, mainIndex, normalizedHexVertexes, barycentricCoordinates != null, out candidate))
            {
                WriteCandidate(flatIndex, candidate, indexTriplets, barycentricCoordinates, chromaticIndices, hasHex, grid.Layout);
            }
        }

        private static bool TryCreateCandidate(
            HexGridDefinition grid,
            VectorXY point,
            VectorXYInt mainIndex,
            bool hasMain,
            VectorXY[] normalizedHexVertexes,
            bool includeBarycentricCoordinates,
            out Candidate candidate)
        {
            VectorXY mainCenter = mainIndex.GetHexCenter(grid.HexApothem, grid.HexRadius, grid.HexOrigin, grid.Layout);
            HexVertex hexVertex = (HexVertex)GetClosestVertexIndex(
                point,
                grid.HexRadius,
                mainCenter,
                normalizedHexVertexes,
                out float closestVertexSquaredDistance);
            Triplet<VectorXYInt> indexTriplet = mainIndex.GetAdjacentTriplet(hexVertex, grid.Layout);
            bool hasLeft = ContainsHex(grid, indexTriplet.Left);
            bool hasRight = ContainsHex(grid, indexTriplet.Right);
            bool hasAny = hasMain || hasLeft || hasRight;

            if (!hasAny)
            {
                candidate = default;
                return false;
            }

            VectorXYInt fallbackIndex = GetFallbackIndex(indexTriplet, hasMain, hasLeft);
            Triplet<VectorXYInt> boundedIndexTriplet = new Triplet<VectorXYInt>(
                hasMain ? indexTriplet.Main : fallbackIndex,
                hasLeft ? indexTriplet.Left : fallbackIndex,
                hasRight ? indexTriplet.Right : fallbackIndex);
            Triplet<float> boundedBarycentricCoordinates = includeBarycentricCoordinates
                ? GetBoundedBarycentricCoordinates(
                    point,
                    new Triplet<VectorXY>(
                        mainCenter,
                        indexTriplet.Left.GetHexCenter(grid.HexApothem, grid.HexRadius, grid.HexOrigin, grid.Layout),
                        indexTriplet.Right.GetHexCenter(grid.HexApothem, grid.HexRadius, grid.HexOrigin, grid.Layout)),
                    hasMain,
                    hasLeft,
                    hasRight)
                : default;

            candidate = new Candidate(
                boundedIndexTriplet,
                boundedBarycentricCoordinates,
                closestVertexSquaredDistance);
            return true;
        }

        private static bool TryCreateNearbyCandidate(
            HexGridDefinition grid,
            VectorXY point,
            VectorXYInt mainIndex,
            VectorXY[] normalizedHexVertexes,
            bool includeBarycentricCoordinates,
            out Candidate bestCandidate)
        {
            bestCandidate = default;
            bool hasBestCandidate = false;

            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    if (offsetX == 0 && offsetY == 0)
                        continue;

                    var candidateIndex = new VectorXYInt(mainIndex.X + offsetX, mainIndex.Y + offsetY);
                    bool candidateHasMain = ContainsHex(grid, candidateIndex);

                    if (!TryCreateCandidate(
                        grid,
                        point,
                        candidateIndex,
                        candidateHasMain,
                        normalizedHexVertexes,
                        includeBarycentricCoordinates,
                        out Candidate candidate))
                    {
                        continue;
                    }

                    if (!hasBestCandidate ||
                        candidate.ClosestVertexSquaredDistance < bestCandidate.ClosestVertexSquaredDistance)
                    {
                        bestCandidate = candidate;
                        hasBestCandidate = true;
                    }
                }
            }

            return hasBestCandidate;
        }

        private static void WriteCandidate(
            int flatIndex,
            Candidate candidate,
            Triplet<VectorXYInt>[] indexTriplets,
            Triplet<float>[] barycentricCoordinates,
            Triplet<byte>[] chromaticIndices,
            bool[] hasHex,
            Layout layout)
        {
            hasHex[flatIndex] = true;

            if (indexTriplets != null)
                indexTriplets[flatIndex] = candidate.IndexTriplet;

            if (chromaticIndices != null)
                chromaticIndices[flatIndex] = candidate.IndexTriplet.GetChromaticTriplet(layout);

            if (barycentricCoordinates != null)
                barycentricCoordinates[flatIndex] = candidate.BarycentricCoordinates;
        }

        private static Triplet<float> GetBoundedBarycentricCoordinates(
            VectorXY point,
            Triplet<VectorXY> centerTriplet,
            bool hasMain,
            bool hasLeft,
            bool hasRight)
        {
            Triplet<float> barycentric = point.BarycentricCoordinates(
                centerTriplet.Main,
                centerTriplet.Left,
                centerTriplet.Right);

            //if (hasMain && hasLeft && hasRight)
            //    return barycentric;

            float main = hasMain ? barycentric.Main : 0f;
            float left = hasLeft ? barycentric.Left : 0f;
            float right = hasRight ? barycentric.Right : 0f;
            float sum = main + left + right;

            //if (sum <= GeometryConstants.GeometryEpsilon)
            //    return GetSingleAvailableWeight(hasMain, hasLeft, hasRight);

            switch (hasMain, hasLeft, hasRight)
            {
                case (false, false, false):
                    return new Triplet<float>(1f, 0f, 0f);
                case (false, false, true):
                    return new Triplet<float>(0f, 0f, 1f);
                case (false, true, false):
                    return new Triplet<float>(0f, 1f, 0f);
                case (false, true, true):
                    (left, right) = point.BarycentricCoordinates(centerTriplet.Left, centerTriplet.Right);
                    sum = left + right;
                    return new Triplet<float>(0f, left / sum, right / sum);
                case (true, false, false):
                    return new Triplet<float>(1f, 0f, 0f);
                case (true, false, true):
                    (main, right) = point.BarycentricCoordinates(centerTriplet.Main, centerTriplet.Right);
                    sum = main + right;
                    return new Triplet<float>(main / sum, 0f, right / sum);
                case (true, true, false):
                    (main, left) = point.BarycentricCoordinates(centerTriplet.Main, centerTriplet.Left);
                    sum = main + left;
                    return new Triplet<float>(main / sum, left / sum, 0f);
                case (true, true, true):
                    (main, left, right) = point.BarycentricCoordinates(
                    centerTriplet.Main,
                    centerTriplet.Left,
                    centerTriplet.Right);
                    return new Triplet<float>(main / sum, left / sum, right / sum);
            }
        }

        private static Triplet<float> GetSingleAvailableWeight(
            bool hasMain,
            bool hasLeft,
            bool hasRight)
        {
            if (hasMain)
                return new Triplet<float>(1f, 0f, 0f);

            if (hasLeft)
                return new Triplet<float>(0f, 1f, 0f);

            if (hasRight)
                return new Triplet<float>(0f, 0f, 1f);

            return new Triplet<float>(0f, 0f, 0f);
        }

        private static void ThrowIfFillModeIsUnsupported(HexVertexTripletGridFillMode fillMode)
        {
            if (fillMode != HexVertexTripletGridFillMode.HitHexesOnly &&
                fillMode != HexVertexTripletGridFillMode.FillEmptyCells)
            {
                throw new ArgumentOutOfRangeException(nameof(fillMode), fillMode, "Unsupported vertex triplet grid fill mode.");
            }
        }

        private static VectorXYInt GetFallbackIndex(
            Triplet<VectorXYInt> indexTriplet,
            bool hasMain,
            bool hasLeft)
        {
            if (hasMain)
                return indexTriplet.Main;

            if (hasLeft)
                return indexTriplet.Left;

            return indexTriplet.Right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsHex(HexGridDefinition grid, VectorXYInt index)
        {
            return ContainsHex(grid, index.X, index.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsHex(HexGridDefinition grid, int x, int y)
        {
            return (uint)x < (uint)grid.HexResolution.X &&
                (uint)y < (uint)grid.HexResolution.Y;
        }

        private static int GetClosestVertexIndex(
            VectorXY point,
            float radius,
            VectorXY hexCenter,
            VectorXY[] normalizedHexVertexes,
            out float minSquaredDistance)
        {
            minSquaredDistance = float.MaxValue;
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

        private readonly struct Candidate
        {
            public Candidate(
                Triplet<VectorXYInt> indexTriplet,
                Triplet<float> barycentricCoordinates,
                float closestVertexSquaredDistance)
            {
                IndexTriplet = indexTriplet;
                BarycentricCoordinates = barycentricCoordinates;
                ClosestVertexSquaredDistance = closestVertexSquaredDistance;
            }

            public Triplet<VectorXYInt> IndexTriplet { get; }

            public Triplet<float> BarycentricCoordinates { get; }

            public float ClosestVertexSquaredDistance { get; }
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
