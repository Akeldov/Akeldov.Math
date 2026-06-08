using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class PolyhexBuilder
    {
        private readonly bool[] _cells;

        public PolyhexBuilder(int qSize, int rSize)
        {
            if (qSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(qSize));

            if (rSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(rSize));

            QRSResolution = new VectorQRSInt(qSize, rSize);
            _cells = new bool[checked(qSize * rSize)];
        }

        public PolyhexBuilder(Polyhex polyhex)
        {
            if (polyhex == null)
                throw new ArgumentNullException(nameof(polyhex));

            QRSResolution = polyhex.QRSResolution;
            _cells = new bool[checked(QRSResolution.Q * QRSResolution.R)];

            for (int q = 0; q < QRSResolution.Q; q++)
            {
                for (int r = 0; r < QRSResolution.R; r++)
                {
                    _cells[GetFlatIndex(q, r)] = polyhex[q, r];
                }
            }
        }

        public VectorQRSInt QRSResolution { get; }

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

        public Polyhex ToPolyhex()
        {
            return new Polyhex(QRSResolution.Q, QRSResolution.R, _cells);
        }

        private int GetFlatIndex(int q, int r) => q * QRSResolution.R + r;
    }
}
