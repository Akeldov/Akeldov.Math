using Akeldov.Math.Hexes.Vectors.QRS;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Defines a contract for IPolyhex implementations.
    /// </summary>
    public interface IPolyhex
    {
        /// <summary>
        /// Represents the <c>QRSResolution</c> value.
        /// </summary>
        VectorQRSInt QRSResolution { get; }

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
            get;
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="QIndex">The QIndex value.</param>
        /// <param name="RIndex">The RIndex value.</param>
        public bool this[int QIndex, int RIndex]
        {
            get;
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        public Polyhex GetExtended();

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        public Polyhex GetContour();
    }
}
