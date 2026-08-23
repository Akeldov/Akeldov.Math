using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes curves into an 8-bit grayscale raster using nearest distance, stroke width, and fade distance.
        /// </summary>
        /// <typeparam name="T">The curve type.</typeparam>
        /// <param name="curves">The curves to rasterize.</param>
        /// <param name="curveWidth">The full curve stroke width in world coordinate units.</param>
        /// <param name="fadeDistance">The non-negative fade distance outside the stroke edge, in world coordinate units.</param>
        /// <param name="curveColor">The grayscale value used inside the stroke.</param>
        /// <param name="backgroundColor">The grayscale value used outside the stroke and fade band.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the nearest curve distance at each cell center.</returns>
        public static SpatialRaster<Gray8BitColor> Rasterize<T>(
            this IReadOnlyList<T> curves,
            float curveWidth,
            float fadeDistance,
            Gray8BitColor curveColor,
            Gray8BitColor backgroundColor,
            RasterGeometry rasterGeometry)
            where T : ICurve
        {
            if (curves == null)
                throw new ArgumentNullException(nameof(curves));

            if (curves.Count == 0)
                throw new ArgumentException("Curve collection must contain at least one curve.", nameof(curves));

            for (int i = 0; i < curves.Count; i++)
            {
                if (curves[i] is null)
                    throw new ArgumentException("Curve collection must not contain null curves.", nameof(curves));
            }

            if (float.IsNaN(curveWidth) || float.IsInfinity(curveWidth) || curveWidth < 0f)
                throw new ArgumentOutOfRangeException(nameof(curveWidth), curveWidth, "Curve width must be finite and non-negative.");

            if (float.IsNaN(fadeDistance) || float.IsInfinity(fadeDistance) || fadeDistance < 0f)
                throw new ArgumentOutOfRangeException(nameof(fadeDistance), fadeDistance, "Fade distance must be finite and non-negative.");

            return RasterizeCurves<T, Gray8BitColor, Gray8BitCurveColorMapper>(
                curves,
                rasterGeometry,
                new Gray8BitCurveColorMapper(curveWidth, fadeDistance, curveColor, backgroundColor));
        }

        /// <summary>
        /// Rasterizes a curve into an 8-bit grayscale raster using distance, stroke width, and fade distance.
        /// </summary>
        /// <typeparam name="T">The curve type.</typeparam>
        /// <param name="curve">The curve to rasterize.</param>
        /// <param name="curveWidth">The full curve stroke width in world coordinate units.</param>
        /// <param name="fadeDistance">The non-negative fade distance outside the stroke edge, in world coordinate units.</param>
        /// <param name="curveColor">The grayscale value used inside the stroke.</param>
        /// <param name="backgroundColor">The grayscale value used outside the stroke and fade band.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster produced from the curve distance at each cell center.</returns>
        public static SpatialRaster<Gray8BitColor> Rasterize<T>(
            this T curve,
            float curveWidth,
            float fadeDistance,
            Gray8BitColor curveColor,
            Gray8BitColor backgroundColor,
            RasterGeometry rasterGeometry)
            where T : ICurve
        {
            if (curve is null)
                throw new ArgumentNullException(nameof(curve));

            return new[] { curve }.Rasterize(
                curveWidth,
                fadeDistance,
                curveColor,
                backgroundColor,
                rasterGeometry);
        }

        /// <summary>
        /// Rasterizes curves into a 16-bit grayscale raster using nearest distance, stroke width, and fade distance.
        /// </summary>
        /// <typeparam name="T">The curve type.</typeparam>
        /// <param name="curves">The curves to rasterize.</param>
        /// <param name="curveWidth">The full curve stroke width in world coordinate units.</param>
        /// <param name="fadeDistance">The non-negative fade distance outside the stroke edge, in world coordinate units.</param>
        /// <param name="curveColor">The grayscale value used inside the stroke.</param>
        /// <param name="backgroundColor">The grayscale value used outside the stroke and fade band.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the nearest curve distance at each cell center.</returns>
        public static SpatialRaster<Gray16BitColor> Rasterize<T>(
            this IReadOnlyList<T> curves,
            float curveWidth,
            float fadeDistance,
            Gray16BitColor curveColor,
            Gray16BitColor backgroundColor,
            RasterGeometry rasterGeometry)
            where T : ICurve
        {
            if (curves == null)
                throw new ArgumentNullException(nameof(curves));

            if (curves.Count == 0)
                throw new ArgumentException("Curve collection must contain at least one curve.", nameof(curves));

            for (int i = 0; i < curves.Count; i++)
            {
                if (curves[i] is null)
                    throw new ArgumentException("Curve collection must not contain null curves.", nameof(curves));
            }

            if (float.IsNaN(curveWidth) || float.IsInfinity(curveWidth) || curveWidth < 0f)
                throw new ArgumentOutOfRangeException(nameof(curveWidth), curveWidth, "Curve width must be finite and non-negative.");

            if (float.IsNaN(fadeDistance) || float.IsInfinity(fadeDistance) || fadeDistance < 0f)
                throw new ArgumentOutOfRangeException(nameof(fadeDistance), fadeDistance, "Fade distance must be finite and non-negative.");

            return RasterizeCurves<T, Gray16BitColor, Gray16BitCurveColorMapper>(
                curves,
                rasterGeometry,
                new Gray16BitCurveColorMapper(curveWidth, fadeDistance, curveColor, backgroundColor));
        }

        /// <summary>
        /// Rasterizes a curve into a 16-bit grayscale raster using distance, stroke width, and fade distance.
        /// </summary>
        /// <typeparam name="T">The curve type.</typeparam>
        /// <param name="curve">The curve to rasterize.</param>
        /// <param name="curveWidth">The full curve stroke width in world coordinate units.</param>
        /// <param name="fadeDistance">The non-negative fade distance outside the stroke edge, in world coordinate units.</param>
        /// <param name="curveColor">The grayscale value used inside the stroke.</param>
        /// <param name="backgroundColor">The grayscale value used outside the stroke and fade band.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the curve distance at each cell center.</returns>
        public static SpatialRaster<Gray16BitColor> Rasterize<T>(
            this T curve,
            float curveWidth,
            float fadeDistance,
            Gray16BitColor curveColor,
            Gray16BitColor backgroundColor,
            RasterGeometry rasterGeometry)
            where T : ICurve
        {
            if (curve is null)
                throw new ArgumentNullException(nameof(curve));

            return new[] { curve }.Rasterize(
                curveWidth,
                fadeDistance,
                curveColor,
                backgroundColor,
                rasterGeometry);
        }

        /// <summary>
        /// Rasterizes curves into an 8-bit RGBA raster using nearest distance, stroke width, and fade distance.
        /// </summary>
        /// <typeparam name="T">The curve type.</typeparam>
        /// <param name="curves">The curves to rasterize.</param>
        /// <param name="curveWidth">The full curve stroke width in world coordinate units.</param>
        /// <param name="fadeDistance">The non-negative fade distance outside the stroke edge, in world coordinate units.</param>
        /// <param name="curveColor">The RGBA color used inside the stroke.</param>
        /// <param name="backgroundColor">The RGBA color used outside the stroke and fade band.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit RGBA raster produced from the nearest curve distance at each cell center.</returns>
        public static SpatialRaster<RGBA8BitColor> Rasterize<T>(
            this IReadOnlyList<T> curves,
            float curveWidth,
            float fadeDistance,
            RGBA8BitColor curveColor,
            RGBA8BitColor backgroundColor,
            RasterGeometry rasterGeometry)
            where T : ICurve
        {
            if (curves == null)
                throw new ArgumentNullException(nameof(curves));

            if (curves.Count == 0)
                throw new ArgumentException("Curve collection must contain at least one curve.", nameof(curves));

            for (int i = 0; i < curves.Count; i++)
            {
                if (curves[i] is null)
                    throw new ArgumentException("Curve collection must not contain null curves.", nameof(curves));
            }

            if (float.IsNaN(curveWidth) || float.IsInfinity(curveWidth) || curveWidth < 0f)
                throw new ArgumentOutOfRangeException(nameof(curveWidth), curveWidth, "Curve width must be finite and non-negative.");

            if (float.IsNaN(fadeDistance) || float.IsInfinity(fadeDistance) || fadeDistance < 0f)
                throw new ArgumentOutOfRangeException(nameof(fadeDistance), fadeDistance, "Fade distance must be finite and non-negative.");

            return RasterizeCurves<T, RGBA8BitColor, RGBA8BitCurveColorMapper>(
                curves,
                rasterGeometry,
                new RGBA8BitCurveColorMapper(curveWidth, fadeDistance, curveColor, backgroundColor));
        }

        /// <summary>
        /// Rasterizes a curve into an 8-bit RGBA raster using distance, stroke width, and fade distance.
        /// </summary>
        /// <typeparam name="T">The curve type.</typeparam>
        /// <param name="curve">The curve to rasterize.</param>
        /// <param name="curveWidth">The full curve stroke width in world coordinate units.</param>
        /// <param name="fadeDistance">The non-negative fade distance outside the stroke edge, in world coordinate units.</param>
        /// <param name="curveColor">The RGBA color used inside the stroke.</param>
        /// <param name="backgroundColor">The RGBA color used outside the stroke and fade band.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit RGBA raster produced from the curve distance at each cell center.</returns>
        public static SpatialRaster<RGBA8BitColor> Rasterize<T>(
            this T curve,
            float curveWidth,
            float fadeDistance,
            RGBA8BitColor curveColor,
            RGBA8BitColor backgroundColor,
            RasterGeometry rasterGeometry)
            where T : ICurve
        {
            if (curve is null)
                throw new ArgumentNullException(nameof(curve));

            return new[] { curve }.Rasterize(
                curveWidth,
                fadeDistance,
                curveColor,
                backgroundColor,
                rasterGeometry);
        }

        /// <summary>
        /// Rasterizes curves into a 16-bit RGBA raster using nearest distance, stroke width, and fade distance.
        /// </summary>
        /// <typeparam name="T">The curve type.</typeparam>
        /// <param name="curves">The curves to rasterize.</param>
        /// <param name="curveWidth">The full curve stroke width in world coordinate units.</param>
        /// <param name="fadeDistance">The non-negative fade distance outside the stroke edge, in world coordinate units.</param>
        /// <param name="curveColor">The RGBA color used inside the stroke.</param>
        /// <param name="backgroundColor">The RGBA color used outside the stroke and fade band.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit RGBA raster produced from the nearest curve distance at each cell center.</returns>
        public static SpatialRaster<RGBA16BitColor> Rasterize<T>(
            this IReadOnlyList<T> curves,
            float curveWidth,
            float fadeDistance,
            RGBA16BitColor curveColor,
            RGBA16BitColor backgroundColor,
            RasterGeometry rasterGeometry)
            where T : ICurve
        {
            if (curves == null)
                throw new ArgumentNullException(nameof(curves));

            if (curves.Count == 0)
                throw new ArgumentException("Curve collection must contain at least one curve.", nameof(curves));

            for (int i = 0; i < curves.Count; i++)
            {
                if (curves[i] is null)
                    throw new ArgumentException("Curve collection must not contain null curves.", nameof(curves));
            }

            if (float.IsNaN(curveWidth) || float.IsInfinity(curveWidth) || curveWidth < 0f)
                throw new ArgumentOutOfRangeException(nameof(curveWidth), curveWidth, "Curve width must be finite and non-negative.");

            if (float.IsNaN(fadeDistance) || float.IsInfinity(fadeDistance) || fadeDistance < 0f)
                throw new ArgumentOutOfRangeException(nameof(fadeDistance), fadeDistance, "Fade distance must be finite and non-negative.");

            return RasterizeCurves<T, RGBA16BitColor, RGBA16BitCurveColorMapper>(
                curves,
                rasterGeometry,
                new RGBA16BitCurveColorMapper(curveWidth, fadeDistance, curveColor, backgroundColor));
        }

        /// <summary>
        /// Rasterizes a curve into a 16-bit RGBA raster using distance, stroke width, and fade distance.
        /// </summary>
        /// <typeparam name="T">The curve type.</typeparam>
        /// <param name="curve">The curve to rasterize.</param>
        /// <param name="curveWidth">The full curve stroke width in world coordinate units.</param>
        /// <param name="fadeDistance">The non-negative fade distance outside the stroke edge, in world coordinate units.</param>
        /// <param name="curveColor">The RGBA color used inside the stroke.</param>
        /// <param name="backgroundColor">The RGBA color used outside the stroke and fade band.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit RGBA raster produced from the curve distance at each cell center.</returns>
        public static SpatialRaster<RGBA16BitColor> Rasterize<T>(
            this T curve,
            float curveWidth,
            float fadeDistance,
            RGBA16BitColor curveColor,
            RGBA16BitColor backgroundColor,
            RasterGeometry rasterGeometry)
            where T : ICurve
        {
            if (curve is null)
                throw new ArgumentNullException(nameof(curve));

            return new[] { curve }.Rasterize(
                curveWidth,
                fadeDistance,
                curveColor,
                backgroundColor,
                rasterGeometry);
        }

        private static SpatialRaster<TColor> RasterizeCurves<TCurve, TColor, TColorMapper>(
            IReadOnlyList<TCurve> curves,
            RasterGeometry rasterGeometry,
            TColorMapper colorMapper)
            where TCurve : ICurve
            where TColorMapper : struct, ICurveColorMapper<TColor>
        {
            var sampler = new CurveRasterSampler<TCurve, TColor, TColorMapper>(curves, colorMapper);
            return SpatialRasterizationCore<TColor>.Rasterize(rasterGeometry, sampler, nameof(rasterGeometry));
        }

        private interface ICurveColorMapper<TColor>
        {
            TColor Map(float distance);
        }

        private struct CurveRasterSampler<TCurve, TColor, TColorMapper> : ISpatialRasterSampler<TColor>
            where TCurve : ICurve
            where TColorMapper : struct, ICurveColorMapper<TColor>
        {
            private readonly IReadOnlyList<TCurve> _curves;
            private TColorMapper _colorMapper;

            public CurveRasterSampler(IReadOnlyList<TCurve> curves, TColorMapper colorMapper)
            {
                _curves = curves;
                _colorMapper = colorMapper;
            }

            public TColor Sample(PointXY point) => _colorMapper.Map(GetNearestDistance(_curves, point));
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Gray8BitCurveColorMapper : ICurveColorMapper<Gray8BitColor>
        {
            private readonly float _curveWidth;
            private readonly float _fadeDistance;
            private readonly Gray8BitColor _curveColor;
            private readonly Gray8BitColor _backgroundColor;

            public Gray8BitCurveColorMapper(
                float curveWidth,
                float fadeDistance,
                Gray8BitColor curveColor,
                Gray8BitColor backgroundColor)
            {
                _curveWidth = curveWidth;
                _fadeDistance = fadeDistance;
                _curveColor = curveColor;
                _backgroundColor = backgroundColor;
            }

            public Gray8BitColor Map(float distance) =>
                MapDistanceToColor(distance, _curveWidth, _fadeDistance, _curveColor, _backgroundColor);
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Gray16BitCurveColorMapper : ICurveColorMapper<Gray16BitColor>
        {
            private readonly float _curveWidth;
            private readonly float _fadeDistance;
            private readonly Gray16BitColor _curveColor;
            private readonly Gray16BitColor _backgroundColor;

            public Gray16BitCurveColorMapper(
                float curveWidth,
                float fadeDistance,
                Gray16BitColor curveColor,
                Gray16BitColor backgroundColor)
            {
                _curveWidth = curveWidth;
                _fadeDistance = fadeDistance;
                _curveColor = curveColor;
                _backgroundColor = backgroundColor;
            }

            public Gray16BitColor Map(float distance) =>
                MapDistanceToColor(distance, _curveWidth, _fadeDistance, _curveColor, _backgroundColor);
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct RGBA8BitCurveColorMapper : ICurveColorMapper<RGBA8BitColor>
        {
            private readonly float _curveWidth;
            private readonly float _fadeDistance;
            private readonly RGBA8BitColor _curveColor;
            private readonly RGBA8BitColor _backgroundColor;

            public RGBA8BitCurveColorMapper(
                float curveWidth,
                float fadeDistance,
                RGBA8BitColor curveColor,
                RGBA8BitColor backgroundColor)
            {
                _curveWidth = curveWidth;
                _fadeDistance = fadeDistance;
                _curveColor = curveColor;
                _backgroundColor = backgroundColor;
            }

            public RGBA8BitColor Map(float distance) =>
                MapDistanceToColor(distance, _curveWidth, _fadeDistance, _curveColor, _backgroundColor);
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct RGBA16BitCurveColorMapper : ICurveColorMapper<RGBA16BitColor>
        {
            private readonly float _curveWidth;
            private readonly float _fadeDistance;
            private readonly RGBA16BitColor _curveColor;
            private readonly RGBA16BitColor _backgroundColor;

            public RGBA16BitCurveColorMapper(
                float curveWidth,
                float fadeDistance,
                RGBA16BitColor curveColor,
                RGBA16BitColor backgroundColor)
            {
                _curveWidth = curveWidth;
                _fadeDistance = fadeDistance;
                _curveColor = curveColor;
                _backgroundColor = backgroundColor;
            }

            public RGBA16BitColor Map(float distance) =>
                MapDistanceToColor(distance, _curveWidth, _fadeDistance, _curveColor, _backgroundColor);
        }

        private static float GetNearestDistance<T>(IReadOnlyList<T> curves, PointXY point)
            where T : ICurve
        {
            float minDistance = curves[0].Distance(point);

            for (int i = 1; i < curves.Count; i++)
            {
                float distance = curves[i].Distance(point);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        private static Gray8BitColor MapDistanceToColor(
            float distance,
            float curveWidth,
            float fadeDistance,
            Gray8BitColor curveColor,
            Gray8BitColor backgroundColor)
        {
            float backgroundAmount = GetBackgroundBlendAmount(distance, curveWidth, fadeDistance);

            if (backgroundAmount <= 0f)
                return curveColor;

            if (backgroundAmount >= 1f)
                return backgroundColor;

            return Gray8BitColor.Blend(curveColor, backgroundColor, backgroundAmount);
        }

        private static Gray16BitColor MapDistanceToColor(
            float distance,
            float curveWidth,
            float fadeDistance,
            Gray16BitColor curveColor,
            Gray16BitColor backgroundColor)
        {
            float backgroundAmount = GetBackgroundBlendAmount(distance, curveWidth, fadeDistance);

            if (backgroundAmount <= 0f)
                return curveColor;

            if (backgroundAmount >= 1f)
                return backgroundColor;

            return Gray16BitColor.Blend(curveColor, backgroundColor, backgroundAmount);
        }

        private static RGBA8BitColor MapDistanceToColor(
            float distance,
            float curveWidth,
            float fadeDistance,
            RGBA8BitColor curveColor,
            RGBA8BitColor backgroundColor)
        {
            float backgroundAmount = GetBackgroundBlendAmount(distance, curveWidth, fadeDistance);

            if (backgroundAmount <= 0f)
                return curveColor;

            if (backgroundAmount >= 1f)
                return backgroundColor;

            return RGBA8BitColor.Blend(curveColor, backgroundColor, backgroundAmount);
        }

        private static RGBA16BitColor MapDistanceToColor(
            float distance,
            float curveWidth,
            float fadeDistance,
            RGBA16BitColor curveColor,
            RGBA16BitColor backgroundColor)
        {
            float backgroundAmount = GetBackgroundBlendAmount(distance, curveWidth, fadeDistance);

            if (backgroundAmount <= 0f)
                return curveColor;

            if (backgroundAmount >= 1f)
                return backgroundColor;

            return RGBA16BitColor.Blend(curveColor, backgroundColor, backgroundAmount);
        }

        private static float GetBackgroundBlendAmount(float distance, float curveWidth, float fadeDistance)
        {
            float halfCurveWidth = curveWidth * 0.5f;
            if (distance <= halfCurveWidth)
                return 0f;

            if (fadeDistance == 0f)
                return 1f;

            float fadePosition = (distance - halfCurveWidth) / fadeDistance;
            if (fadePosition >= 1f)
                return 1f;

            return fadePosition;
        }

        private static byte ToByte(float value)
        {
            if (value <= byte.MinValue)
                return byte.MinValue;

            if (value >= byte.MaxValue)
                return byte.MaxValue;

            return (byte)MathF.Round(value);
        }

        private static ushort ToUInt16(float value)
        {
            if (value <= ushort.MinValue)
                return ushort.MinValue;

            if (value >= ushort.MaxValue)
                return ushort.MaxValue;

            return (ushort)MathF.Round(value);
        }

    }
}
