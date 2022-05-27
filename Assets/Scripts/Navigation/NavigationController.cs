using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour {

	private NavigationPoint[] points;
	private int[][] adjacents;

	private List<PointOfInterest> pointOfInterests;

	public void SetPoints(List<NavigationPoint> points, List<PointOfInterest> pointOfInterests) {
		this.points = points.ToArray();
		adjacents = BFS<NavigationPoint>.GetAdjacents(this.points);

		this.pointOfInterests = pointOfInterests;

		StartCoroutine(SpawnAgent());
	}

	private IEnumerator SpawnAgent() {
		while (true) {
			yield return new WaitForSeconds(Utils.Random(1, 3));
			PointOfInterest start = pointOfInterests.Random();
			PointOfInterest end = GetRandomPointofInterest(start);
			TravelAgent(start.StartNavigationPoint, end.EndNavigationPoint);
		}
	}

	private void TravelAgent(NavigationPoint from, NavigationPoint to) {
		if (BFS<NavigationPoint>.FindPath(points, adjacents, from, to, out List<NavigationPoint> path)) {
			NavigationAgent agent = Instantiate(Resources.Load<NavigationAgent>("Cars/Car0"));
			agent.transform.parent = transform;
			agent.Go(path, () => {
				this.Delay(1f, () => {
					Destroy(agent.gameObject);
				});
			});
		} else {
			Debug.LogError("Path not found");
		}
	}

	private PointOfInterest GetRandomPointofInterest(PointOfInterest exlude) {
		List<PointOfInterest> list = new(pointOfInterests);
		list.Remove(exlude);
		return list.Random();
	}

#if UNITY_EDITOR
	[SerializeField] private bool drawGizmos = default;
	private void OnDrawGizmos() {
		if (!drawGizmos || !Application.isPlaying) {
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
