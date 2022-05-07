using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ZipperController : MonoBehaviour
{
	[SerializeField]
	protected AudioSource _sound;

	public abstract void OnHandleDown();
	public abstract void OnHandleUp();
	public abstract void OnHandleDragged(Vector3 newPosition);
}
