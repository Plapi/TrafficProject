using System;
using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour {

	private NavigationPoint[] points = new NavigationPoint[0];
	private int[][] adjacents;

	private List<PointOfInterest> pointOfInterests;
	private List<LinkedNode> linkedNodes;
	private readonly Dictionary<string, List<NavigationPoint>> conflictPoints = new();

	private readonly Dictionary<string, List<NavigationPoint>> paths = new();
	private readonly List<NavigationAgent> agents = new();

	private int totalAgents;

	public void SetPoints(List<NodeController> nodeControllers, out List<(string, string)> missingPaths, List<LinkedNode> linkedNodes = null) {
		this.linkedNodes = linkedNodes;

		List<NavigationPoint> points = new();
		nodeControllers.ForEach(nodeController => {
			nodeController.GetAllNodes().ForEach(n => {
				points.AddRange(n.GetNavigationRightPoints());
				points.AddRange(n.GetNavigationLeftPoints());
			});
		});

		points.ForEach(p => p.ClearNextNodes());

		Dictionary<string, string> alreadyProcessedConnexions = new();

		nodeControllers.ForEach(nodeController => {
			nodeController.IterateAllConnexions((Node n0, Node n1) => {

				string n0n1Name = n0.name + n1.name;
				string n1n0Name = n1.name + n0.name;

				bool allow = nodeControllers.Count > 1 || nodeController.GetAllNodes().Contains(n0) &&
					nodeController.GetAllNodes().Contains(n1);

				if (allow && !alreadyProcessedConnexions.ContainsKey(n0n1Name) && !alreadyProcessedConnexions.ContainsKey(n1n0Name)) {

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

		missingPaths = new();
		SetPoints(points, pofs, missingPaths);
	}

	private void SetPoints(List<NavigationPoint> points, List<PointOfInterest> pointOfInterests, List<(string, string)> missingPaths) {
		this.points = points.ToArray();
		this.pointOfInterests = pointOfInterests;

		adjacents = BFS<NavigationPoint>.GetAdjacents(this.points);

		SetConflictPoints();
		SetPaths(missingPaths);
		if (linkedNodes != null) {
			SetPathsWithLinkedNodes(missingPaths);
		}
	}

	public void Stop() {
		points = new NavigationPoint[0];
		adjacents = null;
		conflictPoints.Clear();
		paths.Clear();
		for (int i = 0; i < agents.Count; i++) {
			agents[i].Destroy();
		}
		agents.Clear();
		totalAgents = 0;
		StopAllCoroutines();
	}

	private void LateUpdate() {
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

	public bool HasPath(PointOfInterest p0, PointOfInterest p1) {
		return paths.ContainsKey(p0.name + "_" + p1.name);
	}

	public void TravelAgent(string startName, string endName, Action onComplete = null) {
		string key = startName + "_" + endName;
		if (!paths.ContainsKey(key)) {
			Debug.LogError($"Key not found {key}");
			return;
		}

		NavigationAgent agent = Instantiate(Resources.Load<NavigationAgent>("Cars/Car0"));
		agent.name = $"agent{totalAgents}";
		agent.transform.parent = transform;
		agent.Go(paths[key], () => {
			agents.Remove(agent);
			agent.Destroy();
			onComplete?.Invoke();
		});
		agents.Add(agent);
		totalAgents++;
	}

	private void SetPaths(List<(string, string)> missingPaths) {
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
						missingPaths.Add((start.name, end.name));
						//missingPaths.Add()
						//Debug.LogError($"Path not found {start.name} {from.Index}, {to.Index} {end.name}");
						//Debug.DrawLine(from.Position, to.Position, Color.red);
						//Debug.Break();
					}
				}
			}
		}
	}

	private void SetPathsWithLinkedNodes(List<(string, string)> missingPaths) {
		for (int i = 0; i < pointOfInterests.Count; i++) {
			for (int j = 0; j < linkedNodes.Count; j++) {
				if (linkedNodes[j].IsHeadNode) {
					if (FindPathWithLinkedNode(linkedNodes[j], pointOfInterests[i], out List<NavigationPoint> path)) {
						paths.Add(linkedNodes[j].name + "_" + pointOfInterests[i].name, path);
					} else {
						missingPaths.Add((linkedNodes[j].name, pointOfInterests[i].name));
					}
				} else {
					if (FindPathWithLinkedNode(pointOfInterests[i], linkedNodes[j], out List<NavigationPoint> path)) {
						paths.Add(pointOfInterests[i].name + "_" + linkedNodes[j].name, path);
					} else {
						missingPaths.Add((pointOfInterests[i].name, linkedNodes[j].name));
					}
				}
			}
		}
	}

	private bool FindPathWithLinkedNode(LinkedNode fromLinkedNode, PointOfInterest toPointOfInterest, out List<NavigationPoint> path) {
		path = null;
		List<NavigationPoint> navigationPoints = new(fromLinkedNode.GetNavigationRightPoints());
		navigationPoints.AddRange(fromLinkedNode.GetNavigationLeftPoints());
		NavigationPoint to = toPointOfInterest.EndNavigationPoint;
		for (int i = 0; i < navigationPoints.Count; i++) {
			NavigationPoint from = navigationPoints[i];
			if (BFS<NavigationPoint>.FindPath(points, adjacents, from, to, out List<NavigationPoint> path1)) {
				if (path == null || path.Count > path1.Count) {
					path = path1;
				}
			}
		}
		return path != null;
	}

	private bool FindPathWithLinkedNode(PointOfInterest fromPointOfInterest, LinkedNode toLinkedNode, out List<NavigationPoint> path) {
		path = null;
		List<NavigationPoint> navigationPoints = new(toLinkedNode.GetNavigationRightPoints());
		navigationPoints.AddRange(toLinkedNode.GetNavigationLeftPoints());
		NavigationPoint from = fromPointOfInterest.StartNavigationPoint;
		for (int i = 0; i < navigationPoints.Count; i++) {
			NavigationPoint to = navigationPoints[i];
			if (BFS<NavigationPoint>.FindPath(points, adjacents, from, to, out List<NavigationPoint> path1)) {
				if (path == null || path.Count > path1.Count) {
					path = path1;
				}
			}
		}
		return path != null;
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
