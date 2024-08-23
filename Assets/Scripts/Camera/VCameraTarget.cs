using System;
using UnityEditor;
using UnityEngine;

public class VCameraTarget : MonoBehaviour
{
	public LimitDistance limitDistance;

	[Serializable]
	public struct LimitDistance
	{
		[Min(0.0f)]
		public float min;
		[Min(0.0f)]
		public float max;
	}

#if UNITY_EDITOR
	// UNITY GIZMOS
	private void OnDrawGizmosSelected()
	{
		Handles.color = Color.grey;
		Handles.Label(transform.position, "VC Target");
		Handles.DrawWireCube(transform.position, Vector3.one * 0.1f);

		if (!Application.isPlaying)
		{
			Handles.DrawLine(transform.position, transform.position + (Vector3.up * limitDistance.min), 1.0f);

			Handles.color = Color.yellow;
			Handles.DrawLine(transform.position + (Vector3.up * limitDistance.min), transform.position + (Vector3.up * limitDistance.max), 1.0f);

			Handles.DrawWireDisc(transform.position + (Vector3.up * limitDistance.min), Vector3.up, 0.1f, 1.0f);
			Handles.Label(transform.position + (Vector3.up * limitDistance.min), "VC Min dist: " + limitDistance.min.ToString());
			Handles.DrawWireDisc(transform.position + (Vector3.up * limitDistance.max), Vector3.up, 0.2f, 1.0f);
			Handles.Label(transform.position + (Vector3.up * limitDistance.max), "VC Max dist: " + limitDistance.max.ToString());
		}
	}
#endif
}