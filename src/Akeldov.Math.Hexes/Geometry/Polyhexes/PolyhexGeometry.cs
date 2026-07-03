using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Represents a PolyhexGeometry instance.
    /// </summary>
    public class PolyhexGeometry : IPolyhexGeometry
    {
        private readonly Polyhex _polyhex;
        private readonly float _hexApothem;
        private readonly float _hexRadius;

        /// <summary>
        /// Initializes a new instance of the PolyhexGeometry type.
        /// </summary>
        /// <param name="polyhex">The polyhex value.</param>
        /// <param name="apothem">The apothem value.</param>
        public PolyhexGeometry(Polyhex polyhex, float apothem)
        {
            _polyhex = polyhex ?? throw new ArgumentNullException(nameof(polyhex));

            if (float.IsNaN(apothem) || float.IsInfinity(apothem) || apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(apothem), apothem, "Hex apothem must be finite and positive.");

            _hexApothem = apothem;
            _hexRadius = _hexApothem.ConvertHexApothemToRadius();
        }

        /// <summary>
        /// Initializes a new instance of the PolyhexGeometry type.
        /// </summary>
        /// <param name="boolMask">The BoolMask value.</param>
        /// <param name="apothem">The Apothem value.</param>
        public PolyhexGeometry(bool[,] boolMask, float apothem)
            : this(new Polyhex(boolMask), apothem)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PolyhexGeometry type.
        /// </summary>
        /// <param name="intMask">The IntMask value.</param>
        /// <param name="apothem">The Apothem value.</param>
        public PolyhexGeometry(int[,] intMask, float apothem)
            : this(new Polyhex(intMask), apothem)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PolyhexGeometry type.
        /// </summary>
        /// <param name="qrsResolution">The qrsResolution value.</param>
        /// <param name="apothem">The Apothem value.</param>
        public PolyhexGeometry(VectorQRSInt qrsResolution, float apothem)
            : this(new Polyhex(qrsResolution), apothem)
        {
        }

        /// <summary>
        /// Gets the QRSResolution value.
        /// </summary>
        public VectorQRSInt QRSResolution => _polyhex.QRSResolution;

        /// <summary>
        /// Gets the HexCount value.
        /// </summary>
        public int HexCount => _polyhex.HexCount;

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public bool this[VectorQRSInt index]
        {
            get => _polyhex[index];
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="QIndex">The QIndex value.</param>
        /// <param name="RIndex">The RIndex value.</param>
        public bool this[int QIndex, int RIndex]
        {
            get => _polyhex[QIndex, RIndex];
        }

        /// <summary>
        /// Gets the HexApothem value.
        /// </summary>
        public float HexApothem => _hexApothem;

        /// <summary>
        /// Gets the HexRadius value.
        /// </summary>
        public float HexRadius => _hexRadius;

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        public Polyhex GetExtended()
        {
            return _polyhex.GetExtended();
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        public Polyhex GetContour()
        {
            return _polyhex.GetContour();
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        public bool[,] ToBoolArray()
        {
            return _polyhex.ToBoolArray();
        }
    }
}
