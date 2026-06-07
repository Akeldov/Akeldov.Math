using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Chromatization
{
    public sealed class HexFieldChromatization : IHexMap<byte>
    {
        public HexFieldChromatization(int width, int height, Layout layout)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            var count = checked(width * height);

            Width = width;
            Height = height;
            Layout = layout;
            ChromaticIndices = new byte[count];

            switch (layout)
            {
                case Layout.OddR:
                    FillRowLayoutChromaticIndices(false);
                    break;
                case Layout.EvenR:
                    FillRowLayoutChromaticIndices(true);
                    break;
                case Layout.OddQ:
                    FillColumnLayoutChromaticIndices(false);
                    break;
                case Layout.EvenQ:
                    FillColumnLayoutChromaticIndices(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        public int Width { get; }

        public int Height { get; }

        public Layout Layout { get; }

        public byte[] ChromaticIndices { get; }

        public byte this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Width ||
                    index.Y < 0 || index.Y >= Height)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return ChromaticIndices[GetFlatIndex(index)];
            }
        }

        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ChromaticIndices[index];
        }

        private void FillRowLayoutChromaticIndices(bool shiftedRowsUseUpperOffset)
        {
            for (int y = 0; y < Height; y++)
            {
                int rowStart = y * Width;
                int qOffset = shiftedRowsUseUpperOffset
                    ? (y + (y & 1)) / 2
                    : (y - (y & 1)) / 2;

                for (int x = 0; x < Width; x++)
                {
                    ChromaticIndices[rowStart + x] = (byte)PositiveModulo(x - qOffset - y, 3);
                }
            }
        }

        private void FillColumnLayoutChromaticIndices(bool shiftedColumnsUseUpperOffset)
        {
            for (int y = 0; y < Height; y++)
            {
                int rowStart = y * Width;

                for (int x = 0; x < Width; x++)
                {
                    int rOffset = shiftedColumnsUseUpperOffset
                        ? (x + (x & 1)) / 2
                        : (x - (x & 1)) / 2;

                    ChromaticIndices[rowStart + x] = (byte)PositiveModulo(y - rOffset - x, 3);
                }
            }
        }

        private static int PositiveModulo(int value, int divisor)
        {
            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Width + index.X;
    }
}
