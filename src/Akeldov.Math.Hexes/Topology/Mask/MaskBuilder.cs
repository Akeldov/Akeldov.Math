using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class MaskBuilder
    {
        private readonly bool[] _cells;

        public MaskBuilder(int qSize, int rSize)
        {
            if (qSize < 0)
                throw new ArgumentOutOfRangeException(nameof(qSize));

            if (rSize < 0)
                throw new ArgumentOutOfRangeException(nameof(rSize));

            QSize = qSize;
            RSize = rSize;
            _cells = new bool[checked(qSize * rSize)];
        }

        public MaskBuilder(Mask mask)
        {
            if (mask == null)
                throw new ArgumentNullException(nameof(mask));

            QSize = mask.QSize;
            RSize = mask.RSize;
            _cells = new bool[checked(QSize * RSize)];

            for (int q = 0; q < QSize; q++)
            {
                for (int r = 0; r < RSize; r++)
                {
                    _cells[GetFlatIndex(q, r)] = mask[q, r];
                }
            }
        }

        public int QSize { get; }

        public int RSize { get; }

        public bool this[VectorQRSInt index]
        {
            get => this[index.Q, index.R];
            set => this[index.Q, index.R] = value;
        }

        public bool this[int qIndex, int rIndex]
        {
            get => _cells[GetFlatIndex(qIndex, rIndex)];
            set => _cells[GetFlatIndex(qIndex, rIndex)] = value;
        }

        public Mask ToMask()
        {
            return new Mask(QSize, RSize, _cells);
        }

        private int GetFlatIndex(int q, int r) => q * RSize + r;
    }
}
