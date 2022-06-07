using System;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour {

	private const string DATA_SAVE_KEY = "NODES_DATA";

	[SerializeField] private List<PointOfInterest> pointOfInterests = default;
	[SerializeField] private List<LinkedNode> linkedNodes = default;

	[SerializeField] private List<NodeRestrictedArea> restrictedAreas = default;

	private readonly List<Node> nodes = new();
	private readonly List<Node> otherLinkedNodes = new();

	private BoxCollider map;
	private Node prevNode;
	private Node currentNode;
	private Node virtualNode;

	public bool CurrentNodeCanBePlaced { get; private set; }
	public Node CurrentNode => currentNode;

	public bool CurrentNodesHasMinDistance => currentNode != null && prevNode != null &&
		Vector3.Distance(currentNode.transform.position, prevNode.transform.position) > Config.Instance.RoadHalfWidth;

	private string dataSaveKey = DATA_SAVE_KEY;

	public void SetLinkNodes() {
		for (int i = 0; i < linkedNodes.Count; i++) {
			linkedNodes[i].LinkNode.SetNode(true, true);
			linkedNodes[i].SetNode(true, false);
		}
	}

	public void Init(BoxCollider map) {
		this.map = map;
		dataSaveKey += "_" + transform.parent.name;

		for (int i = 0; i < pointOfInterests.Count; i++) {
			pointOfInterests[i].HeadNode.SetNode(true, true);
			pointOfInterests[i].OtherNode.SetNode(true, false);
			nodes.Add(pointOfInterests[i].HeadNode);
			nodes.Add(pointOfInterests[i].OtherNode);
			CreateNodesConnexion(pointOfInterests[i].HeadNode, pointOfInterests[i].OtherNode);
		}

		for (int i = 0; i < linkedNodes.Count; i++) {
			linkedNodes[i].LinkNode.SetNode(true, false);
			linkedNodes[i].SetNode(true, false);
			nodes.Add(linkedNodes[i].LinkNode);
			nodes.Add(linkedNodes[i]);
			CreateNodesConnexion(linkedNodes[i].LinkNode, linkedNodes[i]);
			otherLinkedNodes.Add(linkedNodes[i].LinkNode);
		}

		if (PlayerPrefs.HasKey(dataSaveKey)) {
			NodeData[] nodesData = Utils.Deserialize<NodeData[]>(PlayerPrefs.GetString(dataSaveKey));
			for (int i = 0; i < nodesData.Length; i++) {
				if (!nodesData[i].isStatic) {
					NewNode(nodesData[i].position.ToVector3());
				}
			}

			for (int i = 0; i < nodesData.Length; i++) {
				for (int j = 0; j < nodesData[i].connexions.Length; j++) {
					CreateNodesConnexion(nodes[i], nodes[nodesData[i].connexions[j]]);
				}
			}

			nodes.ForEach(n => n.UpdateMesh());

			for (int i = 0; i < nodesData.Length; i++) {
				if (nodes[i].ConnexionsCount > 2) {
					NavigationPoint[] inputPoints = Config.Instance.RightDriving ?
					nodes[i].GetNavigationRightPoints() : nodes[i].GetNavigationLeftPoints();
					if (nodesData[i].intersection == null) {
						Debug.LogError("Intersection is null");
					}
					nodes[i].UpdateSemaphore(nodesData[i].intersection.semaphore);
					for (int j = 0; j < inputPoints.Length; j++) {
						nodes[i].UpdateGiveWay(j, nodesData[i].intersection.giveWayInputs[j]);
					}
				}
			}
		} else {
			nodes.ForEach(n => n.UpdateMesh());
		}
	}

	public void StartIntersectionsWithSemaphore() {
		GetIntersections().ForEach(intersection => {
			if (intersection.GetSemaphoreData().isOn) {
				intersection.StartSemaphores();
			}
		});
	}

	public void StopIntersectionsWithSemaphores() {
		nodes.ForEach(n => n.StopSemaphores());
	}

	public List<Node> GetAllNodes() {
		return nodes;
	}

	public List<Node> GetIntersections() {
		List<Node> intersections = new();
		nodes.ForEach(node => {
			if (node.ConnexionsCount > 2) {
				intersections.Add(node);
			}
		});
		return intersections;
	}

	public List<PointOfInterest> GetPointOfInterests() {
		return pointOfInterests;
	}

	public void IterateAllConnexions(Action<Node, Node> action) {
		nodes.ForEach(n0 => {
			n0.GetConnexions().ForEach(n1 => {
				action(n0, n1);
			});
		});
	}

	public void GetNavigationPointsBetween(Node node0, Node node1, out NavigationPoint[] navPoints) {
		navPoints = new NavigationPoint[4];

		List<NavigationPoint> points0 = new(node0.GetNavigationLeftPoints());
		points0.AddRange(node0.GetNavigationRightPoints());

		List<NavigationPoint> points1 = new(node1.GetNavigationLeftPoints());
		points1.AddRange(node1.GetNavigationRightPoints());

		(NavigationPoint, NavigationPoint) getGlosestPoints() {
			(NavigationPoint, NavigationPoint) group = (points0[0], points1[0]);
			float dist = Vector3.Distance(group.Item1.Position, group.Item2.Position);
			for (int i = 0; i < points0.Count; i++) {
				for (int j = 0; j < points1.Count; j++) {
					float d = Vector3.Distance(points0[i].Position, points1[j].Position);
					if (dist > d) {
						group = (points0[i], points1[j]);
						dist = d;
					}
				}
			}
			return group;
		}

		(NavigationPoint, NavigationPoint) closestPoints = getGlosestPoints();
		points0.Remove(closestPoints.Item1);
		points0.Remove(closestPoints.Item2);
		navPoints[0] = closestPoints.Item1;
		navPoints[1] = closestPoints.Item2;

		closestPoints = getGlosestPoints();
		navPoints[2] = closestPoints.Item1;
		navPoints[3] = closestPoints.Item2;
	}

	public void UpdateController() {
		if (!GetRaycastPoint(out Vector3 point)) {
			return;
		}

		if (currentNode == null) {
			if (TryGetNearNode(point, out Node nearNode)) {
				prevNode = nearNode;
			} else if (HasConnectionBetween(point, out Node node0, out Node node1)) {
				point = Utils.GetClosestPointOnLine(point, node0.transform.position, node1.transform.position);
				RemoveNodesConnexion(node0, node1);
				prevNode = NewNode(point);
				CreateNodesConnexion(node0, prevNode);
				CreateNodesConnexion(node1, prevNode);
				node0.UpdateMesh();
				node1.UpdateMesh();
			} else {
				prevNode = NewNode(point);
			}

			currentNode = NewNode(point);
			CreateNodesConnexion(prevNode, currentNode);
		}

		if (virtualNode == null) {
			bool hasIntersection = TryGetClosestIntersectionNode(out Node intersectionNode) && !prevNode.IsConnectedWith(intersectionNode);
			if (!hasIntersection) {
				if (ConnexionIntersectsOtherConnexion(prevNode, currentNode, out Vector3 intersection, out Node intNode0, out Node intNode1)) {
					hasIntersection = true;
					RemoveNodesConnexion(intNode0, intNode1);
					intersectionNode = NewNode(intersection);
					CreateNodesConnexion(intersectionNode, intNode0);
					CreateNodesConnexion(intersectionNode, intNode1);
					intNode0.UpdateMesh();
					intNode1.UpdateMesh();
				}
			}
			if (hasIntersection) {
				RemoveNodesConnexion(prevNode, currentNode);
				RemoveNode(currentNode);

				currentNode = intersectionNode;
				CreateNodesConnexion(prevNode, currentNode);

				virtualNode = NewNode(point, false);
			} else {
				currentNode.transform.position = point;
			}
		} else {
			virtualNode.transform.position = point;
			if (!IsNodeBetweenOtherNodes(currentNode, virtualNode, prevNode)) {
				RemoveNodesConnexion(prevNode, currentNode);
				currentNode.UpdateMesh();
				currentNode.UpdateHighlightColor(true);
				Destroy(virtualNode.gameObject);

				currentNode = NewNode(point);
				CreateNodesConnexion(prevNode, currentNode);
			}
		}

		CurrentNodeCanBePlaced = prevNode.HasAcceptedDistance(currentNode) &&
			prevNode.HasAcceptedAngle(currentNode) && currentNode.HasAcceptedAngle(prevNode) &&
			!IntersectAnyRestrictedArea(prevNode, currentNode);

		currentNode.UpdateHighlightColor(CurrentNodeCanBePlaced);
		prevNode.UpdateHighlightColor(CurrentNodeCanBePlaced);

		currentNode.UpdateMesh();
		prevNode.UpdateMesh();
	}

	public void ApplyCurrentNode() {
		if (virtualNode != null) {
			Destroy(virtualNode.gameObject);
			virtualNode = null;
		}
		currentNode = prevNode = null;
		SaveDada();
	}

	public void DismissCurrentNode() {
		if (currentNode == null) {
			return;
		}

		RemoveNodesConnexion(prevNode, currentNode);

		if (virtualNode != null) {
			currentNode.UpdateMesh();
			currentNode.UpdateHighlightColor(true);
			Destroy(virtualNode.gameObject);
		} else {
			RemoveNode(currentNode);
		}

		if (prevNode.ConnexionsCount == 0) {
			RemoveNode(prevNode);
		} else {
			prevNode.UpdateMesh();
			prevNode.UpdateHighlightColor(true);
		}
		prevNode = currentNode = null;
	}

	public void EraseAll() {
		for (int i = 0; i < nodes.Count; i++) {
			Destroy(nodes[i].gameObject);
		}
		nodes.Clear();
		SaveDada();
	}

	public void Demolish() {
		if (!GetRaycastPoint(out Vector3 point)) {
			return;
		}
		if (TryGetNearNode(point, out Node nearNode)) {
			if (nearNode.IsStaticNode) {
				return;
			}
			List<Node> nodeConnexions = new(nearNode.GetConnexions());
			for (int i = 0; i < nodeConnexions.Count; i++) {
				RemoveNodesConnexion(nearNode, nodeConnexions[i]);
				if (nodeConnexions[i].ConnexionsCount == 0) {
					RemoveNode(nodeConnexions[i]);
				} else {
					nodeConnexions[i].UpdateMesh();
				}
			}
			RemoveNode(nearNode);
			SaveDada();
		} else {
			for (int i = 0; i < nodes.Count; i++) {
				if (!nodes[i].IsHeadNode && nodes[i].HasConnectionBetween(point, out Node connexion) && !connexion.IsHeadNode) {
					RemoveNodesConnexion(nodes[i], connexion);
					if (nodes[i].ConnexionsCount == 0) {
						RemoveNode(nodes[i]);
					} else {
						nodes[i].UpdateMesh();
					}
					if (connexion.ConnexionsCount == 0) {
						RemoveNode(connexion);
					} else {
						connexion.UpdateMesh();
					}
					break;
				}
			}
			SaveDada();
		}
	}

	public bool IsMouseInputCloseToCurrentNode() {
		if (currentNode == null || !GetRaycastPoint(out Vector3 point)) {
			return false;
		}
		return Vector3.Distance(point, currentNode.transform.position) <= Config.Instance.RoadWidth;
	}

	private void CreateNodesConnexion(Node from, Node to) {
		from.Connect(to);
	}

	private void RemoveNodesConnexion(Node node0, Node node1) {
		node0.Disconnect(node1);
	}

	private bool HasConnectionBetween(Vector3 point, out Node node0, out Node node1) {
		node0 = node1 = default;
		for (int i = 0; i < nodes.Count; i++) {
			if (nodes[i].HasConnectionBetween(point, out node1)) {
				node0 = nodes[i];
				return true;
			}
		}
		return false;
	}

	private bool TryGetNearNode(Vector3 point, out Node nearNode) {
		nearNode = default;
		float nearNodeDist = float.MaxValue;
		for (int i = 0; i < nodes.Count; i++) {
			if (nodes[i].IsHeadNode) {
				continue;
			}
			float dist = Vector3.Distance(point, nodes[i].transform.position);
			if (dist < Config.Instance.RoadDoubleWidth && nearNodeDist > dist) {
				nearNode = nodes[i];
				nearNodeDist = dist;
			}
		}
		return nearNode != null;
	}

	private bool TryGetClosestIntersectionNode(out Node intersectionNode) {
		intersectionNode = default;
		float dist = 0f;

		for (int i = 0; i < nodes.Count; i++) {
			if (nodes[i] != currentNode && nodes[i] != prevNode && !nodes[i].IsHeadNode) {
				if (IsNodeBetweenOtherNodes(nodes[i], currentNode, prevNode)) {
					if (intersectionNode == null) {
						intersectionNode = nodes[i];
						dist = Vector3.Distance(prevNode.transform.position, intersectionNode.transform.position);
					} else {
						float d = Vector3.Distance(prevNode.transform.position, intersectionNode.transform.position);
						if (dist > d) {
							dist = d;
							intersectionNode = nodes[i];
						}
					}
				}
			}
		}

		return intersectionNode != null;
	}

	private bool IntersectAnyRestrictedArea(Node node0, Node node1) {
		for (int i = 0; i < pointOfInterests.Count; i++) {
			if (pointOfInterests[i].IntersectRestrictedArea(node0, node1)) {
				return true;
			}
		}
		for (int i = 0; i < restrictedAreas.Count; i++) {
			if (restrictedAreas[i].IntersectConnexion(node0, node1)) {
				return true;
			}
		}
		return false;
	}

	private static bool IsNodeBetweenOtherNodes(Node node, Node otherNode0, Node otherNode1) {
		Utils.PerpendicularPoints(otherNode0.transform.position, otherNode1.transform.position, out Vector3 p0, out Vector3 p1, Config.Instance.RoadWidth);
		Utils.PerpendicularPoints(otherNode1.transform.position, otherNode0.transform.position, out Vector3 p2, out Vector3 p3, Config.Instance.RoadWidth);

		Vector3[] points = new Vector3[5] {
			node.transform.position,
			node.transform.position + (Vector3.forward + Vector3.left) * Config.Instance.RoadWidth,
			node.transform.position + (Vector3.forward + Vector3.right) * Config.Instance.RoadWidth,
			node.transform.position + (Vector3.back + Vector3.left) * Config.Instance.RoadWidth,
			node.transform.position + (Vector3.back + Vector3.right) * Config.Instance.RoadWidth
		};

		return Utils.PolyContainsAnyPoint(p0, p1, p2, p3, points);
	}

	private bool ConnexionIntersectsOtherConnexion(Node node0, Node node1, out Vector3 intersection, out Node intNode0, out Node intNode1) {

		intersection = default;
		intNode0 = intNode1 = default;

		Vector3 from = node0.transform.position;
		Vector3 to = node1.transform.position + (node1.transform.position - from).normalized * Config.Instance.RoadHalfWidth;

		//Debug.DrawLine(from, to, Color.cyan);

		for (int i = 0; i < nodes.Count; i++) {
			intNode0 = nodes[i];
			if (intNode0.IsHeadNode || intNode0 == node0 || intNode0 == node1) {
				continue;
			}
			List<Node> connexions = nodes[i].GetConnexions();
			for (int j = 0; j < connexions.Count; j++) {
				intNode1 = connexions[j];
				if (intNode1.IsHeadNode || intNode1 == node0 || intNode1 == node1) {
					continue;
				}
				if (Utils.TryGetIntersection(out intersection, from, to, intNode0.transform.position, intNode1.transform.position)) {
					return true;
				}
			}
		}
		return false;
	}

	private Node NewNode(Vector3 point, bool addToList = true) {
		Node node = new GameObject($"node{(addToList ? nodes.Count : "_virtual")}_{transform.parent.name}").AddComponent<Node>();
		node.transform.parent = transform;
		node.transform.position = point;
		if (addToList) {
			nodes.Add(node);
		}
		return node;
	}

	private void RemoveNode(Node node) {
		Destroy(node.gameObject);
		nodes.Remove(node);
	}

	private bool GetRaycastPoint(out Vector3 point) {
		point = default;
		if (map.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, float.MaxValue)) {
			point = hit.point;
			point = new Vector3(Mathf.RoundToInt(point.x), point.y + Config.Instance.RoadHeight, Mathf.RoundToInt(point.z));
			//point = new Vector3((float)System.Math.Round(point.x / 5f) * 5,
			//	point.y + Config.Instance.RoadHeight,
			//	(float)System.Math.Round(point.z / 5f) * 5);
			return true;
		}
		return false;
	}

	private int prevRaycastX;
	private int prevRaycastZ;
	private bool GetNewRaycastPoint(out Vector3 point) {
		point = default;
		if (map.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, float.MaxValue)) {
			Vector3 hitPoint = hit.point;
			int x = Mathf.RoundToInt(hitPoint.x);
			int z = Mathf.RoundToInt(hitPoint.z);
			if (x != prevRaycastX || z != prevRaycastZ) {
				prevRaycastX = x;
				prevRaycastZ = z;
				point = new Vector3(x, point.y + Config.Instance.RoadHeight, z);
				return true;
			}

		}
		return false;
	}

	public void SaveDada() {
		NodeData[] nodesData = new NodeData[nodes.Count];
		for (int i = 0; i < nodesData.Length; i++) {
			nodesData[i] = new() {
				isStatic = nodes[i].IsStaticNode
			};
			if (!nodesData[i].isStatic) {
				nodesData[i].position = JSONVector3.FromVector3(nodes[i].transform.position);
			}
			if (otherLinkedNodes.Contains(nodes[i])) {
				nodesData[i].connexions = new int[0];
				continue;
			}
			List<Node> connexions = nodes[i].GetConnexions();
			nodesData[i].connexions = new int[connexions.Count];
			for (int j = 0; j < nodesData[i].connexions.Length; j++) {
				int index = nodes.IndexOf(connexions[j]);
				if (index != -1) {
					nodesData[i].connexions[j] = index;
				} else {
					Debug.LogError($"Index not found for {connexions[j]}");
				}
			}
			if (/*!nodesData[i].isStatic && */connexions.Count > 2) {
				NavigationPoint[] inputPoints = Config.Instance.RightDriving ?
					nodes[i].GetNavigationRightPoints() : nodes[i].GetNavigationLeftPoints();
				IntersectionData intersectionData = new();
				intersectionData.giveWayInputs = new bool[inputPoints.Length];
				for (int j = 0; j < intersectionData.giveWayInputs.Length; j++) {
					intersectionData.giveWayInputs[j] = inputPoints[j].GivesWay;
				}
				intersectionData.semaphore = nodes[i].GetSemaphoreData();
				nodesData[i].intersection = intersectionData;
			}
		}

		PlayerPrefs.SetString(dataSaveKey, Utils.Serialize(nodesData));
		PlayerPrefs.Save();
	}

	public void DeleteData() {
		PlayerPrefs.DeleteKey(DATA_SAVE_KEY + "_" + transform.parent.name);
	}

	private class NodeData {
		public bool isStatic;
		public JSONVector3 position;
		public int[] connexions;
		public IntersectionData intersection;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		for (int i = 0; i < pointOfInterests.Count; i++) {
			pointOfInterests[i].name = $"PointOfInterest{i}_{transform.parent.name}";
			pointOfInterests[i].HeadNode.name = $"node_static_head_{i}_{transform.parent.name}";
			pointOfInterests[i].OtherNode.name = $"node_static_{i}_{transform.parent.name}";
		}
		for (int i = 0; i < linkedNodes.Count; i++) {
			linkedNodes[i].name = $"linkedNode{i}_{transform.parent.name}";
			linkedNodes[i].UpdateDestinationMark();
		}
	}
#endif
}

public class IntersectionData {
	public bool[] giveWayInputs;
	public SemaphoreData semaphore;
}

public class SemaphoreData {
	public bool isOn;
	public int[] timers;
}
