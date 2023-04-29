namespace YAMNL.Types
{
    public class Vector3
    {

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 One => new Vector3(1, 1, 1);
        public static Vector3 Zero => new Vector3(0, 0, 0);
        public static Vector3 Up => new Vector3(0, 1, 0);
        public static Vector3 Down => new Vector3(0, -1, 0);
        public static Vector3 North => new Vector3(0, 0, -1);
        public static Vector3 South => new Vector3(0, 0, 1);
        public static Vector3 West => new Vector3(-1, 0, 0);
        public static Vector3 East => new Vector3(1, 0, 0);

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public static implicit operator Position(Vector3 x) => new Position((int)Math.Floor(x.X), (int)Math.Ceiling(x.Y), (int)Math.Floor(x.Z));
        public static explicit operator Vector3(Position x) => new Vector3(x.X, x.Y, x.Z);

        public void Add(Vector3 v)
        {
            X += v.X;
            Y += v.Y;
            Z += v.Z;
        }

        public void Subtract(Vector3 v)
        {
            X -= v.X;
            Y -= v.Y;
            Z -= v.Z;
        }

        public Vector3 Normalized() => this * (1 / Length());

        public Vector3 Clone() => new Vector3(X, Y, Z);

        public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3 Floored() => new Vector3(Math.Floor(X), Math.Floor(Y + 0.001), Math.Floor(Z));

        public Vector3 Plus(Vector3 v) => new Vector3(X + v.X, Y + v.Y, Z + v.Z);

        public Vector3 Minus(Vector3 v) => new Vector3(X - v.X, Y - v.Y, Z - v.Z);

        public void Mul(Vector3 v)
        {
            X *= v.X;
            Y *= v.Y;
            Z *= v.Z;
        }

        public double DistanceSquared(Vector3 v)
        {
            var diff = Minus(v);
            return diff.X * diff.X + diff.Y * diff.Y + diff.Z * diff.Z;
        }

        public double Distance(Vector3 v) => Math.Sqrt(DistanceSquared(v));

        public override string ToString() => $"({X.ToString("0.##")} / {Y.ToString("0.##")} / {Z.ToString("0.##")})";


        public static Vector3 operator *(Vector3 v, int val) => new Vector3(v.X * val, v.Y * val, v.Z * val);

        public static Vector3 operator *(Vector3 v, double val) => new Vector3(v.X * val, v.Y * val, v.Z * val);

        public static Vector3 operator /(Vector3 v, int val) => new Vector3(v.X / val, v.Y / val, v.Z / val);

        public static bool operator ==(Vector3 v1, Vector3 v2) => v1.X == v2.X &&
                                                                  v1.Y == v2.Y &&
                                                                  v1.Z == v2.Z;

        public static bool operator !=(Vector3 v1, Vector3 v2) => v1.X != v2.X ||
                                                                  v1.Y != v2.Y ||
                                                                  v1.Z != v2.Z;

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(Vector3))
            {
                return false;
            }

            return this == (Vector3)obj;
        }

        public override int GetHashCode()
        {
            var hash = X.GetHashCode();
            hash = (hash << 5) + hash ^ Y.GetHashCode();
            hash = (hash << 5) + hash ^ Z.GetHashCode();
            return hash;
        }
    }
}
