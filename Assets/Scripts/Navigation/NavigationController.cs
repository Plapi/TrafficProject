using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour {

	private NavigationPoint[] points;
	private int[][] adjacents;

	public void SetPoints(List<NavigationPoint> points) {
		this.points = points.ToArray();
		adjacents = BFS<NavigationPoint>.GetAdjacents(this.points);
		TestPath();
	}

	private void TestPath() {
		if (BFS<NavigationPoint>.FindPath(points, adjacents, points[0], points[^1], out List<NavigationPoint> path)) {
			for (int i = 0; i < path.Count - 1; i++) {
				Debug.DrawLine(path[i].Position, path[i + 1].Position, Color.green, float.MaxValue);
			}
		} else {
			Debug.LogError("Path not found");
		}
	}

#if UNITY_EDITOR
	[SerializeField] private bool drawGizmos = default;
	private void OnDrawGizmos() {
		if (!drawGizmos) {
			return;
		}

		for (int i = 0; i < points.Length; i++) {

			NavigationPoint from = points[i];
			BFSNode[] nextNodes = points[i].GetNextNodes();
			Gizmos.color = Color.green;

			for (int j = 0; j < nextNodes.Length; j++) {
				NavigationPoint to = (NavigationPoint)nextNodes[j];
				if (from.TryGetCurvePoints(to, out Vector3[] cPoints)) {
					Gizmos.DrawLine(from.Position + Vector3.up * 0.01f, cPoints[0] + Vector3.up * 0.01f);
					for (int k = 0; k < cPoints.Length - 1; k++) {
						Gizmos.DrawLine(cPoints[k] + Vector3.up * 0.01f, cPoints[k + 1] + Vector3.up * 0.01f);
					}
					DrawArrow.Draw(cPoints[^1] + Vector3.up * 0.01f, to.Position + Vector3.up * 0.01f, Color.green);
				} else {
					DrawArrow.Draw(from.Position + Vector3.up * 0.01f, to.Position + Vector3.up * 0.01f, Color.green);
				}
			}
		}
	}
#endif
}
