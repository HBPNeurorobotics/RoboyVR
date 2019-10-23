using System;
using UnityEngine;

static class MathUtil
{
	public static bool CubeInsideSphere(Vector3 C1, Vector3 C2, Vector3 S, float R)
	{
		var x = GetFurther(S.x, C1.x, C2.x);
		var y = GetFurther(S.y, C1.y, C2.y);
		var z = GetFurther(S.z, C1.z, C2.z);
		return IsPointInsideSphere(new Vector3(x, y, z), S, R);
	}

	public static float GetFurther(float target, float a, float b)
	{
		return Math.Abs(target - a) > Math.Abs(target - b) ? a : b;
	}

	public static bool DoesCubeIntersectSphere(Vector3 C1, Vector3 C2, Vector3 S, float R)
	{
		// get box closest point to sphere center by clamping
		var x = Math.Max(C1.x, Math.Min(S.x, C2.x));
		var y = Math.Max(C1.y, Math.Min(S.y, C2.y));
		var z = Math.Max(C1.z, Math.Min(S.z, C2.z));

		return IsPointInsideSphere(new Vector3(x, y, z), S, R);
	}

	public static bool IsPointInsideSphere(Vector3 p, Vector3 S, float R)
	{
		var distanceSq =
				(p.x - S.x) * (p.x - S.x) +
				(p.y - S.y) * (p.y - S.y) +
				(p.z - S.z) * (p.z - S.z);

		return distanceSq < (R * R);
	}

	public static bool IsPointInsideCube(Vector3 p, Vector3 C1, Vector3 C2)
	{
		return p.x > C1.x && p.y > C1.y && p.z > C1.z
			&& p.x < C2.x && p.y < C2.y && p.z < C2.z;
	}
}