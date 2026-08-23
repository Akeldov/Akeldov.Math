using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields.Influence;

internal static class InfluencePngRenderer
{
    private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    private static readonly Rgba[] SourcePalette =
    {
        new Rgba(239, 68, 68, 255),
        new Rgba(34, 197, 94, 255),
        new Rgba(59, 130, 246, 255),
        new Rgba(245, 158, 11, 255),
        new Rgba(168, 85, 247, 255)
    };

    public static byte[] RenderSamplerWeights(
        IInfluenceSampler<FloatPointInfluenceSource, float> sampler,
        IReadOnlyList<FloatPointInfluenceSource> sources,
        VectorXY fieldSize,
        int width,
        int height)
    {
        if (sources.Count != 3)
            throw new ArgumentException("Sampler weight snapshot requires exactly three sources.", nameof(sources));

        var redSources = CreateWeightSources(sources, activeIndex: 0);
        var greenSources = CreateWeightSources(sources, activeIndex: 1);
        var blueSources = CreateWeightSources(sources, activeIndex: 2);
        var pixels = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        {
            float sampleY = (y + 0.5f) * fieldSize.Y / height;

            for (int x = 0; x < width; x++)
            {
                float sampleX = (x + 0.5f) * fieldSize.X / width;
                var point = new PointXY(sampleX, sampleY);
                var color = new Rgba(
                    ToSrgbByte(sampler.Sample(redSources, point)),
                    ToSrgbByte(sampler.Sample(greenSources, point)),
                    ToSrgbByte(sampler.Sample(blueSources, point)),
                    255);

                SetPixel(pixels, width, height, x, y, color);
            }
        }

        DrawBorder(pixels, width, height);
        DrawColoredSources(pixels, width, height, fieldSize, sources);

        return EncodeRgbaPng(width, height, pixels);
    }

    public static byte[] RenderIndexSelection(
        IInfluenceSourceIndex<FloatPointInfluenceSource> sourceIndex,
        VectorXY fieldSize,
        int width,
        int height)
    {
        IReadOnlyList<FloatPointInfluenceSource> sources = sourceIndex.Sources;
        var pixels = new byte[width * height * 4];
        int[] hullIndices = GetConvexHullIndices(sources);
        var fallbackBlendColor = new Rgba(248, 250, 252, 255);

        for (int y = 0; y < height; y++)
        {
            float sampleY = (y + 0.5f) * fieldSize.Y / height;

            for (int x = 0; x < width; x++)
            {
                float sampleX = (x + 0.5f) * fieldSize.X / width;
                var point = new PointXY(sampleX, sampleY);
                List<FloatPointInfluenceSource> selectedSources = sourceIndex.SelectSources(point);
                Rgba color = SelectionColor(sources, selectedSources);

                if (!PointInPolygon(point, sources, hullIndices))
                    color = Blend(color, fallbackBlendColor, 0.72f);

                SetPixel(pixels, width, height, x, y, color);
            }
        }

        DrawBorder(pixels, width, height);
        DrawHull(pixels, width, height, fieldSize, sources, hullIndices);
        DrawColoredSources(pixels, width, height, fieldSize, sources);

        return EncodeRgbaPng(width, height, pixels);
    }

    private static void DrawBorder(byte[] pixels, int width, int height)
    {
        var color = new Rgba(51, 65, 85, 255);

        for (int x = 0; x < width; x++)
        {
            SetPixel(pixels, width, height, x, 0, color);
            SetPixel(pixels, width, height, x, height - 1, color);
        }

        for (int y = 0; y < height; y++)
        {
            SetPixel(pixels, width, height, 0, y, color);
            SetPixel(pixels, width, height, width - 1, y, color);
        }
    }

    private static void DrawColoredSources(
        byte[] pixels,
        int width,
        int height,
        VectorXY fieldSize,
        IReadOnlyList<FloatPointInfluenceSource> sources)
    {
        var stroke = new Rgba(248, 250, 252, 255);

        for (int i = 0; i < sources.Count; i++)
        {
            int x = ToPixelX(sources[i].Position, width, fieldSize);
            int y = ToPixelY(sources[i].Position, height, fieldSize);

            DrawCircle(pixels, width, height, x, y, 7, stroke);
            DrawCircle(pixels, width, height, x, y, 5, SourceColor(i));
        }
    }

