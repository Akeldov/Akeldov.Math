using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Chromatization
{
    /// <summary>
    /// Initializes a new instance of the ChromaticIndexMap type.
    /// </summary>
    public sealed class ChromaticIndexMap : IHexMap<byte>
    {
        private readonly byte[] _values;

        /// <summary>
        /// Initializes a new instance of the ChromaticIndexMap type.
        /// </summary>
        /// <param name="width">The width value.</param>
        /// <param name="height">The height value.</param>
        /// <param name="layout">The layout value.</param>
        public ChromaticIndexMap(int width, int height, Layout layout)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            var count = checked(width * height);

            Width = width;
            Height = height;
            Layout = layout;
            _values = new byte[count];

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

        /// <summary>
        /// Gets the Width value.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the Height value.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the map resolution in hexes.
        /// </summary>
        public VectorXYInt Resolution => new VectorXYInt(Width, Height);

        /// <summary>
        /// Gets the Layout value.
        /// </summary>
        public Layout Layout { get; }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public byte this[VectorXYInt index]
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
        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
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
                    _values[rowStart + x] = (byte)PositiveModulo(x - qOffset - y, 3);
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

                    _values[rowStart + x] = (byte)PositiveModulo(y - rOffset - x, 3);
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
