using System;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct SplineCurves
{
	public int index;
	public AnimationCurve positionCurveX;
	public AnimationCurve positionCurveY;
	public AnimationCurve leftTangentCurveX;
	public AnimationCurve leftTangentCurveY;
	public AnimationCurve rightTangentCurveX;
	public AnimationCurve rightTangentCurveY;

	public Vector3 EvaluatePosition(float t)
	{
		return new Vector3(positionCurveX.Evaluate(t), positionCurveY.Evaluate(t), 0);
	}

	public Vector3 EvaluateLeftTangent(float t)
	{
		return new Vector3(leftTangentCurveX.Evaluate(t), leftTangentCurveY.Evaluate(t), 0);
	}

	public Vector3 EvaluateRightTangent(float t)
	{
		return new Vector3(rightTangentCurveX.Evaluate(t), rightTangentCurveY.Evaluate(t), 0);
	}
}

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteShapeController))]
public class ZipperSplineController : MonoBehaviour
{
#if UNITY_EDITOR
	public bool debug;
	public bool manual;
#endif

	[Range(0, 1)]
	public float control;

	[SerializeField] SplineCurves[] _splineCurves;
	[SerializeField] SpriteShapeController _controller;

	// Start is called before the first frame update
	void Start()
	{
		if (!_controller)
		{
			_controller = GetComponent<SpriteShapeController>();
		}
	}

	// Update is called once per frame
	void Update()
	{
#if UNITY_EDITOR
		if (debug && manual)
		{
			return;
		}
#endif

		foreach (var curve in _splineCurves)
		{
			var pos = curve.EvaluatePosition(control);
			var leftTan = curve.EvaluateLeftTangent(control);
			var rightTan = curve.EvaluateRightTangent(control);

			_controller.spline.SetPosition(curve.index, pos);
			_controller.spline.SetLeftTangent(curve.index, leftTan);
			_controller.spline.SetRightTangent(curve.index, rightTan);
		}
		_controller.BakeCollider();
	}

#if UNITY_EDITOR

	public void Record()
	{
		foreach (var curve in _splineCurves)
		{
			var index = curve.index;
			var pos = _controller.spline.GetPosition(index);
			var leftTan = _controller.spline.GetLeftTangent(index);
			var rightTan = _controller.spline.GetRightTangent(index);

			AddKey(curve.positionCurveX, control, pos.x);
			AddKey(curve.positionCurveY, control, pos.y);
			AddKey(curve.leftTangentCurveX, control, leftTan.x);
			AddKey(curve.leftTangentCurveY, control, leftTan.y);
			AddKey(curve.rightTangentCurveX, control, rightTan.x);
			AddKey(curve.rightTangentCurveY, control, rightTan.y);
		}
	}

	static void AddKey(AnimationCurve curve, float time, float value)
	{
		var index = Array.FindIndex(curve.keys, key => key.time == time);
		if (index >= 0)
		{
			Keyframe[] copiedKeys = curve.keys;
			copiedKeys[index].value = value;
			copiedKeys[index].time = time;
			curve.keys = copiedKeys;
		}
		else
		{
			curve.AddKey(time, value);
		}
	}

#endif
}


#if UNITY_EDITOR

[CustomEditor(typeof(ZipperSplineController))]
public class ZipperSplineControllerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Record"))
		{
			var controller = target as ZipperSplineController;
			controller.Record();
		}
	}
}

#endif