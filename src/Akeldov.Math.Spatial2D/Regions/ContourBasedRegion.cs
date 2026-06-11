using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Regions
{
    /// <summary>
    /// Represents a two-dimensional region bounded by one or more closed contours.
    /// </summary>
    public sealed class ContourBasedRegion : IContourBasedRegion
    {
        private readonly IContour[] _contours;
        private readonly IReadOnlyList<IContour> _readOnlyContours;
        private readonly FillRule _fillRule;

        /// <summary>
        /// Initializes a new contour-based region from the specified contours.
        /// </summary>
        /// <param name="contours">The contours that bound the region.</param>
        /// <param name="fillRule">The fill rule used to determine whether points belong to the region.</param>
        public ContourBasedRegion(IReadOnlyList<IContour> contours, FillRule fillRule = FillRule.EvenOdd)
        {
            if (contours == null)
                throw new ArgumentNullException(nameof(contours));

            if (contours.Count == 0)
                throw new ArgumentException("A region must contain at least one contour.", nameof(contours));

            if (fillRule != FillRule.EvenOdd)
                throw new ArgumentOutOfRangeException(nameof(fillRule), "Fill rule is not supported.");

            _contours = new IContour[contours.Count];

            for (int i = 0; i < contours.Count; i++)
            {
                _contours[i] = contours[i] ?? throw new ArgumentException("A region cannot contain null contours.", nameof(contours));
            }

            _readOnlyContours = Array.AsReadOnly(_contours);
            _fillRule = fillRule;
        }

        /// <summary>
        /// Gets the read-only structural view of the contours that define this region.
        /// </summary>
        public IReadOnlyList<IContour> Contours => _readOnlyContours;

        /// <inheritdoc/>
        public FillRule FillRule => _fillRule;

        /// <inheritdoc/>
        public bool Contains(PointXY point, float geometryEpsilon = 1E-06F)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            int enclosingContourCount = 0;

            for (int i = 0; i < _contours.Length; i++)
            {
                if (_contours[i].Encloses(point, geometryEpsilon))
                    enclosingContourCount++;
            }

            return enclosingContourCount % 2 == 1;
        }
    }
}
