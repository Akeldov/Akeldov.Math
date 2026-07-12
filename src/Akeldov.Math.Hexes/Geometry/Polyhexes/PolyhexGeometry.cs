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
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        public PolyhexGeometry(Polyhex polyhex, float radius)
        {
            _polyhex = polyhex ?? throw new ArgumentNullException(nameof(polyhex));

            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Hex radius must be finite and positive.");

            _hexRadius = radius;
            _hexApothem = radius.ConvertHexRadiusToApothem();
        }

        /// <summary>
        /// Initializes a new instance of the PolyhexGeometry type.
        /// </summary>
        /// <param name="boolMask">The BoolMask value.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        public PolyhexGeometry(bool[,] boolMask, float radius)
            : this(new Polyhex(boolMask), radius)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PolyhexGeometry type.
        /// </summary>
        /// <param name="intMask">The IntMask value.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        public PolyhexGeometry(int[,] intMask, float radius)
            : this(new Polyhex(intMask), radius)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PolyhexGeometry type.
        /// </summary>
        /// <param name="qrsResolution">The qrsResolution value.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        public PolyhexGeometry(VectorQRSInt qrsResolution, float radius)
            : this(new Polyhex(qrsResolution), radius)
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
