using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour {

	private NavigationPoint[] points;
	private int[][] adjacents;

	public void SetPoints(List<NavigationPoint> points) {
		this.points = points.ToArray();
		adjacents = BFS<NavigationPoint>.GetAdjacents(this.points);

		StartTraveAgent(points[0], points[73]);
		StartTraveAgent(points[72], points[1]);
		StartTraveAgent(points[14], points[33]);
		StartTraveAgent(points[32], points[15]);
		StartTraveAgent(points[50], points[36]);
		StartTraveAgent(points[30], points[46]);
	}

	private void StartTraveAgent(NavigationPoint from, NavigationPoint to) {
		if (BFS<NavigationPoint>.FindPath(points, adjacents, from, to, out List<NavigationPoint> path)) {
			NavigationAgent agent = Instantiate(Resources.Load<NavigationAgent>("Cars/Car0"));
			agent.transform.parent = transform;
			TravelAgentOnPath(agent, path);
		} else {
			Debug.LogError("Path not found");
		}
	}

	private void TravelAgentOnPath(NavigationAgent agent, List<NavigationPoint> path) {
		this.Delay(1f, () => {
			agent.Go(path, () => {
				TravelAgentOnPath(agent, path);
			});
		});
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

			UnityEditor.Handles.Label(points[i].Position + Vector3.up * 0.01f, "P" + i);
		}
	}
#endif
}
