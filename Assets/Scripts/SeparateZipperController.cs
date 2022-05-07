using System;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SeparateZipperController : ZipperController
{
	[Header("References")]
	[SerializeField] ZipperHandle _handle;
	[SerializeField] SpriteRenderer[] _leftToothRenderers = new SpriteRenderer[0];
	[SerializeField] SpriteRenderer[] _rightToothRenderers = new SpriteRenderer[0];
	[SerializeField] GameObject _leftTeethContainter;
	[SerializeField] GameObject _rightTeethContainter;
	[SerializeField] Sprite _toothSprite;
	[SerializeField] int _toothSpriteDrawingOrder;

	Vector2 HandleMinPos => transform.TransformPoint(_handleMinPos);
	Vector2 HandleMaxPos => transform.TransformPoint(_handleMaxPos);

	[Header("Control")]
	[Range(20, 60)] public int count = 20;
	[Range(0, 1)] public float control = 0f;
	public Vector3 p0, p1, p2;
	public AnimationCurve curve1x;
	public AnimationCurve curve1y;
	public AnimationCurve curve2x;
	public AnimationCurve curve2y;
	public AnimationCurve controlInterpCurve;
	public bool debug = true;
	public bool drawGizmos;
	[SerializeField] Vector2 _handleMinPos;
	[SerializeField] Vector2 _handleMaxPos;
	[SerializeField] Vector3 _toothSpriteScale = Vector3.one;
	[SerializeField] Vector3 _tapeOffset = Vector3.zero;
	[SerializeField] [Range(0, 0.1f)] float _longitudinalOffset = 0f;

	float InterpControl => controlInterpCurve.Evaluate(control);

	[SerializeField] SpriteShapeController _leftTape;
	[SerializeField] SpriteShapeController _rightTape;

	// Start is called before the first frame update
	void Start()
	{
		InitToothRenderers(ref _leftToothRenderers, count, transform, "LeftTooth");
		InitToothRenderers(ref _rightToothRenderers, count, transform, "RightTooth");
	}

	static void InitToothRenderers(ref SpriteRenderer[] renderers, int count, Transform parent, string rendererPrefix)
	{
		if (renderers.Length != count)
		{
			foreach (var renderer in renderers.Where(r => r != null))
			{
				DestroyImmediate(renderer.gameObject);
			}

			renderers = Enumerable.Range(0, count).Select(i =>
			{
				var go = new GameObject($"{rendererPrefix}{i.ToString("D2")}");
				var renderer = go.AddComponent<SpriteRenderer>();
				go.transform.parent = parent;
				return renderer;
			}).ToArray();
		}
	}

	// Update is called once per frame
	void Update()
	{
#if UNITY_EDITOR
		InitToothRenderers(ref _leftToothRenderers, count, _leftTeethContainter.transform, "LeftTooth");
		InitToothRenderers(ref _rightToothRenderers, count, _rightTeethContainter.transform, "RightTooth");
#endif
		UpdateHandle();
		UpdateTeeth();
		UpdateTapes();
	}

	void UpdateHandle()
	{
		_handle.transform.position = Vector2.Lerp(HandleMinPos, HandleMaxPos, control);
	}

	void UpdateTeeth()
	{
		for (var i = 0; i < count; ++i)
		{
			var t = (float)i / count;

			var point0 = p0;
			var point1 = p1;
			var point2 = p2;

			if (!debug)
			{
				point1.x = curve1x.Evaluate(InterpControl);
				point1.y = curve1y.Evaluate(InterpControl);
				point2.x = curve2x.Evaluate(InterpControl);
				point2.y = curve2y.Evaluate(InterpControl);
			}

			var leftPoint = Bezier.GetPoint(point0, point1, point2, t - _longitudinalOffset);
			var leftTangent = Bezier.GetFirstDerivative(point0, point1, point2, t - _longitudinalOffset);
			var leftNormal = Vector3.Cross(leftTangent, new Vector3(0, 0, 1)).normalized;
			var leftTooth = _leftToothRenderers[i];

			var rightPoint = Bezier.GetPointMirrorX(point0, point1, point2, t + _longitudinalOffset);
			var rightTangent = Bezier.GetFirstDerivativeMirrorX(point0, point1, point2, t + _longitudinalOffset);
			var rightNormal = Vector3.Cross(rightTangent, new Vector3(0, 0, -1)).normalized;
			var rightTooth = _rightToothRenderers[i];

			leftTooth.sprite = _toothSprite;
			leftTooth.sortingOrder = _toothSpriteDrawingOrder;
			leftTooth.transform.localScale = _toothSpriteScale;
			leftTooth.transform.localPosition = leftPoint - _tapeOffset;
			leftTooth.transform.localRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), leftNormal);

			rightTooth.sprite = _toothSprite;
			rightTooth.sortingOrder = _toothSpriteDrawingOrder;
			rightTooth.transform.localScale = _toothSpriteScale;
			rightTooth.transform.localPosition = rightPoint + _tapeOffset;
			rightTooth.transform.localRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), rightNormal);
		}
	}

	void UpdateTapes()
	{
		for (var i = _leftTape.spline.GetPointCount(); i < count; ++i)
		{
			_leftTape.spline.InsertPointAt(i, new Vector3(i, i, i));
		}

		for (var i = _rightTape.spline.GetPointCount(); i < count; ++i)
		{
			_rightTape.spline.InsertPointAt(i, new Vector3(i, i, i));
		}

		for (var i = 0; i < count; ++i)
		{
			var t = (float)i / count;
			var point0 = p0;
			var point1 = p1;
			var point2 = p2;

			if (!debug)
			{
				point1.x = curve1x.Evaluate(InterpControl);
				point1.y = curve1y.Evaluate(InterpControl);
				point2.x = curve2x.Evaluate(InterpControl);
				point2.y = curve2y.Evaluate(InterpControl);
			}

			var leftPos = Bezier.GetPoint(point0, point1, point2, t);
			var rightPos = Bezier.GetPointMirrorX(point0, point1, point2, t);
			_leftTape.spline.SetPosition(i, leftPos);
			_leftTape.spline.SetTangentMode(i, ShapeTangentMode.Linear);
			_rightTape.spline.SetPosition(i, rightPos);
			_rightTape.spline.SetTangentMode(i, ShapeTangentMode.Linear);
		}

		_leftTape.BakeCollider();
		_rightTape.BakeCollider();
	}

	public override void OnHandleDown()
	{
		_sound.Stop();
	}

	public override void OnHandleUp()
	{
		_sound.Stop();
	}

	public override void OnHandleDragged(Vector3 worldPos)
	{
		var localPos = transform.InverseTransformPoint(worldPos);
		var handlePos = Vector3.Project(localPos - (Vector3)_handleMinPos, (Vector3)_handleMaxPos - (Vector3)_handleMinPos) + (Vector3)_handleMinPos;
		handlePos = MathUtils.VectorClamp(handlePos, _handleMinPos, _handleMaxPos);
		control = ((Vector2)handlePos - _handleMinPos).magnitude / (_handleMaxPos - _handleMinPos).magnitude;

		if (!_sound.isPlaying)
		{
			_sound.Play();
		}
	}

