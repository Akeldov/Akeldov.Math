using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Culls influence sources to the subset that should be considered when sampling a point.
    /// </summary>
    /// <remarks>
    /// Culling excludes sources from the current sampling operation only; it must not remove
    /// sources from the original collection. Implementations must return a non-null mutable list,
    /// owned by the caller and containing at least one source. If no source can be selected by the
    /// primary culling strategy, the culler is responsible for applying an explicit fallback, such
    /// as returning the nearest source.
    /// </remarks>
    /// <typeparam name="TInfluenceSource">The influence source type.</typeparam>
    public interface IInfluenceSourceCuller<TInfluenceSource>
        where TInfluenceSource : IInfluenceSource
    {
        /// <summary>
        /// Returns the influence sources that remain relevant after culling for the specified point.
        /// </summary>
        /// <param name="point">The point being sampled.</param>
        /// <returns>
        /// The selected influence sources. The returned list must be non-null, mutable, owned by
        /// the caller, and contain at least one source.
        /// </returns>
        List<TInfluenceSource> Cull(PointXY point);
    }
}
