namespace Akeldov.Math.Spatial2D.Imaging
{
    public readonly partial struct RGBA8BitColor
    {
        /// <summary>
        /// Represents a fully transparent 8-bit RGBA color.
        /// </summary>
        public static readonly RGBA8BitColor Transparent = default(RGBA8BitColor);

        /// <summary>
        /// Represents an opaque black 8-bit RGBA color.
        /// </summary>
        public static readonly RGBA8BitColor Black = new RGBA8BitColor(0, 0, 0, byte.MaxValue);

        /// <summary>
        /// Represents an opaque white 8-bit RGBA color.
        /// </summary>
        public static readonly RGBA8BitColor White = new RGBA8BitColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        /// <summary>
        /// Represents an opaque red 8-bit RGBA color.
        /// </summary>
        public static readonly RGBA8BitColor Red = new RGBA8BitColor(byte.MaxValue, 0, 0, byte.MaxValue);

        /// <summary>
        /// Represents an opaque green 8-bit RGBA color.
        /// </summary>
        public static readonly RGBA8BitColor Green = new RGBA8BitColor(0, byte.MaxValue, 0, byte.MaxValue);

        /// <summary>
        /// Represents an opaque blue 8-bit RGBA color.
        /// </summary>
        public static readonly RGBA8BitColor Blue = new RGBA8BitColor(0, 0, byte.MaxValue, byte.MaxValue);

        /// <summary>
        /// Represents an opaque yellow 8-bit RGBA color.
        /// </summary>
        public static readonly RGBA8BitColor Yellow = new RGBA8BitColor(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);

        /// <summary>
        /// Represents an opaque cyan 8-bit RGBA color.
        /// </summary>
        public static readonly RGBA8BitColor Cyan = new RGBA8BitColor(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        /// <summary>
        /// Represents an opaque magenta 8-bit RGBA color.
        /// </summary>
        public static readonly RGBA8BitColor Magenta = new RGBA8BitColor(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue);
    }
}
