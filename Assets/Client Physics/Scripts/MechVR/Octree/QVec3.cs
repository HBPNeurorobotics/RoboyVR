using System;
using QType = System.Byte;

public struct QVec3
{
	public readonly QType x, y, z;

	public QVec3(QType x, QType y, QType z) { this.x = x; this.y = y; this.z = z; }

	public QVec3? Move(Dir to, int step, int maxWidth)
	{
		if (to == Dir.Xm && (x == QType.MinValue || x - step < 0)) return null;
		if (to == Dir.Ym && (y == QType.MinValue || y - step < 0)) return null;
		if (to == Dir.Zm && (z == QType.MinValue || z - step < 0)) return null;

		if (to == Dir.Xp && (x == QType.MaxValue || x + step >= maxWidth)) return null;
		if (to == Dir.Yp && (y == QType.MaxValue || y + step >= maxWidth)) return null;
		if (to == Dir.Zp && (z == QType.MaxValue || z + step >= maxWidth)) return null;

		switch (to)
		{
		case Dir.Xp: return new QVec3((QType)(x + step), (QType)(y + 0), (QType)(z + 0));
		case Dir.Xm: return new QVec3((QType)(x - step), (QType)(y + 0), (QType)(z + 0));
		case Dir.Yp: return new QVec3((QType)(x + 0), (QType)(y + step), (QType)(z + 0));
		case Dir.Ym: return new QVec3((QType)(x + 0), (QType)(y - step), (QType)(z + 0));
		case Dir.Zp: return new QVec3((QType)(x + 0), (QType)(y + 0), (QType)(z + step));
		case Dir.Zm: return new QVec3((QType)(x + 0), (QType)(y + 0), (QType)(z - step));
		default: throw new InvalidOperationException();
		}
	}

	public static bool operator ==(QVec3 vec1, QVec3 vec2)
	{
		return vec1.x == vec2.x && vec1.y == vec2.y && vec1.z == vec2.z;
	}

	public static bool operator !=(QVec3 vec1, QVec3 vec2)
	{
		return vec1.x != vec2.x || vec1.y != vec2.y || vec1.z != vec2.z;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is QVec3))
			return false;
		return ((QVec3)obj) == this;
	}

	public override int GetHashCode()
	{
		return x << 16 | y << 8 | z << 0;
	}

	public override string ToString() { return string.Format("({0} {1} {2})", x, y, z); }
}