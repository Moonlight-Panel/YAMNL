namespace YAMNL.Types
{
    public class Position
    {

        public Position(ulong value)
        {
            X = (int)(value >> 38);
            Y = (int)(value & 0xFFF);
            Z = (int)(value >> 12 & 0x3FFFFFF);

            if (X >= Math.Pow(2, 25)) { X -= (int)Math.Pow(2, 26); }
            if (Y >= Math.Pow(2, 11)) { Y -= (int)Math.Pow(2, 12); }
            if (Z >= Math.Pow(2, 25)) { Z -= (int)Math.Pow(2, 26); }
        }

        public Position(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X
        {
            get;
        }
        public int Y
        {
            get;
        }
        public int Z
        {
            get;
        }

        public ulong ToULong() => ((ulong)X & 0x3FFFFFF) << 38 | ((ulong)Z & 0x3FFFFFF) << 12 | (ulong)Y & 0xFFF;

        public override string ToString() => $"({X} / {Y} / {Z})";

        //public static implicit operator Vector3(Position x) => new Vector3(x.X, x.Y, x.Z);
        //public static explicit operator Position(Vector3 x) => new Position((int)x.X, (int)x.Y, (int) x.Z);
    }
}
