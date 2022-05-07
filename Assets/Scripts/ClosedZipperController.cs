using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class ClosedZipperController : ZipperController
{
	[SerializeField] [Range(0, 1)] float _control;
	[SerializeField] ZipperSplineController[] _splineControllers;
	[SerializeField] ZipperHandle _handle;

	[SerializeField] Vector2 _handleMinPos;
	[SerializeField] Vector2 _handleMaxPos;

	Vector2 HandleMinPos => transform.TransformPoint(_handleMinPos);
	Vector2 HandleMaxPos => transform.TransformPoint(_handleMaxPos);

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		foreach (var spline in _splineControllers.Where(s => s != null))
		{

#if UNITY_EDITOR
			if (!spline.debug)
			{
#endif
				spline.control = _control;

#if UNITY_EDITOR
			}
#endif
		}

		if (_handle)
		{
			_handle.transform.position = Vector2.Lerp(HandleMinPos, HandleMaxPos, _control);
		}
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
		_control = ((Vector2)handlePos - _handleMinPos).magnitude / (_handleMaxPos - _handleMinPos).magnitude;

		if (!_sound.isPlaying)
		{
			_sound.Play();
		}
	}

#if UNITY_EDITOR

	void OnDrawGizmos()
	{
		var start = HandleMinPos;
		var end = HandleMaxPos;
		var thickness = 30;
		UnityEditor.Handles.DrawBezier(start, end, start, end, Color.green, null, thickness);
	}

#endif
}

