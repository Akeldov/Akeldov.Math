using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public class PolyhexGeometry : IPolyhexGeometry
    {
        private readonly Polyhex _polyhex;
        private readonly float _hexApothem;
        private readonly float _hexRadius;

        public PolyhexGeometry(Polyhex polyhex, float apothem)
        {
            _polyhex = polyhex ?? throw new ArgumentNullException(nameof(polyhex));

            if (float.IsNaN(apothem) || float.IsInfinity(apothem) || apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(apothem), apothem, "Hex apothem must be finite and positive.");

            _hexApothem = apothem;
            _hexRadius = _hexApothem.ConvertHexApothemToRadius();
        }

        public PolyhexGeometry(bool[,] boolMask, float apothem)
            : this(new Polyhex(boolMask), apothem)
        {
        }

        public PolyhexGeometry(int[,] intMask, float apothem)
            : this(new Polyhex(intMask), apothem)
        {
        }

        public PolyhexGeometry(VectorQRSInt qrsResolution, float apothem)
            : this(new Polyhex(qrsResolution), apothem)
        {
        }

        public VectorQRSInt QRSResolution => _polyhex.QRSResolution;

        public int HexCount => _polyhex.HexCount;

        public int PositiveSize => HexCount;

        public bool this[VectorQRSInt index]
        {
            get => _polyhex[index];
        }

        public bool this[int QIndex, int RIndex]
        {
            get => _polyhex[QIndex, RIndex];
        }

        public float HexApothem => _hexApothem;

        public float HexRadius => _hexRadius;

        public Polyhex GetExtended()
        {
            return _polyhex.GetExtended();
        }

        public Polyhex GetContour()
        {
            return _polyhex.GetContour();
        }

        public bool[,] ToBoolArray()
        {
            return _polyhex.ToBoolArray();
        }
    }
}
