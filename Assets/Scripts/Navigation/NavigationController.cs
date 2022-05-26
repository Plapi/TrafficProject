using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour {

	private List<NavigationPoint> points = new();

	public void Init(List<NavigationPoint> points) {
		this.points = points;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {

		for (int i = 0; i < points.Count; i++) {

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
