using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Associates a polyhex mask with the physical radius and apothem of its cells.
    /// </summary>
    public class PolyhexGeometry : IPolyhexGeometry
    {
        private readonly Polyhex _polyhex;
        private readonly float _hexApothem;
        private readonly float _hexRadius;

        /// <summary>
        /// Associates an existing polyhex with a physical hex radius.
        /// </summary>
        /// <param name="polyhex">The polyhex whose mask and QRS resolution are retained.</param>
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
        /// Creates polyhex geometry from a Boolean mask and physical hex radius.
        /// </summary>
        /// <param name="boolMask">A rectangular Q/R mask in which <see langword="true"/> cells belong to the polyhex.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        public PolyhexGeometry(bool[,] boolMask, float radius)
            : this(new Polyhex(boolMask), radius)
        {
        }

        /// <summary>
        /// Creates polyhex geometry from an integer mask and physical hex radius.
        /// </summary>
        /// <param name="intMask">A rectangular Q/R mask in which nonzero cells belong to the polyhex.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        public PolyhexGeometry(int[,] intMask, float radius)
            : this(new Polyhex(intMask), radius)
        {
        }

        /// <summary>
        /// Creates empty polyhex geometry with the specified QRS extents and physical hex radius.
        /// </summary>
        /// <param name="qrsResolution">The QRS extents of a completely filled polyhex.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        public PolyhexGeometry(VectorQRSInt qrsResolution, float radius)
            : this(new Polyhex(qrsResolution), radius)
        {
        }

        /// <summary>
        /// Gets the QRS extents of the polyhex mask.
        /// </summary>
        public VectorQRSInt QRSResolution => _polyhex.QRSResolution;

        /// <summary>
        /// Gets the number of present hex cells.
        /// </summary>
        public int HexCount => _polyhex.HexCount;

        /// <summary>
        /// Gets whether the specified QRS cell belongs to the polyhex.
        /// </summary>
        /// <param name="index">The integer QRS index to test.</param>
        public bool this[VectorQRSInt index]
        {
            get => _polyhex[index];
        }

        /// <summary>
        /// Gets whether the cell at the specified Q/R mask coordinates belongs to the polyhex.
        /// </summary>
        /// <param name="QIndex">The zero-based Q coordinate.</param>
        /// <param name="RIndex">The zero-based R coordinate.</param>
        public bool this[int QIndex, int RIndex]
        {
            get => _polyhex[QIndex, RIndex];
        }

        /// <summary>
        /// Gets the distance from a hex center to an edge.
        /// </summary>
        public float HexApothem => _hexApothem;

        /// <summary>
        /// Gets the distance from a hex center to a vertex.
        /// </summary>
        public float HexRadius => _hexRadius;

        /// <summary>
        /// Creates a polyhex that includes this shape and every adjacent hex.
        /// </summary>
        public Polyhex GetExtended()
        {
            return _polyhex.GetExtended();
        }

        /// <summary>
        /// Creates a polyhex containing the outermost present cells of this shape.
        /// </summary>
        public Polyhex GetContour()
        {
            return _polyhex.GetContour();
        }

        /// <summary>
        /// Creates a rectangular Q/R mask of the polyhex.
        /// </summary>
        /// <returns>A new two-dimensional array owned by the caller.</returns>
        public bool[,] ToBoolArray()
        {
            return _polyhex.ToBoolArray();
        }
    }
}
