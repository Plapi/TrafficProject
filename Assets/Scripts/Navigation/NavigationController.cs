using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour {

	private NavigationPoint[] points;
	private int[][] adjacents;

	private List<PointOfInterest> pointOfInterests;
	private readonly Dictionary<string, List<NavigationPoint>> conflictPoints = new();

	public readonly List<NavigationAgent> agents = new();

	public void SetPoints(List<NavigationPoint> points, List<PointOfInterest> pointOfInterests) {
		this.points = points.ToArray();
		this.pointOfInterests = pointOfInterests;
		adjacents = BFS<NavigationPoint>.GetAdjacents(this.points);
		SetConflictPoints();
		StartCoroutine(SpawnAgentsCorutine());
	}

	private IEnumerator SpawnAgentsCorutine() {
		while (true) {
			yield return new WaitForSeconds(Utils.Random(1, 3));
			SpawnAgents();
		}
	}

	private void SpawnAgents() {
		for (int i = 0; i < pointOfInterests.Count; i++) {
			PointOfInterest start = pointOfInterests[i];
			PointOfInterest end = GetRandomPointofInterest(start);
			TravelAgent(start.StartNavigationPoint, end.EndNavigationPoint);
		}
	}

	private void LateUpdate() {

		if (Input.GetKeyDown(KeyCode.A)) {
			SpawnAgents();
		}

		for (int i = 0; i < agents.Count; i++) {
			if (AgentCollidesWithOthers(agents[i], agents[i].NextNavPoint.GetAgents(), out NavigationAgent otherAgent0)
				&& otherAgent0.BlockedByOtherAgent != agents[i]) {

				agents[i].BlockedByOtherAgent = otherAgent0;
				Debug.DrawLine(agents[i].transform.position, otherAgent0.transform.position, Color.yellow);

				continue;
			} else {
				agents[i].BlockedByOtherAgent = null;
			}

			string key = agents[i].CurrentNavPoint.Index + "_" + agents[i].NextNavPoint.Index;
			if (conflictPoints.TryGetValue(key, out List<NavigationPoint> cPoints)) {
				for (int j = 0; j < cPoints.Count; j++) {
					if (AgentCollidesWithOthers(agents[i], cPoints[j].GetAgents(), out NavigationAgent otherAgent1)
						&& otherAgent1.BlockedByOtherAgent != agents[i]) {

						agents[i].BlockedByOtherAgent = otherAgent1;
						Debug.DrawLine(agents[i].transform.position, otherAgent1.transform.position, Color.blue);

						break;
					} else {
						agents[i].BlockedByOtherAgent = null;
					}
				}
			}
		}

		CheckAnyBlockableLoop();
	}

	private void CheckAnyBlockableLoop() {
		for (int i = 0; i < agents.Count; i++) {
			for (int j = i + 1; j < agents.Count; j++) {
				if (agents[i].BlockedByOtherAgent == agents[j] && IsBlockableLoop(agents[i], agents[j])) {
					agents[i].BlockedByOtherAgent = null;
					return;
				}
			}
		}
	}

	private bool IsBlockableLoop(NavigationAgent blockableAgent, NavigationAgent agent1, int level = 1) {
		if (level >= 5) {
			return false;
		}
		if (agent1.BlockedByOtherAgent != null) {
			if (agent1.BlockedByOtherAgent == blockableAgent) {
				return true;
			}
			level++;
			return IsBlockableLoop(blockableAgent, agent1.BlockedByOtherAgent, level);
		}
		return false;
	}

	private bool AgentCollidesWithOthers(NavigationAgent agent, List<NavigationAgent> otherAgents, out NavigationAgent otherAgent) {
		otherAgent = null;
		for (int i = 0; i < otherAgents.Count; i++) {
			if (otherAgents[i] != agent && AgentCollidesWithOther(agent, otherAgents[i])) {
				otherAgent = otherAgents[i];
				return true;
			}
		}
		return false;
	}

	private bool AgentCollidesWithOther(NavigationAgent agent0, NavigationAgent agent1) {
		if (agent1.ContainsPoint(agent0.FrontForwardPoint)) {
			return true;
		}
		if (agent1.ContainsPoint(agent0.FrontForwardLeftPoint)) {
			return true;
		}
		if (agent1.ContainsPoint(agent0.FrontForwardRightPoint)) {
			return true;
		}
		return false;
	}

	private void TravelAgent(NavigationPoint from, NavigationPoint to) {
		if (BFS<NavigationPoint>.FindPath(points, adjacents, from, to, out List<NavigationPoint> path)) {
			NavigationAgent agent = Instantiate(Resources.Load<NavigationAgent>("Cars/Car0"));
			agent.name = $"agent{agents.Count}";
			agent.transform.parent = transform;
			agent.Go(path, () => {
				this.Delay(1f, () => {
					agents.Remove(agent);
					agent.Destroy();
				});
			});
			agents.Add(agent);
		} else {
			Debug.LogError("Path not found");
		}
	}

	private PointOfInterest GetRandomPointofInterest(PointOfInterest exlude) {
		List<PointOfInterest> list = new(pointOfInterests);
		list.Remove(exlude);
		return list.Random();
	}

	private void SetConflictPoints() {
		for (int i = 0; i < points.Length; i++) {

			if (points[i].GetNextNodes().Length < 2) {
				continue;
			}

			Vector3 a0 = points[i].Position;
			NavigationPoint[] nextPoints0 = (NavigationPoint[])points[i].GetNextNodes();

			for (int j = 0; j < nextPoints0.Length; j++) {
				Vector3 a1 = nextPoints0[j].Position;
				string key0 = points[i].Index + "_" + nextPoints0[j].Index;

				for (int k = i + 1; k < points.Length; k++) {

					if (points[k].GetNextNodes().Length < 2) {
						continue;
					}

					Vector3 b0 = points[k].Position;
					NavigationPoint[] nextPoints1 = (NavigationPoint[])points[k].GetNextNodes();

					for (int l = 0; l < nextPoints1.Length; l++) {
						Vector3 b1 = nextPoints1[l].Position;
						if (Utils.LineIntersectOtherLine(a0, a1, b0, b1)) {

							if (!conflictPoints.ContainsKey(key0)) {
								conflictPoints.Add(key0, new());
							}
							if (!conflictPoints[key0].Contains(nextPoints1[l])) {
								conflictPoints[key0].Add(nextPoints1[l]);
							}

							string key1 = points[k].Index + "_" + nextPoints1[l].Index;
							if (!conflictPoints.ContainsKey(key1)) {
								conflictPoints.Add(key1, new());
							}

							if (!conflictPoints[key1].Contains(nextPoints0[j])) {
								conflictPoints[key1].Add(nextPoints0[j]);
							}
						}
					}
				}
			}
		}
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
