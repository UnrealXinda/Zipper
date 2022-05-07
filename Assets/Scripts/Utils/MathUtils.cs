using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
	public static Vector3 MirrorX(Vector3 vec)
	{
		return new Vector3(-vec.x, vec.y, vec.z);
	}

	public static Vector3 VectorClamp(Vector3 input, Vector3 min, Vector3 max)
	{
		var x = Mathf.Clamp(input.x, Mathf.Min(min.x, max.x), Mathf.Max(min.x, max.x));
		var y = Mathf.Clamp(input.y, Mathf.Min(min.y, max.y), Mathf.Max(min.y, max.y));
		var z = Mathf.Clamp(input.z, Mathf.Min(min.z, max.z), Mathf.Max(min.z, max.z));
		return new Vector3(x, y, z);
	}
}


public static class Bezier
{
	public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return oneMinusT * oneMinusT * p0 +
				2f * oneMinusT * t * p1 +
				t * t * p2;
	}

	public static Vector3 GetPointMirrorX(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		return GetPoint(MathUtils.MirrorX(p0), MathUtils.MirrorX(p1), MathUtils.MirrorX(p2), t);
	}

	public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		return 2f * (1f - t) * (p1 - p0) +
				2f * t * (p2 - p1);
	}

	public static Vector3 GetFirstDerivativeMirrorX(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		return GetFirstDerivative(MathUtils.MirrorX(p0), MathUtils.MirrorX(p1), MathUtils.MirrorX(p2), t);
	}
}