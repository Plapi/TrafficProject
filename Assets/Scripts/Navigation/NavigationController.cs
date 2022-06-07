using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour {

	private NavigationPoint[] points = new NavigationPoint[0];
	private int[][] adjacents;

	private List<PointOfInterest> pointOfInterests;
	private readonly Dictionary<string, List<NavigationPoint>> conflictPoints = new();

	private readonly Dictionary<string, List<NavigationPoint>> paths = new();
	private readonly List<NavigationAgent> agents = new();

	private int totalAgents;

	public void SetPoints(List<NodeController> nodeControllers) {
		List<NavigationPoint> points = new();
		nodeControllers.ForEach(nodeController => {
			nodeController.GetAllNodes().ForEach(n => {
				points.AddRange(n.GetNavigationRightPoints());
				points.AddRange(n.GetNavigationLeftPoints());
			});
		});

		Dictionary<string, string> alreadyProcessedConnexions = new();

		nodeControllers.ForEach(nodeController => {
			nodeController.IterateAllConnexions((Node n0, Node n1) => {

				string n0n1Name = n0.name + n1.name;
				string n1n0Name = n1.name + n0.name;

				if (!alreadyProcessedConnexions.ContainsKey(n0n1Name) && !alreadyProcessedConnexions.ContainsKey(n1n0Name)) {

					alreadyProcessedConnexions.Add(n0n1Name, null);
					alreadyProcessedConnexions.Add(n1n0Name, null);

					nodeController.GetNavigationPointsBetween(n0, n1, out NavigationPoint[] navPoints);

					Vector3 mid0 = Utils.MidPoint(navPoints[0].Position, navPoints[2].Position);
					Vector3 mid1 = Utils.MidPoint(navPoints[1].Position, navPoints[3].Position);
					Vector3 dir = (mid1 - mid0).normalized;

					Vector3 delta = (navPoints[0].Position - mid0).normalized;
					Vector3 cross = Vector3.Cross(delta, dir);

					if (Config.Instance.RightDriving) {
						if (cross.y > 0) {
							navPoints[1].AddNextNode(navPoints[0]);
							navPoints[2].AddNextNode(navPoints[3]);
						} else {
							navPoints[0].AddNextNode(navPoints[1]);
							navPoints[3].AddNextNode(navPoints[2]);
						}
					} else {
						if (cross.y > 0) {
							navPoints[0].AddNextNode(navPoints[1]);
							navPoints[3].AddNextNode(navPoints[2]);
						} else {
							navPoints[1].AddNextNode(navPoints[0]);
							navPoints[2].AddNextNode(navPoints[3]);
						}
					}
				}
			});
		});

		nodeControllers.ForEach(nodeController => {
			nodeController.GetAllNodes().ForEach(node => {
				if (node.GetNavigationRightPoints().Length >= 2) {

					NavigationPoint[] rightPoints = node.GetNavigationRightPoints();
					NavigationPoint[] leftPoints = node.GetNavigationLeftPoints();

					Vector3[] rightDirections = node.GetNavigationDirectionRightPoints();
					Vector3[] leftDirections = node.GetNavigationDirectionLeftPoints();

					if (Config.Instance.RightDriving) {
						for (int i = 0; i < rightPoints.Length; i++) {
							for (int j = 0; j < leftPoints.Length; j++) {
								if (i != j) {
									Vector3 cPoint = Utils.Intersection(rightPoints[i].Position, rightDirections[i], leftPoints[j].Position, leftDirections[j]);
									rightPoints[i].AddNextNodeWithCurvePoints(leftPoints[j], cPoint);
								}
							}
						}
					} else {
						for (int i = 0; i < leftPoints.Length; i++) {
							for (int j = 0; j < rightPoints.Length; j++) {
								if (i != j) {
									Vector3 cPoint = Utils.Intersection(leftPoints[i].Position, leftDirections[i], rightPoints[j].Position, rightDirections[j]);
									leftPoints[i].AddNextNodeWithCurvePoints(rightPoints[j], cPoint);
								}
							}
						}
					}
				}
			});
		});

		List<PointOfInterest> pofs = new();
		nodeControllers.ForEach(nodeController => {
			pofs.AddRange(nodeController.GetPointOfInterests());
		});

		SetPoints(points, pofs);
	}

	public void SetPoints(List<NavigationPoint> points, List<PointOfInterest> pointOfInterests) {
		this.points = points.ToArray();
		this.pointOfInterests = pointOfInterests;

		adjacents = BFS<NavigationPoint>.GetAdjacents(this.points);

		SetConflictPoints();
		//SetPaths();

		//this.Delay(2f, () => StartCoroutine(SpawnAgentsCorutine()));
	}

	public void Stop() {
		points = new NavigationPoint[0];
		conflictPoints.Clear();
		paths.Clear();
		for (int i = 0; i < agents.Count; i++) {
			agents[i].Destroy();
		}
		agents.Clear();
		totalAgents = 0;
		StopAllCoroutines();
	}

	private IEnumerator SpawnAgentsCorutine() {
		while (true) {
			SpawnAgents();
			yield return new WaitForSeconds(3f);
		}
	}

	private void SpawnAgents() {
		if (agents.Count >= 50) {
			return;
		}
		for (int i = 0; i < pointOfInterests.Count; i++) {
			TravelAgent(pointOfInterests[i]);
		}
	}

	private void LateUpdate() {

		if (Input.GetKeyDown(KeyCode.B)) {
			for (int i = 0; i < points.Length; i++) {
				List<NavigationAgent> agents = points[i].GetAgents();
				if (agents.Count > 0) {
					Debug.LogError("agents count:" + agents.Count);
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.A)) {
			SpawnAgents();
		}

		for (int i = 0; i < agents.Count; i++) {
			if (AgentCollidesWithOthers(agents[i], agents[i].NextNavPoint.GetAgents(), out NavigationAgent otherAgent0)
				&& otherAgent0.BlockedByOtherAgent != agents[i]) {

				agents[i].BlockedByOtherAgent = otherAgent0;
				//Debug.DrawLine(agents[i].transform.position, otherAgent0.transform.position, Color.yellow);

				continue;
			} else {
				agents[i].BlockedByOtherAgent = null;
			}

			string key = agents[i].CurrentNavPoint.Index + "_" + agents[i].NextNavPoint.Index;
			if (conflictPoints.TryGetValue(key, out List<NavigationPoint> cPoints)) {
				for (int j = 0; j < cPoints.Count; j++) {
					if (AgentCollidesWithOthers(agents[i], cPoints[j].GetAgents(), out NavigationAgent otherAgent1)
						&& otherAgent1.BlockedByOtherAgent != agents[i] && !otherAgent1.CurrentNavPoint.GivesWay) {
						agents[i].BlockedByOtherAgent = otherAgent1;
						//Debug.DrawLine(agents[i].transform.position, otherAgent1.transform.position, Color.blue);
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

	private void TravelAgent(PointOfInterest start) {

		PointOfInterest end = GetRandomPointofInterest(start);
		List<NavigationPoint> path = paths[start.name + "_" + end.name];

		NavigationAgent agent = Instantiate(Resources.Load<NavigationAgent>("Cars/Car0"));
		agent.name = $"agent{totalAgents}";
		agent.transform.parent = transform;
		agent.Go(path, () => {
			this.Delay(1f, () => {
				agents.Remove(agent);
				agent.Destroy();
			});
		});
		agents.Add(agent);
		totalAgents++;
	}

	private PointOfInterest GetRandomPointofInterest(PointOfInterest exlude) {
		List<PointOfInterest> list = new(pointOfInterests);
		list.Remove(exlude);
		return list.Random();
	}

	private void SetPaths() {
		for (int i = 0; i < pointOfInterests.Count; i++) {
			PointOfInterest start = pointOfInterests[i];
			for (int j = 0; j < pointOfInterests.Count; j++) {
				if (i != j) {
					PointOfInterest end = pointOfInterests[j];

					NavigationPoint from = start.StartNavigationPoint;
					NavigationPoint to = end.EndNavigationPoint;

					if (BFS<NavigationPoint>.FindPath(points, adjacents, from, to, out List<NavigationPoint> path)) {
						paths.Add(start.name + "_" + end.name, path);
					} else {
						Debug.LogError($"Path not found {start.name} {from.Index}, {to.Index} {end.name}");
						Debug.DrawLine(from.Position, to.Position, Color.red);
						Debug.Break();
					}
				}
			}

		}
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

							//if (points[i].Index == 46 && nextPoints0[j].Index == 53) {
							//Debug.DrawLine(a0, a1, Color.red);
							//Debug.DrawLine(b0, b1, Color.red);
							//}
						}
					}
				}
			}
		}

		//Debug.Break();
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
