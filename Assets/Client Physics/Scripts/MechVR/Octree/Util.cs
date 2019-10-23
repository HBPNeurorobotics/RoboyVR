using System;
using System.Linq;

public static class Util
{
	public const int OctNodes = 8;
	public const int OctDirs = 6;

	public static int GetBit(int number, int bit) { return (number >> bit) & 1; }

	public static bool AllNull<T>(T[] array)
	{
		for (int i = 0; i < array.Length; i++)
			if (array[i] != null)
				return false;
		return true;
	}

	public static readonly Pos[] Poss = (Pos[])Enum.GetValues(typeof(Pos));
	public static readonly Dir[] Dirs = (Dir[])Enum.GetValues(typeof(Dir));
	/// swaps the sign of a direction eg: Xp -> Xm or Ym -> Yp
	public static readonly Dir[] SwapDirs = { Dir.Xm, Dir.Xp, Dir.Ym, Dir.Yp, Dir.Zm, Dir.Zp };

	public static readonly Pos[][] AffectedSites // TODO rework statically
		= Dirs.Select(
			x => Poss.Where(
				p => p.ToString().Contains(x.ToString())
			).ToArray()
		).ToArray();

	private static readonly CubeMove[] CubeMoveDict;

	static Util()
	{
		CubeMoveDict = new CubeMove[OctNodes * OctDirs];
		foreach (var pos in Poss)
		{
			foreach (var dir in Dirs)
			{
				int index = ((int)dir) << 3 | ((int)pos);
				string dirStr = dir.ToString();

				bool posi = dirStr[1] == 'p';
				int bit = dirStr[0] == 'X' ? 2 : dirStr[0] == 'Y' ? 1 : 0;

				Pos target = (Pos)((int)pos ^ (1 << bit));
				bool isOutside = (GetBit((int)pos, bit) ^ (posi ? 1 : 0)) == 0;

				CubeMoveDict[index] = new CubeMove(target, isOutside);
			}
		}

		//CubeMoveDict[((int)Dir.Xp) << 3 | ((int)Pos.XpYpZp)] = new CubeMove(Pos.XmYpZp, true);
		//CubeMoveDict[((int)Dir.Xm) << 3 | ((int)Pos.XpYpZp)] = new CubeMove(Pos.XmYpZp, false);
	}

	public static CubeMove GetMove(Pos start, Dir to)
	{
		if (start == Pos.Root)
			return new CubeMove(start, false);

		int index = ((int)to) << 3 | ((int)start);
		return CubeMoveDict[index];
	}
}

public struct CubeMove
{
	public readonly Pos OutPos;
	public readonly bool IsOutsideCube;
	public CubeMove(Pos pos, bool isOutCube) { OutPos = pos; IsOutsideCube = isOutCube; }
}

public enum Pos
{
	//       X      | Y      | Z     ,
	XmYmZm = 0 << 2 | 0 << 1 | 0 << 0,
	XpYmZm = 1 << 2 | 0 << 1 | 0 << 0,
	XmYmZp = 0 << 2 | 0 << 1 | 1 << 0,
	XpYmZp = 1 << 2 | 0 << 1 | 1 << 0,
	XmYpZm = 0 << 2 | 1 << 1 | 0 << 0,
	XpYpZm = 1 << 2 | 1 << 1 | 0 << 0,
	XmYpZp = 0 << 2 | 1 << 1 | 1 << 0,
	XpYpZp = 1 << 2 | 1 << 1 | 1 << 0,
	Root = 8,
}

public enum Dir
{
	Xp = 0,
	Xm,
	Yp,
	Ym,
	Zp,
	Zm,
}