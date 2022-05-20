using System.Collections.Generic;
using UnityEngine;
using Poly2Tri;

public class NodeController : MonoBehaviour {

	[SerializeField] private BoxCollider map = default;

	private readonly List<Node> nodes = new();

	private Node prevNode;
	private Node currentNode;
	private Node virtualNode;

	private void Update() {

		if (!GetRaycastPoint(out Vector3 point)) {
			return;
		}

		if (currentNode != null) {
			if (virtualNode == null) {
				if (TryGetClosestIntersectionNode(out Node intersectionNode) && !prevNode.HasConnexion(intersectionNode)) {
					prevNode.RemoveConnexion(currentNode);
					RemoveNode(currentNode);

					currentNode = intersectionNode;
					currentNode.AddConnexion(prevNode);

					virtualNode = NewNode(point, false);
				} else {
					currentNode.transform.position = point;
				}
			} else {
				virtualNode.transform.position = point;
				if (!IsNodeBetweenOtherNodes(currentNode, virtualNode, prevNode)) {
					currentNode.RemoveConnexion(prevNode);
					currentNode.UpdateMesh();
					currentNode.UpdateHighlightColor(true);
					Destroy(virtualNode.gameObject);

					currentNode = NewNode(point);
					currentNode.AddConnexion(prevNode);
				}
			}

			bool canBePlaced = prevNode.HasAcceptedDistance(currentNode) &&
				prevNode.HasAcceptedAngle(currentNode) && currentNode.HasAcceptedAngle(prevNode);

			currentNode.UpdateHighlightColor(canBePlaced);
			prevNode.UpdateHighlightColor(canBePlaced);

			if (Input.GetMouseButtonDown(1)) {
				currentNode.RemoveConnexion(prevNode);

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
				return;
			}

			currentNode.UpdateMesh();
			prevNode.UpdateMesh();

			if (!canBePlaced) {
				return;
			}
		}

		if (Input.GetMouseButtonDown(0)) {
			if (virtualNode != null) {
				Destroy(virtualNode.gameObject);
				virtualNode = null;
			}

			if (currentNode == null) {
				if (TryGetNearNode(point, out Node nearNode)) {
					prevNode = nearNode;
				} else if (HasConnectionBetween(point, out Node node0, out Node node1)) {
					point = Utils.GetClosestPointOnLine(point, node0.transform.position, node1.transform.position);
					node0.RemoveConnexion(node1);
					prevNode = NewNode(point);
					prevNode.AddConnexion(node0);
					prevNode.AddConnexion(node1);
					node0.UpdateMesh();
					node1.UpdateMesh();
				} else {
					prevNode = NewNode(point);
				}
			} else {
				prevNode = currentNode;
			}
			currentNode = NewNode(point);
			currentNode.AddConnexion(prevNode);
			return;
		}
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
		for (int i = 0; i < nodes.Count; i++) {
			if (Vector3.Distance(point, nodes[i].transform.position) < Config.Instance.RoadWidth) {
				nearNode = nodes[i];
				return true;
			}
		}
		return false;
	}

	private bool TryGetClosestIntersectionNode(out Node intersectionNode) {
		intersectionNode = default;
		float dist = 0f;

		for (int i = 0; i < nodes.Count; i++) {
			if (nodes[i] != currentNode && nodes[i] != prevNode) {
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

	private static bool IsNodeBetweenOtherNodes(Node node, Node otherNode0, Node otherNode1) {
		Utils.PerpendicularPoints(otherNode0.transform.position, otherNode1.transform.position, out Vector3 p0, out Vector3 p1, Config.Instance.RoadHalfWidth);
		Utils.PerpendicularPoints(otherNode1.transform.position, otherNode0.transform.position, out Vector3 p2, out Vector3 p3, Config.Instance.RoadHalfWidth);

		Vector3[] points = new Vector3[5] {
			node.transform.position,
			node.transform.position + (Vector3.forward + Vector3.left) * Config.Instance.RoadHalfWidth,
			node.transform.position + (Vector3.forward + Vector3.right) * Config.Instance.RoadHalfWidth,
			node.transform.position + (Vector3.back + Vector3.left) * Config.Instance.RoadHalfWidth,
			node.transform.position + (Vector3.back + Vector3.right) * Config.Instance.RoadHalfWidth
		};

		return Utils.PolyContainsAnyPoint(p0, p1, p2, p3, points);
	}

	private Node NewNode(Vector3 point, bool addAtList = true) {
		Node node = new GameObject($"node{nodes.Count}").AddComponent<Node>();
		node.transform.parent = transform;
		node.transform.position = point;
		if (addAtList) {
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
}
