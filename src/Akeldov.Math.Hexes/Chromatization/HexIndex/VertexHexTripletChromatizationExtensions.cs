using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Chromatization
{
    /// <summary>
    /// Converts vertex-adjacent hex triplets to three-color classes.
    /// </summary>
    public static partial class VertexHexTripletExtensions
    {
        /// <summary>
        /// Converts the three hex indices meeting at a vertex to their chromatic classes.
        /// </summary>
        /// <param name="vertexHexIndexTriplet">The center, left, and right hex indices that meet at the vertex.</param>
        /// <param name="layout">The offset-coordinate layout.</param>
        /// <returns>The corresponding chromatic classes, each from 0 through 2.</returns>
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
