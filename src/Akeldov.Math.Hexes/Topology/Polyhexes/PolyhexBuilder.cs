using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Builds a mutable Q/R cell mask that can be copied into an immutable <see cref="Polyhex"/>.
    /// </summary>
    public sealed class PolyhexBuilder
    {
        private readonly bool[] _cells;

        /// <summary>
        /// Initializes an empty mask with the specified Q/R dimensions.
        /// </summary>
        /// <param name="qSize">The number of cells along the Q axis.</param>
        /// <param name="rSize">The number of cells along the R axis.</param>
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
        /// Initializes a mutable builder by copying an existing polyhex.
        /// </summary>
        /// <param name="polyhex">The polyhex whose dimensions and cell mask are copied.</param>
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
        /// Gets the QRS extents of the mutable mask.
        /// </summary>
        public VectorQRSInt QRSResolution { get; }

        /// <summary>
        /// Gets or sets whether the specified QRS cell belongs to the mask.
        /// </summary>
        /// <param name="index">The integer QRS index to access.</param>
        public bool this[VectorQRSInt index]
        {
            get => this[index.Q, index.R];
            set => this[index.Q, index.R] = value;
        }

        /// <summary>
        /// Gets or sets whether the cell at the specified Q/R coordinates belongs to the mask.
        /// </summary>
        /// <param name="qIndex">The zero-based Q coordinate.</param>
        /// <param name="rIndex">The zero-based R coordinate.</param>
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
        /// Creates an immutable polyhex by copying the current mask.
        /// </summary>
        public Polyhex ToPolyhex()
        {
            return new Polyhex(QRSResolution.Q, QRSResolution.R, _cells);
        }

        private int GetFlatIndex(int q, int r) => q * QRSResolution.R + r;
    }
}
