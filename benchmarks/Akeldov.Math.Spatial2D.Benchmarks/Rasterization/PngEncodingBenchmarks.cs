using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using BenchmarkDotNet.Attributes;
using System.IO.Compression;

namespace Akeldov.Math.Spatial2D.Benchmarks.Rasterization;

[MemoryDiagnoser]
[ShortRunJob]
public class PngEncodingBenchmarks
{
    private Raster<RGBA8BitColor> _raster = null!;

    [Params(CompressionLevel.NoCompression, CompressionLevel.Fastest, CompressionLevel.Optimal, CompressionLevel.SmallestSize)]
    public CompressionLevel CompressionLevel { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        const int resolution = 512;
        var values = new RGBA8BitColor[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                byte red = (byte)(x / 8);
                byte green = (byte)(y / 8);
                byte blue = (byte)((x + y) / 16);
                values[y * resolution + x] = new RGBA8BitColor(red, green, blue, byte.MaxValue);
            }
        }

        _raster = new Raster<RGBA8BitColor>(new VectorXYInt(resolution, resolution), values);
    }

    [Benchmark]
    public byte[] EncodeRgba8()
    {
        using var stream = new MemoryStream();
        _raster.SaveAsPng(stream, CompressionLevel);
        return stream.ToArray();
    }
}
