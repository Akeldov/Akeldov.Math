using Akeldov.Math.Hexes.Vectors.QRS;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides read-only access to a finite set of hex cells stored in a rectangular Q/R mask.
    /// </summary>
    public interface IPolyhex
    {
        /// <summary>
        /// Gets the QRS extents of the mask.
        /// </summary>
        VectorQRSInt QRSResolution { get; }

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
            get;
        }

        /// <summary>
        /// Gets whether the cell at the specified Q/R mask coordinates belongs to the polyhex.
        /// </summary>
        /// <param name="QIndex">The zero-based Q coordinate.</param>
        /// <param name="RIndex">The zero-based R coordinate.</param>
        public bool this[int QIndex, int RIndex]
        {
            get;
        }

        /// <summary>
        /// Creates a polyhex that includes this shape and every adjacent hex.
        /// </summary>
        public Polyhex GetExtended();

        /// <summary>
        /// Creates a polyhex containing the outermost present cells of this shape.
        /// </summary>
        public Polyhex GetContour();
    }
}