    private static void DrawHull(
        byte[] pixels,
        int width,
        int height,
        VectorXY fieldSize,
        IReadOnlyList<FloatPointInfluenceSource> sources,
        IReadOnlyList<int> hullIndices)
    {
        var color = new Rgba(15, 23, 42, 255);

        for (int i = 0; i < hullIndices.Count; i++)
        {
            PointXY start = sources[hullIndices[i]].Position;
            PointXY end = sources[hullIndices[(i + 1) % hullIndices.Count]].Position;

            DrawLine(
                pixels,
                width,
                height,
                ToPixelX(start, width, fieldSize),
                ToPixelY(start, height, fieldSize),
                ToPixelX(end, width, fieldSize),
                ToPixelY(end, height, fieldSize),
                color);
        }
    }

    private static void DrawCircle(
        byte[] pixels,
        int width,
        int height,
        int centerX,
        int centerY,
        int radius,
        Rgba color)
    {
        int radiusSquared = radius * radius;

        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            int dy = y - centerY;

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                int dx = x - centerX;
                if (dx * dx + dy * dy > radiusSquared)
                    continue;

                SetPixel(pixels, width, height, x, y, color);
            }
        }
    }

    private static void DrawLine(
        byte[] pixels,
        int width,
        int height,
        int x0,
        int y0,
        int x1,
        int y1,
        Rgba color)
    {
        int dx = System.Math.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -System.Math.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;

        while (true)
        {
            DrawCircle(pixels, width, height, x0, y0, 1, color);

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private static FloatPointInfluenceSource[] CreateWeightSources(
        IReadOnlyList<FloatPointInfluenceSource> sources,
        int activeIndex)
    {
        return new[]
        {
            new FloatPointInfluenceSource(sources[0].Weight, sources[0].Position, activeIndex == 0 ? 1f : 0f),
            new FloatPointInfluenceSource(sources[1].Weight, sources[1].Position, activeIndex == 1 ? 1f : 0f),
            new FloatPointInfluenceSource(sources[2].Weight, sources[2].Position, activeIndex == 2 ? 1f : 0f)
        };
    }

    private static byte ToSrgbByte(float value)
    {
        float clamped = MathF.Max(0f, MathF.Min(1f, value));
        float srgb = clamped <= 0.0031308f
            ? clamped * 12.92f
            : 1.055f * MathF.Pow(clamped, 1f / 2.4f) - 0.055f;

        return (byte)MathF.Round(srgb * 255f);
    }

    private static Rgba SelectionColor(
        IReadOnlyList<FloatPointInfluenceSource> sources,
        IReadOnlyList<FloatPointInfluenceSource> selectedSources)
    {
        int count = selectedSources.Count;
        if (count == 0)
            return new Rgba(226, 232, 240, 255);

        float red = 0f;
        float green = 0f;
        float blue = 0f;

        for (int i = 0; i < selectedSources.Count; i++)
        {
            Rgba color = SourceColor(IndexOfSource(sources, selectedSources[i]));
            red += SrgbByteToLinear(color.Red);
            green += SrgbByteToLinear(color.Green);
            blue += SrgbByteToLinear(color.Blue);
        }

        return new Rgba(
            ToSrgbByte(red / count),
            ToSrgbByte(green / count),
            ToSrgbByte(blue / count),
            255);
    }

    private static float SrgbByteToLinear(byte value)
    {
        float srgb = value / 255f;
        return srgb <= 0.04045f
            ? srgb / 12.92f
            : MathF.Pow((srgb + 0.055f) / 1.055f, 2.4f);
    }

    private static int IndexOfSource(
        IReadOnlyList<FloatPointInfluenceSource> sources,
        FloatPointInfluenceSource selectedSource)
    {
        for (int i = 0; i < sources.Count; i++)
        {
            if (ReferenceEquals(sources[i], selectedSource))
                return i;
        }

        throw new InvalidOperationException("Selected source does not belong to the index snapshot.");
    }

    private static Rgba SourceColor(int index)
    {
        return SourcePalette[index % SourcePalette.Length];
    }

    private static int[] GetConvexHullIndices(IReadOnlyList<FloatPointInfluenceSource> sources)
    {
        if (sources.Count <= 3)
        {
            var allIndices = new int[sources.Count];
            for (int i = 0; i < sources.Count; i++)
                allIndices[i] = i;

            return allIndices;
        }

        int start = 0;
        for (int i = 1; i < sources.Count; i++)
        {
            PointXY candidate = sources[i].Position;
            PointXY current = sources[start].Position;
            if (candidate.X < current.X || (candidate.X == current.X && candidate.Y < current.Y))
                start = i;
        }

        var hull = new List<int>();
        int currentIndex = start;

        do
        {
            hull.Add(currentIndex);
            int nextIndex = currentIndex == 0 ? 1 : 0;

            for (int i = 0; i < sources.Count; i++)
            {
                if (i == currentIndex)
                    continue;

                PointXY current = sources[currentIndex].Position;
                PointXY next = sources[nextIndex].Position;
                PointXY candidate = sources[i].Position;
                float cross = Cross(next - current, candidate - current);

                if (cross < -GeometryConstants.GeometryEpsilon ||
                    (cross.IsAlmostZero() && DistanceSquared(current, candidate) > DistanceSquared(current, next)))
                {
                    nextIndex = i;
                }
            }

            currentIndex = nextIndex;
        }
        while (currentIndex != start && hull.Count <= sources.Count);

        return hull.ToArray();
    }

    private static bool PointInPolygon(
        PointXY point,
        IReadOnlyList<FloatPointInfluenceSource> sources,
        IReadOnlyList<int> hullIndices)
    {
        bool isInside = false;
        int j = hullIndices.Count - 1;

        for (int i = 0; i < hullIndices.Count; i++)
        {
            PointXY a = sources[hullIndices[i]].Position;
            PointXY b = sources[hullIndices[j]].Position;

            if (PointOnSegment(point, a, b))
                return true;

            bool crossesRay = (a.Y > point.Y) != (b.Y > point.Y);
            if (crossesRay)
            {
                float intersectionX = (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X;
                if (point.X < intersectionX)
                    isInside = !isInside;
            }

            j = i;
        }

        return isInside;
    }

    private static bool PointOnSegment(PointXY point, PointXY a, PointXY b)
    {
        if (!Cross(b - a, point - a).IsAlmostZero())
            return false;

        float minX = MathF.Min(a.X, b.X) - GeometryConstants.GeometryEpsilon;
        float maxX = MathF.Max(a.X, b.X) + GeometryConstants.GeometryEpsilon;
        float minY = MathF.Min(a.Y, b.Y) - GeometryConstants.GeometryEpsilon;
        float maxY = MathF.Max(a.Y, b.Y) + GeometryConstants.GeometryEpsilon;

        return point.X >= minX && point.X <= maxX &&
               point.Y >= minY && point.Y <= maxY;
    }

    private static Rgba Blend(Rgba color, Rgba background, float backgroundAmount)
    {
        float foregroundAmount = 1f - backgroundAmount;

        return new Rgba(
            (byte)MathF.Round(color.Red * foregroundAmount + background.Red * backgroundAmount),
            (byte)MathF.Round(color.Green * foregroundAmount + background.Green * backgroundAmount),
            (byte)MathF.Round(color.Blue * foregroundAmount + background.Blue * backgroundAmount),
            255);
    }

    private static float Cross(VectorXY left, VectorXY right)
    {
        return left.X * right.Y - left.Y * right.X;
    }

    private static float DistanceSquared(PointXY left, PointXY right)
    {
        float dx = left.X - right.X;
        float dy = left.Y - right.Y;
        return dx * dx + dy * dy;
    }

    private static int ToPixelX(PointXY point, int width, VectorXY fieldSize)
    {
        return (int)MathF.Round(point.X * (width - 1) / fieldSize.X);
    }

    private static int ToPixelY(PointXY point, int height, VectorXY fieldSize)
    {
        return (int)MathF.Round(point.Y * (height - 1) / fieldSize.Y);
    }

    private static void SetPixel(byte[] pixels, int width, int height, int x, int y, Rgba color)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
            return;

        int offset = (y * width + x) * 4;
        pixels[offset] = color.Red;
        pixels[offset + 1] = color.Green;
        pixels[offset + 2] = color.Blue;
        pixels[offset + 3] = color.Alpha;
    }

    private static byte[] EncodeRgbaPng(int width, int height, byte[] pixels)
    {
        int rowLength = width * 4;
        var scanlines = new byte[height * (rowLength + 1)];

        for (int y = 0; y < height; y++)
        {
            int targetOffset = y * (rowLength + 1) + 1;
            int sourceOffset = y * rowLength;
            Array.Copy(pixels, sourceOffset, scanlines, targetOffset, rowLength);
        }

        using var stream = new MemoryStream();
        stream.Write(PngSignature);
        WriteChunk(stream, "IHDR", CreateHeader(width, height));
        WriteChunk(stream, "IDAT", CreateZlibStoredData(scanlines));
        WriteChunk(stream, "IEND", Array.Empty<byte>());
        return stream.ToArray();
    }

    private static byte[] CreateHeader(int width, int height)
    {
        var header = new byte[13];
        WriteUInt32(header, 0, (uint)width);
        WriteUInt32(header, 4, (uint)height);
        header[8] = 8;
        header[9] = 6;
        header[10] = 0;
        header[11] = 0;
        header[12] = 0;
        return header;
    }

    private static byte[] CreateZlibStoredData(byte[] data)
    {
        using var stream = new MemoryStream();
        stream.WriteByte(0x78);
        stream.WriteByte(0x01);

        int offset = 0;
        while (offset < data.Length)
        {
            int length = System.Math.Min(65535, data.Length - offset);
            bool isFinal = offset + length == data.Length;
            stream.WriteByte(isFinal ? (byte)1 : (byte)0);
            WriteUInt16LittleEndian(stream, (ushort)length);
            WriteUInt16LittleEndian(stream, (ushort)~length);
            stream.Write(data, offset, length);
            offset += length;
        }

        WriteUInt32(stream, Adler32(data));
        return stream.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        byte[] typeBytes = System.Text.Encoding.ASCII.GetBytes(type);

        WriteUInt32(stream, (uint)data.Length);
        stream.Write(typeBytes);
        stream.Write(data);
        WriteUInt32(stream, Crc32(typeBytes, data));
    }

    private static void WriteUInt16LittleEndian(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value & 0xff));
        stream.WriteByte((byte)(value >> 8));
    }

    private static void WriteUInt32(Stream stream, uint value)
    {
        stream.WriteByte((byte)(value >> 24));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    private static void WriteUInt32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;
    }

    private static uint Adler32(byte[] data)
    {
        const uint mod = 65521;
        uint a = 1;
        uint b = 0;

        for (int i = 0; i < data.Length; i++)
        {
            a = (a + data[i]) % mod;
            b = (b + a) % mod;
        }

        return (b << 16) | a;
    }

    private static uint Crc32(byte[] typeBytes, byte[] data)
    {
        uint crc = 0xffffffff;

        for (int i = 0; i < typeBytes.Length; i++)
            crc = UpdateCrc32(crc, typeBytes[i]);

        for (int i = 0; i < data.Length; i++)
            crc = UpdateCrc32(crc, data[i]);

        return crc ^ 0xffffffff;
    }

    private static uint UpdateCrc32(uint crc, byte value)
    {
        crc ^= value;

        for (int i = 0; i < 8; i++)
        {
            if ((crc & 1) == 0)
                crc >>= 1;
            else
                crc = (crc >> 1) ^ 0xedb88320;
        }

        return crc;
    }

    private readonly struct Rgba
    {
        public Rgba(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public byte Red { get; }

        public byte Green { get; }

        public byte Blue { get; }

        public byte Alpha { get; }
    }
}
