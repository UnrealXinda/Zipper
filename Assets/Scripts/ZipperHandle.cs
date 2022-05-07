using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ZipperHandle : MonoBehaviour
{
	[SerializeField] ZipperController _zipperController;

	void OnMouseDown()
	{
		_zipperController.OnHandleDown();
	}

	void OnMouseUp()
	{
		_zipperController.OnHandleUp();
	}

	void OnMouseDrag()
	{
		var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		_zipperController.OnHandleDragged(worldPos);
	}
}
