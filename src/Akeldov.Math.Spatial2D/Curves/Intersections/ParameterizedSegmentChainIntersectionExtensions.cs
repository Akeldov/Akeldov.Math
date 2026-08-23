using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact line-intersection calculations for <see cref="ParameterizedSegmentChain"/>.
    /// </summary>
    public static class ParameterizedSegmentChainIntersectionExtensions
    {
        /// <summary>
        /// Returns the distinct isolated point intersections between a segment chain and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment chain.</param>
        /// <param name="line">The line to intersect with the source chain.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the canonical direction of the line. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegmentChain source, Line line)
        {
            return SegmentCollectionLineIntersections.GetPointIntersections(source.Segments, line);
        }
    }
}
