using System;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>A Voronoi region clipped to the partition bounds.</summary>
    public sealed class VoronoiCell
    {
        private readonly PointXY _centroid;

        internal VoronoiCell(Site site, ContourBasedRegion? region, PointXY centroid)
        {
            Site = site.Position;
            Weight = site.Weight;
            Region = region;
            _centroid = centroid;
        }

        /// <summary>The position generating this cell, after any relaxation.</summary>
        public PointXY Site { get; }

        /// <summary>The multiplicative weight of this site. Unweighted points have unit weight.</summary>
        public float Weight { get; }

        /// <summary>The filled cell, including all components and holes, or <see langword="null"/>
        /// when the clipped cell has no representable area.</summary>
        public ContourBasedRegion? Region { get; }

        /// <summary>Returns the area centroid of the clipped cell.</summary>
        /// <returns>The center of mass of the cell assuming uniform density.</returns>
        /// <exception cref="InvalidOperationException">The cell has no area.</exception>
        public PointXY GetCentroid()
        {
            if (Region == null)
                throw new InvalidOperationException("A cell with no area has no centroid.");

            return _centroid;
        }
    }
}
