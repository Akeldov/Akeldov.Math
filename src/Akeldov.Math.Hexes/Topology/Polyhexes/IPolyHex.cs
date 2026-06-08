using Akeldov.Math.Hexes.Vectors.QRS;

namespace Akeldov.Math.Hexes.Topology
{
    public interface IPolyhex
    {
        VectorQRSInt QRSResolution { get; }

        public int HexCount { get; }

        public bool this[VectorQRSInt index]
        {
            get;
        }

        public bool this[int QIndex, int RIndex]
        {
            get;
        }

        public Polyhex GetExtended();

        public Polyhex GetContour();
    }
}
