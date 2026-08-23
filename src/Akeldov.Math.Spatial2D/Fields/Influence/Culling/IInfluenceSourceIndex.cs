using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Owns an immutable snapshot of influence sources and selects the sources relevant to a sampled point.
    /// </summary>
    /// <remarks>
    /// Implementations must own and retain a structurally immutable source snapshot, copying any
    /// caller-owned mutable input used to create it. Selected sources come from that snapshot. If no
    /// source can be selected by the primary indexing strategy, the index is responsible for applying
    /// an explicit fallback, such as selecting the nearest source.
    /// </remarks>
    /// <typeparam name="TInfluenceSource">The influence source type.</typeparam>
    public interface IInfluenceSourceIndex<TInfluenceSource>
        where TInfluenceSource : IInfluenceSource
    {
        /// <summary>
        /// Gets the immutable source snapshot owned by this index.
        /// </summary>
        IReadOnlyList<TInfluenceSource> Sources { get; }

        /// <summary>
        /// Returns the influence sources relevant to the specified point.
        /// </summary>
        /// <param name="point">The point being sampled.</param>
        /// <returns>
        /// A new mutable list owned by the caller. The list must contain at least one source, and
        /// every returned source must come from <see cref="Sources"/>.
        /// </returns>
        List<TInfluenceSource> SelectSources(PointXY point);
    }
}
