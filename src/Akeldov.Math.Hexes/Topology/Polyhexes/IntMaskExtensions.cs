namespace Akeldov.Math.Hexes.Topology
{
    internal static partial class IntMaskExtensions
    {
        public static bool[,] ToBoolMask(this int[,] mask)
        {
            var x = mask.GetLength(0);
            var y = mask.GetLength(1);
            var res = new bool[x, y];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    res[i, j] = mask[i, j] == 0 ? false : true;
                }
            }

            return res;
        }
    }
}
