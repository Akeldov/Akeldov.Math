using Akeldov.Math.Hexes.Vectors.QRS;
using System;
using System.Text;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a Polyhex instance.
    /// </summary>
    public class Polyhex : IPolyhex, IEquatable<Polyhex>
    {
        private readonly bool[] _cells;
        private readonly int _hash;

        /// <summary>
        /// Initializes a new instance of the Polyhex type.
        /// </summary>
        /// <param name="intMask">The IntMask value.</param>
        public Polyhex(int[,] intMask)
            : this((intMask ?? throw new ArgumentNullException(nameof(intMask))).ToBoolMask())
        {
        }

        /// <summary>
        /// Initializes a new instance of the Polyhex type.
        /// </summary>
        /// <param name="boolMask">The BoolMask value.</param>
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
        /// Initializes a new instance of the Polyhex type.
        /// </summary>
        /// <param name="qrsResolution">The qrsResolution value.</param>
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
        /// Gets the QRSResolution value.
        /// </summary>
        public VectorQRSInt QRSResolution { get; }

        /// <summary>
        /// Gets the HexCount value.
        /// </summary>
        public int HexCount { get; }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public bool this[VectorQRSInt index]
        {
            get => this[index.Q, index.R];
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="QIndex">The QIndex value.</param>
        /// <param name="RIndex">The RIndex value.</param>
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
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        public Polyhex GetExtended()
        {
            return new Polyhex(ToBoolArray().GetExtended());
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        public Polyhex GetContour()
        {
            return new Polyhex(ToBoolArray().GetContour());
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
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
        /// Performs the Equals operation.
        /// </summary>
        /// <param name="other">The other value.</param>
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
        /// Performs the implicit operator Polyhex operation.
        /// </summary>
        /// <param name="boolMask">The BoolMask value.</param>
        public static implicit operator Polyhex(bool[,] boolMask) => new Polyhex(boolMask);

        /// <summary>
        /// Applies the operator == operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static bool operator ==(Polyhex? left, Polyhex? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Applies the operator != operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static bool operator !=(Polyhex? left, Polyhex? right) => !(left == right);

        private int GetFlatIndex(int q, int r) => q * QRSResolution.R + r;
    }
}
