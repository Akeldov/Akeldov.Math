using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents a closed two-dimensional contour made from finite paths.
    /// </summary>
    public interface ICompositeContour : IContour
    {
        /// <summary>
        /// Gets the read-only structural view of the finite paths that form this contour.
        /// </summary>
        IReadOnlyList<IFinitePath> Curves { get; }
    }
}
