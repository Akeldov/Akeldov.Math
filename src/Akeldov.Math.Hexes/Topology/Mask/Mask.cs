using Akeldov.Math.Hexes.Vectors.QRS;
using System;
using System.Text;

namespace Akeldov.Math.Hexes.Topology
{
    public class Mask : IEquatable<Mask>
    {
        private readonly bool[] _cells;
        private readonly int _hash;

        public Mask(int[,] intMask) : this(intMask.ToBoolMask())
        {
        }

        public Mask(bool[,] boolMask)
        {
            if (boolMask == null)
                throw new ArgumentNullException(nameof(boolMask));

            QSize = boolMask.GetLength(0);
            RSize = boolMask.GetLength(1);
            _cells = new bool[checked(QSize * RSize)];

            var hash = new HashCode();
            hash.Add(QSize);
            hash.Add(RSize);

            var positiveSize = 0;
            for (int q = 0; q < QSize; q++)
            {
                for (int r = 0; r < RSize; r++)
                {
                    bool value = boolMask[q, r];
                    _cells[GetFlatIndex(q, r)] = value;
                    hash.Add(value);

                    if (value)
                        positiveSize++;
                }
            }

            PositiveSize = positiveSize;
            _hash = hash.ToHashCode();
        }

        internal Mask(int qSize, int rSize, bool[] cells)
        {
            if (qSize < 0)
                throw new ArgumentOutOfRangeException(nameof(qSize));

            if (rSize < 0)
                throw new ArgumentOutOfRangeException(nameof(rSize));

            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            int length = checked(qSize * rSize);
            if (cells.Length != length)
                throw new ArgumentException("Cell count must match mask dimensions.", nameof(cells));

            QSize = qSize;
            RSize = rSize;
            _cells = new bool[length];

            var hash = new HashCode();
            hash.Add(QSize);
            hash.Add(RSize);

            var positiveSize = 0;
            for (int i = 0; i < cells.Length; i++)
            {
                bool value = cells[i];
                _cells[i] = value;
                hash.Add(value);

                if (value)
                    positiveSize++;
            }

            PositiveSize = positiveSize;
            _hash = hash.ToHashCode();
        }

        public int QSize { get; }

        public int RSize { get; }

        public int PositiveSize { get; }

        public bool this[VectorQRSInt index]
        {
            get => this[index.Q, index.R];
        }

        public bool this[int QIndex, int RIndex]
        {
            get => _cells[GetFlatIndex(QIndex, RIndex)];
        }

        public Mask GetExtended()
        {
            return new Mask(ToBoolArray().GetExtended());
        }

        public Mask GetContour()
        {
            return new Mask(ToBoolArray().GetContour());
        }

        public bool[,] ToBoolArray()
        {
            var result = new bool[QSize, RSize];

            for (int q = 0; q < QSize; q++)
            {
                for (int r = 0; r < RSize; r++)
                {
                    result[q, r] = this[q, r];
                }
            }

            return result;
        }

        public override int GetHashCode() => _hash;

        public override bool Equals(object obj) => obj is Mask other && Equals(other);

        public bool Equals(Mask other)
        {
            if (other is null)
                return false;

            if (QSize != other.QSize || RSize != other.RSize)
                return false;

            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != other._cells[i])
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int q = 0; q < QSize; q++)
            {
                for (int r = 0; r < RSize; r++)
                {
                    sb.Append(this[q, r] ? 1 : 0);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static implicit operator Mask(bool[,] boolMask) => new Mask(boolMask);

        public static bool operator ==(Mask left, Mask right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Mask left, Mask right) => !(left == right);

        private int GetFlatIndex(int q, int r) => q * RSize + r;
    }
}
