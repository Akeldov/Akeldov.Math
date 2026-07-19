using Akeldov.Math.Hexes.Vectors.QRS;
using System;
using System.Text;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents an immutable set of hex cells in a rectangular Q/R mask.
    /// </summary>
    public class Polyhex : IPolyhex, IEquatable<Polyhex>
    {
        private readonly bool[] _cells;
        private readonly int _hash;

        /// <summary>
        /// Creates a polyhex from a rectangular integer mask.
        /// </summary>
        /// <param name="intMask">A Q/R mask in which nonzero cells belong to the polyhex. The contents are copied.</param>
        public Polyhex(int[,] intMask)
            : this((intMask ?? throw new ArgumentNullException(nameof(intMask))).ToBoolMask())
        {
        }

        /// <summary>
        /// Creates a polyhex from a rectangular Boolean mask.
        /// </summary>
        /// <param name="boolMask">A Q/R mask in which <see langword="true"/> cells belong to the polyhex. The contents are copied.</param>
        public Polyhex(bool[,] boolMask)
        {
            if (boolMask == null)
                throw new ArgumentNullException(nameof(boolMask));

            int qSize = boolMask.GetLength(0);
            int rSize = boolMask.GetLength(1);
            if (qSize <= 0 || rSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(boolMask), boolMask, "Polyhex dimensions must be greater than zero.");

            QRSResolution = new VectorQRSInt(qSize, rSize);
            _cells = new bool[checked(qSize * rSize)];

            var hash = new HashCode();
            hash.Add(QRSResolution);

            var hexCount = 0;
            for (int q = 0; q < qSize; q++)
            {
                for (int r = 0; r < rSize; r++)
                {
                    bool value = boolMask[q, r];
                    _cells[GetFlatIndex(q, r)] = value;
                    hash.Add(value);

                    if (value)
                        hexCount++;
                }
            }

            HexCount = hexCount;
            _hash = hash.ToHashCode();
        }

        /// <summary>
        /// Creates an empty polyhex mask with the specified QRS extents.
        /// </summary>
        /// <param name="qrsResolution">The QRS extents of the mask.</param>
        public Polyhex(VectorQRSInt qrsResolution)
        {
            if (qrsResolution.Q <= 0 || qrsResolution.R <= 0)
                throw new ArgumentOutOfRangeException(nameof(qrsResolution), qrsResolution, "Polyhex resolution components must be greater than zero.");

            QRSResolution = qrsResolution;
            _cells = new bool[checked(qrsResolution.Q * qrsResolution.R)];

            var hash = new HashCode();
            hash.Add(QRSResolution);
            for (int i = 0; i < _cells.Length; i++)
                hash.Add(false);

            HexCount = 0;
            _hash = hash.ToHashCode();
        }

        internal Polyhex(int qSize, int rSize, bool[] cells)
        {
            if (qSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(qSize));

            if (rSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(rSize));

            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            int length = checked(qSize * rSize);
            if (cells.Length != length)
                throw new ArgumentException("Cell count must match polyhex dimensions.", nameof(cells));

            QRSResolution = new VectorQRSInt(qSize, rSize);
            _cells = new bool[length];

            var hash = new HashCode();
            hash.Add(QRSResolution);

            var hexCount = 0;
            for (int i = 0; i < cells.Length; i++)
            {
                bool value = cells[i];
                _cells[i] = value;
                hash.Add(value);

                if (value)
                    hexCount++;
            }

            HexCount = hexCount;
            _hash = hash.ToHashCode();
        }

        /// <summary>
        /// Gets the QRS extents of the mask.
        /// </summary>
        public VectorQRSInt QRSResolution { get; }

        /// <summary>
        /// Gets the number of present hex cells.
        /// </summary>
        public int HexCount { get; }

        /// <summary>
        /// Gets whether the specified QRS cell belongs to the polyhex.
        /// </summary>
        /// <param name="index">The integer QRS index to test.</param>
        public bool this[VectorQRSInt index]
        {
            get => this[index.Q, index.R];
        }

        /// <summary>
        /// Gets whether the cell at the specified Q/R mask coordinates belongs to the polyhex.
        /// </summary>
        /// <param name="QIndex">The zero-based Q coordinate.</param>
        /// <param name="RIndex">The zero-based R coordinate.</param>
        public bool this[int QIndex, int RIndex]
        {
            get
            {
                if ((uint)QIndex >= (uint)QRSResolution.Q ||
                    (uint)RIndex >= (uint)QRSResolution.R)
                    throw new IndexOutOfRangeException($"Polyhex index out of bounds: ({QIndex}, {RIndex})");

                return _cells[GetFlatIndex(QIndex, RIndex)];
            }
        }

        /// <summary>
        /// Creates a polyhex that includes this shape and every adjacent hex.
        /// </summary>
        public Polyhex GetExtended()
        {
            return new Polyhex(ToBoolArray().GetExtended());
        }

        /// <summary>
        /// Creates a polyhex containing the outermost present cells of this shape.
        /// </summary>
        public Polyhex GetContour()
        {
            return new Polyhex(ToBoolArray().GetContour());
        }

        /// <summary>
        /// Creates a rectangular Q/R mask of the polyhex.
        /// </summary>
        /// <returns>A new two-dimensional array owned by the caller.</returns>
        public bool[,] ToBoolArray()
        {
            var result = new bool[QRSResolution.Q, QRSResolution.R];

            for (int q = 0; q < QRSResolution.Q; q++)
            {
                for (int r = 0; r < QRSResolution.R; r++)
                {
                    result[q, r] = this[q, r];
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => _hash;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Polyhex other && Equals(other);

        /// <summary>
        /// Determines whether another polyhex has the same resolution and cell mask.
        /// </summary>
        /// <param name="other">The polyhex to compare with this instance.</param>
        public bool Equals(Polyhex? other)
        {
            if (other is null)
                return false;

            if (QRSResolution != other.QRSResolution)
                return false;

            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != other._cells[i])
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int q = 0; q < QRSResolution.Q; q++)
            {
                for (int r = 0; r < QRSResolution.R; r++)
                {
                    sb.Append(this[q, r] ? 1 : 0);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a polyhex by copying a Boolean Q/R mask.
        /// </summary>
        /// <param name="boolMask">A mask in which <see langword="true"/> cells belong to the polyhex.</param>
        public static implicit operator Polyhex(bool[,] boolMask) => new Polyhex(boolMask);

        /// <summary>
        /// Determines whether two polyhexes have the same resolution and cell mask.
        /// </summary>
        /// <param name="left">The first polyhex to compare.</param>
        /// <param name="right">The second polyhex to compare.</param>
        public static bool operator ==(Polyhex? left, Polyhex? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two polyhexes differ in resolution or cell mask.
        /// </summary>
        /// <param name="left">The first polyhex to compare.</param>
        /// <param name="right">The second polyhex to compare.</param>
        public static bool operator !=(Polyhex? left, Polyhex? right) => !(left == right);

        private int GetFlatIndex(int q, int r) => q * QRSResolution.R + r;
    }
}