#if UNITY_EDITOR

	void OnDrawGizmos()
	{
		if (!drawGizmos)
		{
			return;
		}

		var thickness = 10;
		var leftTp0 = transform.TransformPoint(p0);
		var leftTp1 = transform.TransformPoint(p1);
		var leftTp2 = transform.TransformPoint(p2);
		Gizmos.DrawSphere(leftTp0, 0.3f);
		Gizmos.DrawSphere(leftTp1, 0.3f);
		Gizmos.DrawSphere(leftTp2, 0.3f);
		Handles.DrawBezier(leftTp0, leftTp2, leftTp0, leftTp1, Color.red, null, thickness);

		var rightTp0 = transform.TransformPoint(MathUtils.MirrorX(p0));
		var rightTp1 = transform.TransformPoint(MathUtils.MirrorX(p1));
		var rightTp2 = transform.TransformPoint(MathUtils.MirrorX(p2));
		Handles.DrawBezier(rightTp0, rightTp2, rightTp0, rightTp1, Color.red, null, thickness);

		for (var i = 0; i < count; ++i)
		{
			var t = (float)i / count;
			var leftPoint = Bezier.GetPoint(leftTp0, leftTp1, leftTp2, t + _longitudinalOffset);
			var leftTangent = Bezier.GetFirstDerivative(leftTp0, leftTp1, leftTp2, t + _longitudinalOffset);
			var leftNormal = Vector3.Cross(leftTangent, new Vector3(0, 0, 1)).normalized;

			var rightPoint = Bezier.GetPoint(rightTp0, rightTp1, rightTp2, t + _longitudinalOffset);
			var rightTangent = Bezier.GetFirstDerivative(rightTp0, rightTp1, rightTp2, t + _longitudinalOffset);
			var rightNormal = Vector3.Cross(rightTangent, new Vector3(0, 0, -1)).normalized;

			Gizmos.DrawLine(leftPoint, leftPoint + leftNormal);
			Gizmos.DrawLine(rightPoint, rightPoint + rightNormal);
		}

		var start = HandleMinPos;
		var end = HandleMaxPos;
		thickness = 30;
		Handles.DrawBezier(start, end, start, end, Color.green, null, thickness);
	}

	public void Record()
	{
		AddKey(curve1x, control, p1.x);
		AddKey(curve1y, control, p1.y);
		AddKey(curve2x, control, p2.x);
		AddKey(curve2y, control, p2.y);
	}

	public void Load()
	{
		p1.x = curve1x.Evaluate(control);
		p1.y = curve1y.Evaluate(control);
		p2.x = curve2x.Evaluate(control);
		p2.y = curve2y.Evaluate(control);
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

[CustomEditor(typeof(SeparateZipperController))]
public class SeparateZipperControllerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Record"))
		{
			var controller = target as SeparateZipperController;
			controller.Record();
		}

		if (GUILayout.Button("Load"))
		{
			var controller = target as SeparateZipperController;
			controller.Load();
		}
	}
}

#endif