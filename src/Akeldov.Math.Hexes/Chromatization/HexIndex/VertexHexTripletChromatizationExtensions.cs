using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Chromatization
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class VertexHexTripletExtensions
    {
        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="vertexHexIndexTriplet">The vertexHexIndexTriplet value.</param>
        /// <param name="layout">The layout value.</param>
        public static Triplet<byte> GetChromaticTriplet(this Triplet<VectorXYInt> vertexHexIndexTriplet, Layout layout)
        {
            var hexBlendIndex = vertexHexIndexTriplet.Main.GetChromaticClass(layout);
            var hexLeftBlendIndex = vertexHexIndexTriplet.Left.GetChromaticClass(layout);
            var hexRighBlendtIndex = vertexHexIndexTriplet.Right.GetChromaticClass(layout);
            var chromaticTriplet = new Triplet<byte>((byte)hexBlendIndex, (byte)hexLeftBlendIndex, (byte)hexRighBlendtIndex);
            return chromaticTriplet;
        }
    }
}
