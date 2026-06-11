using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Regions
{
    /// <summary>
    /// Represents a filled two-dimensional region defined by one or more contours.
    /// </summary>
    public interface IContourBasedRegion : IRegion
    {
        /// <summary>
        /// Gets the read-only structural view of the contours that define this region.
        /// </summary>
        IReadOnlyList<IContour> Contours { get; }

        /// <summary>
        /// Gets the fill rule used to interpret the contours.
        /// </summary>
        FillRule FillRule { get; }
    }
}
