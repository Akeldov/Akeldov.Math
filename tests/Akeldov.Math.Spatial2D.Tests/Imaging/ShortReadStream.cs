namespace Akeldov.Math.Spatial2D.Tests.Imaging;

/// <summary>
/// Exercises PNG loading from a non-seekable stream that returns partial reads.
/// </summary>
internal sealed class ShortReadStream : Stream
{
    private readonly MemoryStream _inner;
    public ShortReadStream(byte[] data) => _inner = new MemoryStream(data);
    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, System.Math.Min(count, 3));
    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _inner.Dispose();
        base.Dispose(disposing);
    }
}
