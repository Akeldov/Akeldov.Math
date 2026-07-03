using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Initializes a new instance of the PolyhexBuilder type.
    /// </summary>
    public sealed class PolyhexBuilder
    {
        private readonly bool[] _cells;

        /// <summary>
        /// Initializes a new instance of the PolyhexBuilder type.
        /// </summary>
        /// <param name="qSize">The qSize value.</param>
        /// <param name="rSize">The rSize value.</param>
        public PolyhexBuilder(int qSize, int rSize)
        {
            if (qSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(qSize));

            if (rSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(rSize));

            QRSResolution = new VectorQRSInt(qSize, rSize);
            _cells = new bool[checked(qSize * rSize)];
        }

        /// <summary>
        /// Initializes a new instance of the PolyhexBuilder type.
        /// </summary>
        /// <param name="polyhex">The polyhex value.</param>
        public PolyhexBuilder(Polyhex? polyhex)
        {
            if (polyhex is null)
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

        /// <summary>
        /// Gets the QRSResolution value.
        /// </summary>
        public VectorQRSInt QRSResolution { get; }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public bool this[VectorQRSInt index]
        {
            get => this[index.Q, index.R];
            set => this[index.Q, index.R] = value;
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="qIndex">The qIndex value.</param>
        /// <param name="rIndex">The rIndex value.</param>
        public bool this[int qIndex, int rIndex]
        {
            get
            {
                if ((uint)qIndex >= (uint)QRSResolution.Q ||
                    (uint)rIndex >= (uint)QRSResolution.R)
                    throw new IndexOutOfRangeException($"Polyhex builder index out of bounds: ({qIndex}, {rIndex})");

                return _cells[GetFlatIndex(qIndex, rIndex)];
            }
            set
            {
                if ((uint)qIndex >= (uint)QRSResolution.Q ||
                    (uint)rIndex >= (uint)QRSResolution.R)
                    throw new IndexOutOfRangeException($"Polyhex builder index out of bounds: ({qIndex}, {rIndex})");

                _cells[GetFlatIndex(qIndex, rIndex)] = value;
            }
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        public Polyhex ToPolyhex()
        {
            return new Polyhex(QRSResolution.Q, QRSResolution.R, _cells);
        }

        private int GetFlatIndex(int q, int r) => q * QRSResolution.R + r;
    }
}
