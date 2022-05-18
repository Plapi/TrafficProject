using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour {

	[SerializeField] private BoxCollider map = default;

	private readonly List<Node> nodes = new();
	private readonly Node[] lastNodes = new Node[3];

	private void Update() {

		if (!GetRaycastPoint(out Vector3 point)) {
			return;
		}

		if (lastNodes[0] != null) {
			lastNodes[0].transform.position = point;

			bool isAcceptedDistance = Vector3.Distance(lastNodes[0].transform.position, lastNodes[1].transform.position) >= Config.Instance.RoadWidth;

			if (isAcceptedDistance && lastNodes[1].ConnexionsCount == 2) {
				float angle = Utils.GetAngle(lastNodes[0].transform.position, lastNodes[1].transform.position, lastNodes[2].transform.position);
				isAcceptedDistance = Mathf.Abs(angle) > 50;
			}

			lastNodes[0].UpdateHighlightColor(isAcceptedDistance);
			if (lastNodes[2] == null) {
				lastNodes[1].UpdateHighlightColor(isAcceptedDistance);
			}

			if (Input.GetMouseButtonDown(1)) {
				lastNodes[0].RemoveConnexion(lastNodes[1]);
				RemoveNode(lastNodes[0]);
				if (lastNodes[1].ConnexionsCount == 0) {
					RemoveNode(lastNodes[1]);
				} else {
					lastNodes[1].UpdateMesh();
					lastNodes[1].UpdateHighlightColor(true);
				}
				for (int i = 0; i < lastNodes.Length; i++) {
					lastNodes[i] = null;
				}
				return;
			}

			lastNodes[0].UpdateMesh();
			lastNodes[1].UpdateMesh();

			if (!isAcceptedDistance) {
				return;
			}
		}

		if (Input.GetMouseButtonDown(0)) {
			if (lastNodes[0] == null) {
				if (TryGetNearNode(point, out Node nearNode)) {
					lastNodes[1] = nearNode;
					lastNodes[2] = nearNode.GetConnexion();
				} else {
					lastNodes[1] = NewNode(point);
				}
			} else {
				for (int i = lastNodes.Length - 1; i > 0; i--) {
					lastNodes[i] = lastNodes[i - 1];
				}
			}
			lastNodes[0] = NewNode(point);
			lastNodes[0].AddConnexion(lastNodes[1]);
			return;
		}
	}

	private void RemoveNode(Node node) {
		Destroy(node.gameObject);
		nodes.Remove(node);
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
