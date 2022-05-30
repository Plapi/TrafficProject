#if UNITY_EDITOR
using UnityEngine;

public class Test : MonoBehaviour {

	[SerializeField] private Transform t0 = default;
	[SerializeField] private Transform t1 = default;
	[SerializeField] private Transform t2 = default;

	[SerializeField] private Transform t = default;

	public float angle;

	private void OnDrawGizmos() {
		Gizmos.color = Utils.PointInTriangle(t.position, t0.position, t1.position, t2.position) ? Color.green : Color.red;
		Gizmos.DrawLine(t0.position, t1.position);
		Gizmos.DrawLine(t1.position, t2.position);
		Gizmos.DrawLine(t2.position, t0.position);
		//angle = Utils.GetAngle360(t0.position, t1.position, t2.position);
		//UnityEditor.Handles.Label(t1.position, angle.ToString());
	}
}
#endif