using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour {

	[SerializeField] private BoxCollider map = default;

	private readonly List<Node> nodes = new();

	private Node prevNode;
	private Node currentNode;

	private void Update() {

		if (!GetRaycastPoint(out Vector3 point)) {
			return;
		}

		if (currentNode != null) {
			currentNode.transform.position = point;

			bool canBePlaced = prevNode.HasAcceptedDistance(currentNode) && prevNode.HasAcceptedAngle(currentNode);

			currentNode.UpdateHighlightColor(canBePlaced);
			prevNode.UpdateHighlightColor(canBePlaced);

			if (Input.GetMouseButtonDown(1)) {
				currentNode.RemoveConnexion(prevNode);
				RemoveNode(currentNode);
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

	private Node NewNode(Vector3 point) {
		Node node = new GameObject($"node{nodes.Count}").AddComponent<Node>();
		node.transform.parent = transform;
		node.transform.position = point;
		nodes.Add(node);
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
}
