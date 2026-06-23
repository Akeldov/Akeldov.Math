namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Provides predefined 16-bit RGBA colors.
    /// </summary>
    public static class RGBA16BitColors
    {
        /// <summary>
        /// Represents a fully transparent 16-bit RGBA color.
        /// </summary>
        public static readonly RGBA16BitColor Transparent = default;

        /// <summary>
        /// Represents an opaque black 16-bit RGBA color.
        /// </summary>
        public static readonly RGBA16BitColor Black = new RGBA16BitColor(0, 0, 0, ushort.MaxValue);

        /// <summary>
        /// Represents an opaque white 16-bit RGBA color.
        /// </summary>
        public static readonly RGBA16BitColor White = new RGBA16BitColor(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue);

        /// <summary>
        /// Represents an opaque red 16-bit RGBA color.
        /// </summary>
        public static readonly RGBA16BitColor Red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);

        /// <summary>
        /// Represents an opaque green 16-bit RGBA color.
        /// </summary>
        public static readonly RGBA16BitColor Green = new RGBA16BitColor(0, ushort.MaxValue, 0, ushort.MaxValue);

        /// <summary>
        /// Represents an opaque blue 16-bit RGBA color.
        /// </summary>
        public static readonly RGBA16BitColor Blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        /// <summary>
        /// Represents an opaque yellow 16-bit RGBA color.
        /// </summary>
        public static readonly RGBA16BitColor Yellow = new RGBA16BitColor(ushort.MaxValue, ushort.MaxValue, 0, ushort.MaxValue);

        /// <summary>
        /// Represents an opaque cyan 16-bit RGBA color.
        /// </summary>
        public static readonly RGBA16BitColor Cyan = new RGBA16BitColor(0, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue);

        /// <summary>
        /// Represents an opaque magenta 16-bit RGBA color.
        /// </summary>
        public static readonly RGBA16BitColor Magenta = new RGBA16BitColor(ushort.MaxValue, 0, ushort.MaxValue, ushort.MaxValue);
    }
}
