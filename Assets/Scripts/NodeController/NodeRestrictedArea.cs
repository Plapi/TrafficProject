using UnityEngine;

public class NodeRestrictedArea : MonoBehaviour {

	[SerializeField] private Transform[] points = default;

	public bool IntersectConnexion(Node node0, Node node1) {
		Utils.PerpendicularPoints(node0.transform.position, node1.transform.position, out Vector3 p0, out Vector3 p1, Config.Instance.RoadHalfWidth);
		Utils.PerpendicularPoints(node1.transform.position, node0.transform.position, out Vector3 p2, out Vector3 p3, Config.Instance.RoadHalfWidth);
		for (int i = 0; i < points.Length - 1; i++) {
			if (Utils.LineIntersectOtherLine(points[i].position, points[i + 1].position, p0, p3)) {
				return true;
			}
			if (Utils.LineIntersectOtherLine(points[i].position, points[i + 1].position, p1, p2)) {
				return true;
			}
		}
		if (Utils.LineIntersectOtherLine(points[^1].position, points[0].position, p0, p3)) {
			return true;
		}
		if (Utils.LineIntersectOtherLine(points[^1].position, points[0].position, p1, p2)) {
			return true;
		}
		return false;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		if (points == null || points.Length == 0) {
			return;
		}

		if (!Application.isPlaying) {
			for (int i = 0; i < points.Length; i++) {
				if (points[i] == null) {
					points[i] = new GameObject($"points{i}").transform;
					points[i].parent = transform;
					points[i].localPosition = Vector3.zero;
				}
			}
		}

		Gizmos.color = Color.red;
		for (int i = 0; i < points.Length - 1; i++) {
			Gizmos.DrawLine(points[i].position, points[i + 1].position);
		}
		Gizmos.DrawLine(points[^1].position, points[0].position);
	}
#endif
}
